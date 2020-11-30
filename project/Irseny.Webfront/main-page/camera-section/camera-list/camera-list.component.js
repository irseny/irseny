function CameraListController() {

	this.addCamera = function(cameraType) {
		console.log("adding camera " + cameraType);
	};
	this.removeCamera = function() {
		console.log("removing selected camera");
	};
}
var module = angular.module("cameraList", []);
module.component("cameraList", {
	templateUrl : "main-page/camera-section/camera-list/camera-list.template.html",
	controller : CameraListController,
	bindings : {
		base : "=base"
	}
});
