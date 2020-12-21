/**
 * Constructs a controller for webcam settings customization.
 *
 * @constructor
 */
function WebcamDetailController() {
	var self = this;
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
	/**
	 * Returns all typically used aspect ratios.
	 * @return {Array} aspect ratio strings
	 */
	this.getTypicalAspectRatios = function() {
		return aspectRatios;
	};
	/**
	 * Returns all typical resolutions for the given aspect ratio.
	 * @param {string} ratio resolution filter
	 * @return {Array} resolutions with width and height properties
	 */
	this.getTypicalResolutions = function(ratio) {
		if (typeof ratio != "string") {
			throw Error("ratio");
		}
		return resolutions[ratio];
	};
	/**
	 * Returns the currently selected aspect ratio.
	 * @return {string} aspect ratio
	 */
	this.getAspectRatio = function() {
		return aspectRatio;
	};
	/**
	 * Sets the currently selected aspect ratio.
	 * @param {string} ratio
	 */
	this.setAspectRatio = function(ratio) {
		switch (ratio) {
		case "4:3":
			aspectRatio = "4:3";
		break;
		case "16:9":
			aspectRatio = "16:9";
		break;
		default:
		throw Error("ratio");
		}
	};
	/*this.getResolution = function() {
		return resolution;
	};
	this.setResolution = function(res) {
		if (!Number.isInteger(res.width) || !Number.isInteger(res.height)) {
			return;
		}
		resolution = { width: res.width, height: res.height };
	};*/

	/**
	 * Sets the active resolution configuration mode.
	 * @param {string} mode valid values are "list" and "custom"
	 */
	this.setResolutionMode = function(mode) {
		if (mode == resolutionConfigMode) {
			return;
		}
		if (mode == "list") {
			var ratio = resolutions.find(function(list) {
				var res = list.find(function(r) {
					return r.width == resolution.width && r.height == resolution.height;
				});
				/*if (res != undefined) {
					resolution = { width: res.width, height: res.height };
					return true;
				}*/
				return false;
			});
			resolutionConfigMode = "list";
			if (ratio != undefined) {
				aspectRatio = ratio;
			}
		} else if (mode == "custom") {
			resolutionConfigMode = "custom";
		} else {
			throw new Error("mode");
		}
	};
	/**
	 * Returns the active resolution configuration mode.
	 * @return {string} possible values are "list" and "custom"
	 */
	this.getResolutionMode = function() {
		return resolutionConfigMode;
	};
	/**
	 * Returns the given property of the active sensor.
	 * @param {string} prop property name
	 * @return {number} if the property is set, otherwise {undefined}
	 */
	this.getWebcamProperty = function(prop) {
		if (typeof prop != "string") {
			throw Error("prop");
		}
		var webcam = self.shared.getActiveSensor();
		if (webcam.index >= 0) {
			return webcam.data[prop];
		}
		return undefined;
	};
	/**
	 * Indicates whether the given property of the active sensor
	 * is supposed to adjust automatically.
	 * @param {string} prop property name
	 * @return {bool} false if the property is set, otherwise false
	 */
	this.getWebcamPropertyAuto = function(prop) {
		if (typeof prop != "string") {
			throw Error("prop");
		}
		var webcam = self.shared.getActiveSensor();
		if (webcam.index >= 0) {
			return webcam.data[prop] != undefined;
		}
		return true;
	};
	/**
	 * Sets the given property of the active sensor.
	 * @param {string} prop property name
	 * @param {number} value property value
	 */
	this.setWebcamProperty = function(prop, value) {
		if (typeof prop != "string") {
			throw Error("prop");
		}
		if (!Number.isInteger(value)) {
			throw Error("value");
		}
		var webcam = self.shared.getActiveSensor();
		if (webcam.index >= 0) {
			webcam.data[prop] = value;
		}
	};
	/**
	 * Unsets the given property of the active sensor.
	 * @param {string} prop property name
	 */
	this.setWebcamPropertyAuto = function(prop) {
		if (typeof prop != "string") {
			throw Error("prop");
		}
		var webcam = self.shared.getActiveSensor();
		if (webcam.index >= 0) {
			delete webcam.data[prop];
		}
	};
	/**
	 * Generates a property getter/setter for the given property
	 * for use in html templates.
	 * @param {string} prop property name
	 */
	this.generateWebcamPropertyGetterSetter = function(prop) {
		if (typeof prop != "string") {
			throw Error("prop");
		}
		/**
		 * Gets or sets the property of the active sensor.
		 * @param {number} value optional property value tthat indicates
		 * whether to get or set the property
		 */
		return function(value) {
			if (arguments.length > 0) {
				if (!Number.isInteger(value)) {
					throw Error("prop");
				}
				self.setWebcamProperty(prop, value);
			} else {
				return self.getWebcamProperty(prop);

			}
		};
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
