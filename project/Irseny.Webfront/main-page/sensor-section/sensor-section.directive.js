/**
 * Active sensor to be returned from the sensor section.
 * Provides functionality for property access.
 *
 * @constructor
 * @param {number} index sensor index
 * @param {Object} data sensor data
 */
function ActiveSensor(index, active, data) {
	if (!Number.isInteger(index)) {
		throw Error("index");
	}
	if (typeof active != "boolean") {
		throw Error("active");
	}
	var self = this;
	this.index = index;
	this.active = active;
	this.data = data;

	/**
	 * Returns the given property.
	 * @param {string} prop property name
	 * @return {number} if the property is not adjusting automatically, otherwise undefined
	 */
	this.getProperty = function(prop) {
		if (typeof prop != "string") {
			throw Error("prop");
		}
		if (self.index >= 0) {
			return self.data[prop];
		}
		return undefined;
	};
	/**
	 * Indicates whether the given property is supposed to adjust automatically.
	 * This is the case if the property is not set.
	 * @param {string} prop property name
	 * @return {bool} true if the property is not set, otherwise false
	 */
	this.getPropertyAuto = function(prop) {
		if (typeof prop != "string") {
			throw Error("prop");
		}
		if (self.index >= 0) {
			return self.data[prop] == undefined;
		}
		return true;
	};
	/**
	 * Sets the given property.
	 * @param {string} prop property name
	 * @param {number} value property value or undefinded to set the property to auto
	 */
	this.setProperty = function(prop, value) {
		if (typeof prop != "string") {
			throw Error("prop");
		}
		if (self.index >= 0) {
			if (value == undefined) {
				delete self.data[prop];
			} else if (Number.isInteger(value)) {
				self.data[prop] = value;
			} else {
				throw Error("value");
			}
		}
	};
	/**
	 * Sets the given property to adjust automatically.
	 * @param {string} prop property name
	 */
	this.setPropertyAuto = function(prop) {
		if (typeof prop != "string") {
			throw Error("prop");
		}
		if (self.index >= 0) {
			delete self.data[prop];
		}
	};

};
/**
 * Controller for the sensor section template.
 * Provides functionality for accessing sensors.
 * @constructor
 * @param {Object} $scope scope service
 * @param {Object} LiveWireService LiveWire service
 * @param {Object} LiveExchangeService LiveExchange service
 * @param {Object} TaskScheduleService TaskSchedule service
 */
