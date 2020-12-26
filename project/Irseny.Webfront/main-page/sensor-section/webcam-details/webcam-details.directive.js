/**
 * Constructs a controller for webcam settings customization.
 *
 * @constructor
 */
function WebcamDetailController() {
	var self = this;
	var aspectRatio = ""; // to be initialized
	var resolutionConfigMode = "invalid"; // to be initialized
	var resolutionStyles = {}; // in $onInit()
	var aspectRatios = [];
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
	 * Initializes this instance.
	 */
	this.$onInit = function() {
		aspectRatios = Object.keys(resolutions);
		self.setAspectRatio(aspectRatios[0]);
		self.setResolutionMode("list");
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
		if (typeof ratio != "string") {
			throw Error("ratio");
		}
		aspectRatio = ratio;
	};

	/**
	 * Sets the active resolution configuration mode.
	 * @param {string} mode valid values are "list" and "custom"
	 */
	this.setResolutionMode = function(mode) {
		if (mode == resolutionConfigMode) {
			return;
		}
		if (mode == "list") {
			// adjust the styling to use for resolution config
			// the goal is to hide the complementary elements
			// but avoid resizing the whole property table
			resolutionConfigMode = "list";
			resolutionStyles["list"] = {
				"visibility": "visible"
			};
			// with max height 0 the elements of the two modes occupy the same vertical space
			// while the hidden elements are still considered in layout calculations
			// TODO is this hack platform independent?
			resolutionStyles["custom"] = {
				"visibility": "hidden",
				"max-height": 0
			};
		} else if (mode == "custom") {
			resolutionConfigMode = "custom";
			resolutionStyles["custom"] = {
				"visibility": "visible"
			};
			resolutionStyles["list"] = {
				"visibility": "hidden",
				"max-height": 0
			};
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
	 * Returns the styling of the resolution customization box
	 * for the given resolution mode.
	 * @param {string} mode reference resolution mode
	 * @return {Object} styling to be used in ng-style
	 */
	this.getResolutionStyle = function(mode) {
		return resolutionStyles[mode];
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
		return function(value) {
			if (arguments.length > 0) {
				self.shared.getActiveSensor().setProperty(prop, value);
			} else {
				return self.shared.getActiveSensor().getProperty(prop);
			}
		};
	};
	/**
	 * Generates a property auto getter/setter for the given property
	 * for use in html templates.
	 * @param {string} prop property name
	 */
	this.generateWebcamAutoGetterSetter = function(prop) {
		if (typeof prop != "string") {
			throw Error("prop");
		}
		return function(value) {
			if (arguments.length > 0) {
				if (value) {
					self.shared.getActiveSensor().setPropertyAuto(prop);
				}
			} else {
				return self.shared.getActiveSensor().getPropertyAuto(prop);
			}
		};
	};
	/**
	 * Generates a property getter/setter
	 * that returns the given fallback value
	 * if the given property is set to auto adjust.
	 * for use in html templates.
	 * @param {string} prop property name
	 * @param {number} fallback value to return on auto
	 */
	this.generateWebcamRangeGetterSetter = function(prop, fallback) {
		if (typeof prop != "string") {
			throw Error("prop");
		}
		if (!Number.isInteger(fallback)) {
			throw Error("fallback");
		}
		return function(value) {
			if (arguments.length > 0) {
				self.shared.getActiveSensor().setProperty(prop, value);
			} else {
				if (self.shared.getActiveSensor().getPropertyAuto(prop)) {
					return fallback;
				} else {
					return self.shared.getActiveSensor().getProperty(prop);
				}
			}
		};
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
