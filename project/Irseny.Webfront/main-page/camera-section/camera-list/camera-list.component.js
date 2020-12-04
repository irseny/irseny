
var module = angular.module("cameraList");
module.component("cameraList", {
	require : {
		shared: "^cameraSection"
	},
	templateUrl : "main-page/camera-section/camera-list/camera-list.template.html",
	controller : function() {}
});
