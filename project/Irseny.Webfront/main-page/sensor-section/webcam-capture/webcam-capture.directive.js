function WebcamCaptureController($scope, MessageLog, LiveWireService, LiveExchangeService) {
	var self = this;
	var videoSource = undefined;
	var videoSize = {
		width: 0,
		height: 0
	};
	var videoSubscription = undefined;

	this.$onInit = function() {
		videoSubscription = LiveExchangeService.getObserver("sensorCapture").subscribe(self.receiveVideoSource);
	};

	this.$onDestroy = function() {
		if (videoSubscription != undefined) {
			LiveExchangeService.getObserver("sensorCapture").unsubscribe(self.videoSubscription);
		}
	};
	this.requestVideoSource = function() {
		var sensor = self.shared.getActiveSensor();
		if (sensor.index < 0) {
			return false;
		}
		var subject = {
			type: "get",
			topic: "sensorCapture",
			position: sensor.index,
		};
		var future = LiveWireService.sendRequest(subject);
		if (future == undefined) {
			videoSource = undefined;
			return false;
		}
		future.then(function(response) {
			console.log("Webcan sample query succes");

		});
		future.reject(function(response) {
			MessageLog.logError("Webcam sample query failed");
			videoSource = undefined;
		});
	};
	this.receiveVideoSource = function(capture) {

		if (capture.data.image == undefined) {
			MessageLog.logWarning("Webcam sample has no image");
		} else {
			videoSource = capture.data.image;
			videoSize = {
				width: capture.data.width,
				height: capture.data.height
			};
			$scope.$digest();
		}
	};
	this.getVideoSource = function() {
		return videoSource;
	};
	this.getVideoSize = function() {
		return videoSize;
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
}
WebcamCaptureController.$inject = ["$scope", "MessageLog", "LiveWireService", "LiveExchangeService"];

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
