function WebcamCaptureController($scope, MessageLog, LiveWireService) {
	var self = this;
	var videoSource = undefined;

	this.$onInit = function() {

	};

	this.$onDestroy = function() {

	};
	this.beginRequestVideoSource = function() {
		var sensor = self.shared.getActiveSensor();
		if (sensor.index < 0) {
			return false;
		}
		var subject = {
			type: "get",
			topic: "sensorCapture",
			position: sensor.index,
		};
		var future = LiveWireService.requestUpdate(subject);
		if (future == undefined) {
			videoSource = undefined;
			return false;
		}
		future.then(function(response) {
			if (!(response.data.length > 0)) {
				MessageLog.logError("Webcam sample has no data");
				videoSource = undefined;
				self.endRequestVideoSource();
				return;
			}
			videoSource = response.data[0].image;
			self.endRequestVideoSource();

		});
		future.reject(function(response) {
			MessageLog.logError("Webcam sample query failed");
			videoSource = undefined;
			self.endRequestVideoSource();
		});
	};
	this.endRequestVideoSource = function() {
		$scope.$digest();
	};
	this.getVideoSource = function() {
		return videoSource;
	};
	this.startCapture = function() {
		var sensor = self.shared.getActiveSensor();
		if (sensor.index < 0) {
			return;
		}
		sensor.data.capturing = true;
		self.shared.exchangeSensor(sensor);
	};
	this.stopCapture = function() {
		var sensor = self.shared.getActiveSensor();
		if (sensor.index < 0) {
			return;
		}
		sensor.data.capturing = false;
		self.shared.exchangeSensor(sensor);
	};
}
WebcamCaptureController.$inject = ["$scope", "MessageLog", "LiveWireService"];

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
