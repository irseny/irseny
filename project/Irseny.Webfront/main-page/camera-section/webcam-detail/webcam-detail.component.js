function WebcamDetailController(LiveWireService) {
	var self = this;
	this.framerate = 30;
	this.exposure = 0.0;
	this.imageResolutionConfigMode = "selectTypical";
	this.imageResolution = {
		x: 320,
		y: 240
	};
	this.aspectRatio = "4:3";
	this.typicalAspectRatios = [
		"1:1", "3:2", "4:3", "5:4", "16:9"
	];
	this.typicalResolutions = {
		"1:1": [
			{ x: 100, y: 100 },
			{ x: 200, y: 200 }
		],
		"3:2": [
			{ x: 300, y: 200}
		],
		"4:3": [
			{ x: 320, y: 240 },
			{ x: 640, y: 480 },
			{ x: 800, y: 600 }
		],
		"5:4": [
			{ x: 500, y: 400},
			{ x: 1000, y:800}
		],
		"16:9": [
			{ x: 1280, y: 720 },
			{ x: 1920, y: 1080 }
		]
	};
	this.fallbackResolution = [];




	this.$onInit = function() {

		if (self.shared == undefined) {
			console.log("nothing shared in init");
		} else {
			//console.log("WebcamDetailController got " + shared.getActiveCamera().data.name);
			console.log("shared in init");
		}
	};
	this.$postLink = function() {
		if (self.shared == undefined) {
			console.log("nothing shared in postLink");
		} else {
			console.log("shared in postLink");
		}

	};
	this.switchImageSizeConfigMode = function(switchFor) {
		console.log("switching to mode " + switchFor);
	};
	this.selectAspectRatio = function(aspectRatio) {

	};
	this.updateImageSize = function() {
		console.log("image size changed");
	};
	this.setAspectRatio = function(aspectRatio) {

	};
	this.imageResolutionAdvancedConfig = function() {
		return this.imageResolutionConfigMode == 'enterCustom';
	};
	this.getResolutionSuggestions = function() {
		if (this.typicalResolutions[this.aspectRatio] == null) {
			return this.fallbackResolution;
		}
		if (this.typicalResolutions[this.aspectRatio].length == 0) {
			return this.fallbackResolution;
		}
		return this.typicalResolutions[this.aspectRatio];
	};
	this.typicalResolutionConfig = function(value) {

	};
	this.customResolutionConfig = function(value) {
		if (arguments.length > 0) {
			this.imageSizeConfig = value ? 'enterCustom' : 'selectTypical';
		} else {
			return this.imageSizeConfig == 'enterCustom';
		}
	};
	this.typicalResolutionConfig = function(value) {
		if (arguments.length > 0) {
			this.imageSizeConfig = value ? 'selectTypical' : 'enterCustom';
		} else {
			return this.imageSizeConfig == 'selectTypical';
		}
	};
	this.applyChanges = function() {
		console.log("applying changes");
		LiveWire.sendMessage("live changes applied");
	};
	this.resetChanges = function() {
		console.log("resetting changes");
	};
	this.$onDestroy = function() {
		console.log("webcam details destroyed");
	};
}

var module = angular.module("webcamDetail");
module.component("webcamDetail", {
	require: {
		shared: "^cameraSection"
	},
	controller: WebcamDetailController,
	templateUrl: "main-page/camera-section/webcam-detail/webcam-detail.template.html"
});

/*module.directive("webcamDetail", function() {
	return {
		restrict: 'E',
		require: {
			shared: "^cameraSection",
			self: "webcamDetail"
		},
		link: function(scope, element, attrs, required) {
			console.log("directive link");
			required.self.shared = required.shared;
			//console.log("link got camera " + required.shared.getActiveCamera().data.name);
		},
		controller: WebcamDetailController,
		bindToController: true
	};
});*/
WebcamDetailController.$inject = ["LiveWireService"];
