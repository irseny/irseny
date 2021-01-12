function WebcamCaptureController($scope, MessageLog, LiveWireService, LiveExchangeService, TaskScheduleService) {
	const STATUS_FLAG_CLEAR = 0x0;
	const STATUS_FLAG_STOPPED = 0x0;
	const STATUS_FLAG_WORKING = 0x1;
	const STATUS_FLAG_RUNNING = 0x2;
	const STATUS_FLAG_ERROR = 0x3;

	var self = this;
	var videoImage = undefined; // compressed last live image string
	var videoMetadata = {}; // initialized below

	var liveSubscription = undefined; // subscription to receive data from LiveExchange
	var videoStatusFlag = 0x0; // 0 stopped, 1 waiting, 2 running, 3 error
	var captureStatusFlag = 0x0; // 0 clear, 1 waiting, 2 running, 3 error


	this.$onInit = function() {
		liveSubscription = LiveExchangeService.getObserver("sensorCapture").subscribe(receiveVideoCapture);

		videoMetadata = {
			frameWidth: 160,
			frameHeight: 120,
			frameRate: 30,
			frameTime: 33,
			frameTimeDeviation: 0
		};
		self.subscribeToCapture();
		// react to changes
	};

	this.$onDestroy = function() {
		if (liveSubscription != undefined) {
			LiveExchangeService.getObserver("sensorCapture").unsubscribe(liveSubscription);
		}
		self.unsubscribeFromCapture();
	};
	/**
	 * Helper function to schedule a digest call.
	 * @param {Number} timeout time to wait before calling digest
	 */
	function scheduleDigest(timeout=100) {
		TaskScheduleService.addTimeout("digest", 100, function() {
			$scope.$digest();
		});
	}
	/**
	 * Returns the video status flags.
	 * @return {Number} video status flags
	 */
	this.getVideoStatus = function() {
		return videoStatusFlag;
	};
	/**
	 * Returns the capture status.
	 * @return {Number} capture status flags
	 */
	this.getCaptureStatus = function() {
		return captureStatusFlag;
	};
	/**
	 * Subscribes the client to receive webcam capture from the server.
	 * A single subscription is active for the given timespan.
	 * The subscription is then automatically renewed which is roughly the case
	 * when the server automatically cancels the previous subscription.
	 * The subscription for this instance to receive data from LiveExchangeService
	 * is handled in the controller initialization.
	 * @param {boolean} includeImage indicates whether to also request image data from the server
	 * @param {Number} timeout timespan after which the subscription times out
	 */
	this.subscribeToCapture = function(includeImage=false, timeout=30000) {
		requestVideoCapture(includeImage);
		TaskScheduleService.addTimeout("webcamCapture", timeout, function() {
			// when the timer runs out, resubscribe
			// unsubscribe from video data by default
			self.subscribeToCapture(false, timeout);
		});
	}
	/**
	 * Unsubscribes the client from receiving webcam capture from the server.
	 * This only cancels any resubscription after timeout so that the server
	 * is likely to send captured data for a few more seconds.
	 */
	this.unsubscribeFromCapture = function() {
		// the server will end the subscription by itself after timeout
		// we just do not want any more subscription calls
		TaskScheduleService.cancelTimeout("webcamCapture");
	};
	/**
	 * Requests video capture access of the active sensor through LiveWire.
	 * @param {boolean} includeImage indicates whether to additionally request captured images
	 * @return {boolean} indicates whether the request was sent out
	 */
	function requestVideoCapture(includeImage=true, timeout=30000) {
		var sensor = self.shared.getActiveSensor();
		if (sensor.index < 0) {
			return false;
		}
		var subject = {
			type: "get",
			topic: "sensorCapture",
			position: sensor.index,
			data: [{
				timeout: timeout,
				includeImage: includeImage
			}]
		};

		if (includeImage) {
			videoStatusFlag = 0x1;
		} else {
			videoStatusFlag = 0x0;
		}
		var future = LiveWireService.sendRequest(subject);
		if (future == undefined) {
			videoImage = undefined;
			return false;
		}
		future.then(function(response) {
			// nothing to do
			// react when the first actual frame arrives
		});
		future.catch(function(response) {
			if (includeImage) {
				videoImage = undefined;
				videoStatusFlag = 0x3;
			}
			scheduleDigest();
		});
		return true;
	};


	/**
	 * Receives and processes a captured video frame.
	 * @param {Object} capture captured video frame including metadata
	 */
	function receiveVideoCapture(capture) {
		// look for an optional image in the data
		var realtime = false;
		if (typeof capture.data.image == "string") {
			videoImage = capture.data.image;
			videoStatusFlag = 0x2;
			realtime = true;
		} else {
			videoImage = undefined;
			videoStatusFlag = 0x0;
		}
		// look for additional metadata
		videoMetadata = {};
		const properties = ["frameWidth", "frameHeight", "frameRate", "frameTime", "frameTimeDeviation"];
		for (var prop of properties) {
			if (Number.isInteger(capture.data[prop])) {
				videoMetadata[prop] = capture.data[prop];
			}
		}
		//scheduleDigest(0);
		$scope.$digest();
	};
	/**
	 * Gets the latest captured video image.
	 * @return {string} captured video or undefined if nothing was captured
	 */
	this.getVideoImage = function() {
		return videoImage;
	};
	/**
	 * Gets the latest video metadata.
	 * @return {Object} video metadata
	 */
	this.getVideoMetadata = function() {
		return videoMetadata;
	};
	/**
	 * Sets the active webcam to capturing and exchanges it with the server.
	 * @return {boolean} indicates whether the operation was successful
	 */
	this.startCapture = function() {
		var sensor = self.shared.getActiveSensor();
		if (sensor.index < 0) {
			return false;
		}
		captureStatusFlag = 0x1;
		sensor.data.capturing = true;
		var future = self.shared.exchangeSensor(sensor);
		if (future == undefined) {
			return false;
		}
		future.then(function() {
			captureStatusFlag = 0x2;
			scheduleDigest();
		});
		future.catch(function() {
			captureStatusFlag = 0x3;
			self.shared.resetSensor(sensor);
			scheduleDigest();
		});
		return true;
	};
	/**
	 * Sets the active webcam to not capturing and exchanges it with the server.
	 * @return {boolean} indicates whether the operation was successful
	 */
	this.stopCapture = function() {
		var sensor = self.shared.getActiveSensor();
		if (sensor.index < 0) {
			return false;
		}
		captureStatusFlag = 0x1;
		sensor.data.capturing = false;
		var future = self.shared.exchangeSensor(sensor);
		if (future == undefined) {
			return false;
		}
		future.then(function() {
			captureStatusFlag = 0x0;
			videoStatusFlag = 0x0;
			scheduleDigest();
		});
		future.catch(function() {
			captureStatusFlag = 0x3;
			self.shared.resetSensor(sensor);
			scheduleDigest();
		});
		return true;
	};
	/**
	 * Starts or stops capturing with the active webcam depending on the current state.
	 * Sets the capturing property and exchanges the current configuration with the server.
	 * @return {boolean} indicates whether the operation finished successfully
	 */
	this.toggleCapture = function() {
		var sensor = self.shared.getActiveSensor();
		if (sensor.index < 0) {
			return false;
		}
		if (sensor.data.capturing) {
			return self.stopCapture();
		} else {
			return self.startCapture();
		}

	};
}
WebcamCaptureController.$inject = ["$scope", "MessageLog", "LiveWireService", "LiveExchangeService", "TaskScheduleService"];

var module = angular.module("webcamCapture");
module.directive("webcamCapture", function() {
	return {
		restrict: 'E',
		require: {
			shared: "^sensorSection"
		},
		bindToController: true,
		controller: WebcamCaptureController,
		controllerAs: "$ctrl",
		scope: true,
		templateUrl: "main-page/sensor-section/webcam-capture/webcam-capture.template.html"
	};
});
