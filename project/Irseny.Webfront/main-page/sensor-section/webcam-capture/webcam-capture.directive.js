function WebcamCaptureController($scope, MessageLog, LiveWireService, LiveExchangeService, TaskScheduleService) {
	var self = this;
	var videoImage = undefined;
	var videoMetadata = {}; // initialized below

	var videoSubscription = undefined;

	this.$onInit = function() {
		videoSubscription = LiveExchangeService.getObserver("sensorCapture").subscribe(self.receiveVideoCapture);

		videoMetadata = {
			frameWidth: 160,
			frameHeight: 120,
			frameRate: 30,
			frameTime: 33,
			frameTimeDeviation: 0
		};
	};

	this.$onDestroy = function() {
		if (videoSubscription != undefined) {
			LiveExchangeService.getObserver("sensorCapture").unsubscribe(self.videoSubscription);
		}
	};
	/**
	 * Requests video capture access of the active sensor through LiveWire.
	 * @param {boolean} includeImage indicates whether to additionally request captured images
	 * @return {boolean} indicates whether the request was sent out
	 */
	this.requestVideoCapture = function(includeImage) {
		var sensor = self.shared.getActiveSensor();
		if (sensor.index < 0) {
			return false;
		}
		var subject = {
			type: "get",
			topic: "sensorCapture",
			position: sensor.index,
			data: [{
				timeout: 30000,
				includeImage: true
			}]
		};
		var future = LiveWireService.sendRequest(subject);
		if (future == undefined) {
			videoImage = undefined;
			return false;
		}
		future.then(function(response) {
			console.log("Webcan sample query succes");

		});
		future.reject(function(response) {
			MessageLog.logError("Webcam sample query failed");
			videoImage = undefined;
		});
		return true;
	};
	/**
	 * Receives and processes a captured video frame.
	 * @param {Object} capture captured video frame
	 */
	this.receiveVideoCapture = function(capture) {

		if (typeof capture.data.image == "string") {
			videoImage = capture.data.image;
		}
		videoMetadata = {};
		for (var prop of ["frameWidth", "frameHeight", "frameRate", "frameTime", "frameTimeDeviation"]) {
			if (Number.isInteger(capture.data[prop])) {
				videoMetadata[prop] = capture.data[prop];
			}
		}
		TaskScheduleService.addTimeout("digest", 100, function() {
			$scope.$digest;
		});

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
		sensor.data.capturing = true;
		self.shared.exchangeSensor(sensor);
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
		sensor.data.capturing = false;
		self.shared.exchangeSensor(sensor);
		return true;
	};
	/**
	 * Starts or stops capturing with the active webcam depending on the current state.
	 * Sets the capturing property and exchanges the current configuration with the server.
	 */
	this.toggleCapture = function() {
		var sensor = self.shared.getActiveSensor();
		if (sensor.index < 0) {
			return;
		}
		if (sensor.data.capturing) {
			sensor.data.capturing = false;
		} else {
			sensor.data.capturing = true;
		}
		self.shared.exchangeSensor(sensor);
		// TODO send as request and process return info
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
