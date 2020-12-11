
function CameraListController() {
	var classes = ["james", "amos", "praxideke"];
	this.$onInit = function() {
		console.log("list initializing");
	};
	this.getCameraClasses = function() {
		return classes;
	};
}

var module = angular.module("cameraList");
/*module.component("cameraList", {
	require : {
		shared: "^cameraSection"
	},
	templateUrl : "main-page/camera-section/camera-list/camera-list.template.html",
	controller : function() {}
});*/

module.directive("cameraList", function() {
	return {
		restrict: 'E',
		require: {
			shared: "^cameraSection"
		},
		bindToController: true,
		controller: CameraListController,
		controllerAs: "$ctrl",
		templateUrl : "main-page/camera-section/camera-list/camera-list.template.html",
	};
});