function SensorSectionController($scope, LiveWireService, LiveExchangeService, TaskScheduleService, MessageLog) {
	var self = this;

	var setup = [];
	var availableModel = [];
	var availableDirty = true;
	var activeIndex = -1;
	var activeModel = undefined;
	var activeDirty = true;

	const sensorClasses =  ["Webcam", "Kinect"];
	const maxSensorNo = 16;
	var liveSubscription = undefined;

	this.$onInit = function() {
		liveSubscription = LiveExchangeService.getObserver("sensor").subscribe(self.sensorUpdated);
		LiveExchangeService.getObtainer("sensor").then(self.sensorUpdated);
	};

	this.ensureCleanModels = function() {
		if (availableDirty) {
			// regenerate available
			availableModel = [];
			for (var i = 0; i < setup.length; i++) {
				if (setup[i]) {
					availableModel.push({index: i, active: i == activeIndex, data: setup[i]});
				}
			}
			availableDirty = false;
		}
		if (activeDirty) {
			// regenerate active
			if (activeIndex < 0 || activeIndex >= setup.length || setup[activeIndex] == undefined) {
				activeIndex = -1;
				activeModel = new ActiveSensor(-1, false, undefined);
			} else {
				activeModel = new ActiveSensor(activeIndex, true, setup[activeIndex]);//{index: activeIndex, active: true, data: setup[activeIndex]};
			}
			activeDirty = false;
		}
	};

	this.getSensorClasses = function() {
		return sensorClasses;
	};
	this.isActiveSensor = function(sensor) {
		return sensor.index == activeIndex;
	};
	/**
	 * Exchanges a sensor with the server through LiveWire.
	 * @param {Object} sensor object with index property which specifies the sensor to exchange
	 * @return request future
	 */
	this.exchangeSensor = function(sensor) {
		if (!Number.isInteger(sensor.index)) {
			throw Error("sensor.index");
		}
		var data = setup[sensor.index];

		var subject = {
			type: "post",
			topic: "sensor",
			position: sensor.index,
			data: [{
				inuse: data != undefined,
				settings: data
			}]
		};
		return LiveWireService.sendRequest(subject);
	};
	/**
	 * Resets a sensor to the current settings in LiveExchange.
	 * @param {Object} sensor object with index property which specifies the sensor to reset
	 * @return {boolean} indicates whether the operation was successful
	 */
	this.resetSensor = function(sensor) {
		if (!Number.isInteger(sensor.index)) {
			throw Error("sensor");
		}
		// this should replace the local changes with the current state
		LiveExchangeService.getObtainer("sensor", sensor.index).then(self.sensorUpdated);
		//MessageLog.logInfo("Sensor ".concat(sensor.index, " reset"));
	};
	/**
	 * Exchanges a new sensor through LiveWire.
	 * @param {Object} sensor object with index and sensor data
	 * @return {Object} request future, or undefined if the operation failed
	 */
	this.addSensor = function(sensor) {
		if (sensor.data == undefined) {
			throw Error("sensor.data");
		}
		var iFree = -1;
		for (var i = 0; i < maxSensorNo; i++) {
			if (setup[i] == undefined) {
				iFree = i;
			}
		}
		if (iFree < 0) {
			return undefined;
		}
		var subject = {
			type: "post",
			topic: "sensor",
			position: iFree,
			data: [{
				inuse: true,
				settings: sensor.data
			}]
		};
		return LiveWireService.sendRequest(subject);
	};
	/**
	 * Sends an unused sensor message through LiveWire.
	 * @param {Object} sensor object with index information
	 * @return {Object} request future
	 */
	this.removeSensor = function(sensor) {
		if (!Number.isInteger(sensor.index)) {
			throw Error("sensor.index");
		}
		var subject = {
			type: "post",
			topic: "sensor",
			position: sensor.index,
			data: [{inuse: false}]
		};
		return LiveWireService.sendRequest(subject);
	};
	/**
	 * Sets the active sensor.
	 * @param {Object} sensor sensor with index property
	 * @return {boolean} indicates if the given sensor could be set as active sensor
	 */
	this.setActiveSensor = function(sensor) {
		if (!Number.isInteger(sensor.index)) {
			throw Error("sensor.index");
		}
		if (setup[sensor.index] == undefined) {
			return false;
		}
		activeIndex = sensor.index;
		activeDirty = true;
		return true;
	};
	/**
	 * Returns the currently active sensor.
	 * @return {Object} active sensor descriptor with index and data properties
	 */
	this.getActiveSensor = function() {
		self.ensureCleanModels();
		return activeModel;
	};
	/**
	 * Returns a list of available sensors
	 * @return {Array} sensor descriptors with index and data properties
	 */
	this.getAvailableSensors = function() {
		self.ensureCleanModels();
		return availableModel;
	};

	/**
	 * Updates the information about available sensors.
	 * Called whenever LiveExchange receives new sensor data.
	 * @param {Object} update updated sensor information
	 */
	this.sensorUpdated = function(update) {
		if (!Number.isInteger(update.index) || update.index < 0) {
			return;
		}
		if (update.data == undefined) {
			setup[update.index] = undefined;
		} else {
			setup[update.index] = JSON.parse(JSON.stringify(update.data));
		}
		//MessageLog.logInfo("Received update for sensor ".concat(update.index));
		setup = setup.slice();
		activeDirty = true;
		availableDirty = true;
		TaskScheduleService.addTimeout("digest", 5000, function() {
			$scope.$digest();
		});
	};

	this.$onDestroy = function() {
		LiveExchangeService.getObserver("sensor").unsubscribe(liveSubscription);

	};
}
SensorSectionController.$inject = ["$scope", "LiveWireService", "LiveExchangeService", "TaskScheduleService", "MessageLog"];

var module = angular.module("sensorSection");
module.directive("sensorSection", function() {
	return {
		restrict: 'E',
		scope: false,
		controller: SensorSectionController,
		controllerAs: "$ctrl",
		templateUrl: "main-page/sensor-section/sensor-section.template.html"
	};
});
