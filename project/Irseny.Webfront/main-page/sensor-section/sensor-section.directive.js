function SensorSectionController($scope, LiveWireService, LiveExchangeService, TaskScheduleService) {
	var self = this;

	var setup = [];
	var availableModel = [];
	var availableDirty = true;
	var activeIndex = -1;
	var activeModel = undefined;
	var activeDirty = true;

	const sensorClasses =  ["Webcam", "Kinect"];

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
				activeModel = {index: -1, active: false, data: undefined};
			} else {
				activeModel = {index: activeIndex, active: true, data: setup[activeIndex]};
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
	this.exchangeSensor = function(sensor) {
		if (!Number.isInteger(sensor.index)) {
			return false;
		}
		var sensor = setup[sensor.index];

		var subject = {
			type: "post",
			topic: "sensor",
			position: sensor.index,
			data: [{
				inuse: sensor != undefined,
				settings: sensor
			}]
		};
		LiveWireService.sendUpdate(subject);
		return true;
	};
	this.setActiveSensor = function(sensor) {
		if (!Number.isInteger(sensor.index)) {
			return false;
		}
		if (setup[sensor.index] == undefined) {
			return false;
		}
		activeIndex = sensor.index;
		activeDirty = true;
		return true;
	};
	this.getActiveSensor = function() {
		self.ensureCleanModels();
		return activeModel;
	};
	this.getAvailableSensors = function() {
		self.ensureCleanModels();
		return availableModel;
	};


	this.sensorUpdated = function(update) {
		if (!Number.isInteger(update.index) || update.index < 0) {
			return;
		}
		if (update.data == undefined) {
			setup[update.index] = undefined;
		} else {
			setup[update.index] = Object.assign(update.data);
		}
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
SensorSectionController.$inject = ["$scope", "LiveWireService", "LiveExchangeService", "TaskScheduleService"];

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
