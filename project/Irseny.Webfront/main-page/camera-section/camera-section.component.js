function CameraSectionController(LiveExchangeService) {
	this.cameraClasses = [ "Webcam", "MS Kinect", "Camcorder" ];
	this.availableCameras = [ "LiveVision0", "Kinect1", "Logitech UltraMegaPro2" ];
	this.activeCamera = "Kinect1";




	this.cameraUpdated = function(update) {
		console.log("updated camera to ".concat(JSON.stringify(update)));
	};

	LiveExchangeService.observe("camera").subscribe(this.cameraUpdated);
	LiveExchangeService.touch();

}

var module = angular.module("cameraSection");

module.component("cameraSection", {
	templateUrl : "main-page/camera-section/camera-section.template.html",
	controller : CameraSectionController
});

CameraSectionController.$inject = ["LiveExchangeService"];


