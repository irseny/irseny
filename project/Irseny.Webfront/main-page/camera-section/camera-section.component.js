function CameraSectionController() {
	this.cameraClasses = [ "Webcam", "MS Kinect", "Camcorder" ];
	this.availableCameras = [ "LiveVision0", "Kinect1", "Logitech UltraMegaPro2" ];
	this.activeCamera = "Kinect1";

}

var module = angular.module("cameraSection");

module.component("cameraSection", {
	templateUrl : "main-page/camera-section/camera-section.template.html",
	controller : CameraSectionController
});


