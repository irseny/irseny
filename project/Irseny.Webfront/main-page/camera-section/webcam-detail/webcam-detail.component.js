function WebcamDetailController() {

	this.$onInit = function() {
		console.log("WebcamDetailController got " + this.cameraId);
	};
}

var module = angular.module("webcamDetail");
module.component("webcamDetail", {
	templateUrl : "main-page/camera-section/webcam-detail/webcam-detail.template.html",
	controller : WebcamDetailController,
	bindings : {
		cameraId : "=camera"
	}
});
