function CameraSectionController($scope, LiveExchangeService, LiveWireService) {
	var self = this;

	// available cameras
	var setup = [];
	var availableModel = [];
	var availableDirty = true;
	var activeIndex = -1;
	var activeModel = undefined;
	var activeDirty = true;

	const sensorClasses = [ "Webcam", "MS Kinect", "Camcorder" ];
	//this.availableCameras = [ { index: 0, data: "LiveVision0"}, {index: 1, data: "Kintect2"}, {index: 2, data: "Logitech UltraMegaPro2"} ];
	//this.activeCamera = 0;



	this.ensureClean = function() {
		if (availableDirty) {
			// regenerate available
			availableModel = [];
			for (var i = 0; i < setup.length; i++) {
				if (setup[i]) {
					availableModel.push({ index: i, data: setup[i]});
				}
			}
			availableDirty = false;
		}
		if (activeDirty) {
			// regenerate active
			if (activeIndex < 0 || activeIndex >= setup.length || setup[activeIndex] == undefined) {
				activeIndex = -1;
				activeModel = {index: -1, data: undefined};
			} else {
				activeModel = {index: activeIndex, data: setup[activeIndex]};
			}
			activeDirty = false;
		}
	};

	this.getSensorClasses = function() {
		return sensorClasses;
	};
	this.isActiveCamera = function(cam) {
		return cam.index == activeIndex;
	};
	this.exchangeActiveSensor = function() {
		// TODO send from LiveExchange to server
		self.ensureClean();
		if (activeModel.index < 0) {
			return false;
		}
		var subject = {
			type: "post",
			topic: "camera",
			position: activeModel.index,
			data: activeModel.data
		};
		LiveWireService.sendUpdate(subject);
	};
	this.setActiveCamera = function(cam) {
		if (!Number.isInteger(cam.index)) {
			return false;
		}
		if (setup[cam.index] == undefined) {
			return false;
		}
		activeIndex = cam.index;
		activeDirty = true;
		return true;
	};
	this.getActiveCamera = function() {
		self.ensureClean();
		return activeModel;
	};
	this.getAvailableCameras = function() {
		self.ensureClean();
		return availableModel;
	};


	this.cameraUpdated = function(update) {
		if (!Number.isInteger(update.index) || update.index < 0) {
			return;
		}
		if (update.data == undefined) {
			setup[update.index] = undefined;
		} else {
			setup[update.index] = Object.assign(update.data);
		}
		// console.log("section camera".concat(update.index, " updated"));
		setup = setup.slice();
		activeDirty = true;
		availableDirty = true;
		$scope.$digest(); // TODO maybe delay until all updates are done; timeout?
	};
	this.requestActiveSensorCapture = function() {
		if (activeIndex < 0) {
			return undefined;
		}
		var subject = {
			type: "get",
			topic: "sensorCapture",
			position: activeIndex
		};
		return LiveWireService.requestUpdate(subject);
	};


	const liveSubscription = LiveExchangeService.observe("camera").subscribe(this.cameraUpdated);
	//LiveExchangeService.obtain("camera").then(this.cameraUpdated);
	LiveExchangeService.touch(); // TODO remove

	this.$onDestroy = function() {
		LiveExchangeService.observe("camera").unsubscribe(liveSubscription);
	};
	/*$scope.$on('$destroy', function () {
    	LiveExchangeService.observe("camera").unsubscribe(liveSubscription);
	});*/
}
function SensorSectionController() {
	this.getIndexTesting = function() {
		return 32;
	};
}

var module = angular.module("cameraSection");
CameraSectionController.$inject = ["$scope", "LiveExchangeService", "LiveWireService"];
/*module.component("cameraSection", {
	templateUrl : "main-page/camera-section/camera-section.template.html",
	controller : CameraSectionController,
	controllerAs: "$ctrl"
});*/



module.directive("cameraSection", function() {
	return {
		restrict: 'E',
		scope: true,
		controller: SensorSectionController,
		controllerAs: "$ctrl",
		templateUrl: "main-page/camera-section/camera-section.template.html"
	};
});


