function WebcamDetailController() {
	var aspectRatio = "4:3";
	var resolution = { width: 800, height: 600 };
	var resolutionConfigMode = "list";
	var aspectRatios = [
		"4:3",
		"16:9"
	];
	var resolutions = {
		"4:3": [
			{ width: 160, height: 120 },
			{ width: 320, height: 240 },
			{ width: 800, height: 600 },
			{ width: 1024, height: 768 },
			{ width: 1280, height: 960 },
			{ width: 1440, height: 1080 }
		],
		"16:9": [
			{ width: 1280, height: 720 },
			{ width: 1440, height: 810 },
			{ width: 1600, height: 900 }
		]
	};
	this.getTypicalAspectRatios = function() {
		return aspectRatios;
	};
	this.getTypicalResolutions = function(aspectRatio) {
		return resolutions[aspectRatio];
	};
	this.getAspectRatio = function() {
		return aspectRatio;
	};
	this.setAspectRatio = function(ratio) {
		switch (ratio) {
		case "4:3":
			aspectRatio = "4:3";
		break;
		case "16:9":
			aspectRatio = "16:9";
		break;
		}
	};
	this.getResolution = function() {
		return resolution;
	};
	this.setResolution = function(res) {
		if (!Number.isInteger(res.width) || !Number.isInteger(res.height)) {
			return;
		}
		resolution = { width: res.width, height: res.height };
	};
	this.setResolutionMode = function(mode) {
		if (mode == resolutionConfigMode) {
			return;
		}
		if (mode == "list") {
			var ratio = resolutions.find(function(list) {
				var res = list.find(function(r) {
					return r.width == resolution.width && r.height == resolution.height;
				});
				if (res != undefined) {
					resolution = { width: res.width, height: res.height };
					return true;
				}
				return false;
			});
			resolutionConfigMode = "list";
			if (ratio != undefined) {
				aspectRatio = ratio;
			}
		}
		if (mode == "custom") {
			resolutionConfigMode = "custom";
		}
	};
	this.$onInit = function() {

	};

	this.$onDestroy = function() {

	};
}

var module = angular.module("webcamDetails");
module.directive("webcamDetails", function() {
	return {
		restrict: 'E',
		require: {
			shared: "^sensorSection"
		},
		bindToController: true,
		controller: WebcamDetailController,
		controllerAs: "$ctrl",
		scope: true,
		templateUrl: "main-page/sensor-section/webcam-details/webcam-details.template.html"
	};
});
