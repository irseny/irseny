function CameraListController() {
	this.availableCameras = [ "Camera0", "Camera1", "Camera2" ];
	this.activeCamera = "Camera1";
}

var module = angular.module("cameraList");

module.component("cameraList", {
	templateUrl : "main-page/camera-list/camera-list.template.html",
	controller : CameraListController
});


