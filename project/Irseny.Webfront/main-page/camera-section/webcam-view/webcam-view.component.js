function WebcamViewController($scope) {
	var self = this;
	var videoSource = undefined;

	this.$onInit = function() {

	};

	this.$onDestroy = function() {

	};
	this.requestVideoSource = function() {
		var future = shared.requestActiveSensorCapture();
		if (future != undefined) {
			future.then(function(response) {
				if (response.status != 200) {
					console.log("server has no webcam video");
					return;
				}
				if (!(response.data.length > 0)) {
					console.log("server did not send webcam video");
					return;
				}
				videoSource = response.data[0];
				$scope.$digest();
			});
		}
	};
	this.getVideoSource = function() {
		return videoSource;
	};
	this.startCapture = function() {
		var sensor = self.shared.getActiveCamera();
		if (sensor.index < 0) {
			return;
		}
		sensor.data.capturing = true;
		self.shared.exchangeActiveSensor();
	};
	this.stopCapture = function() {
		var sensor = self.shared.getActiveCamera();
		if (sensor.index < 0) {
			return;
		}
		sensor.data.capturing = false;
		self.shared.exchangeActiveSensor();
	};

}
WebcamViewController.$inject = ["$scope"];
var module = angular.module("webcamView");

module.directive("webcamView", function() {
	return {
		restrict: 'E',
		require: {
			shared: "^cameraSection"
		},
		bindToController: true,
		controller: WebcamViewController,
		controllerAs: "$ctrl",
		templateUrl: "main-page/camera-section/webcam-view/webcam-view.template.html"
	};
});

