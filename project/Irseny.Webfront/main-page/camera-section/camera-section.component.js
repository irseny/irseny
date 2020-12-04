function CameraSectionController($scope, LiveExchangeService) {
	var self = this;

	// available cameras
	var setup = [];
	var availableModel = [];
	var availableDirty = true;
	var activeIndex = -1;
	var activeModel = undefined;
	var activeDirty = false;

	const cameraClasses = [ "Webcam", "MS Kinect", "Camcorder" ];
	//this.availableCameras = [ { index: 0, data: "LiveVision0"}, {index: 1, data: "Kintect2"}, {index: 2, data: "Logitech UltraMegaPro2"} ];
	//this.activeCamera = 0;

	this.getSensorClasses = function() {
		return cameraClasses;
	};
	this.isActiveCamera = function(cam) {
		return cam.index == activeIndex;
	};
	this.exchangeCamera = function(cam) {
		// TODO send from LiveExchange to server
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
		if (activeDirty) {
			if (activeIndex < 0 || activeIndex >= setup.length || setup[activeIndex] == undefined) {
				activeIndex = -1;
				activeModel = undefined;
			} else {
				activeModel = {index: activeIndex, data: setup[activeIndex]};
			}
			activeDirty = false;

		}
		return activeModel;
	};
	this.getAvailableCameras = function() {
		if (availableDirty) {
			availableModel = [];
			for (var i = 0; i < setup.length; i++) {
				if (setup[i]) {
					availableModel.push({ index: i, data: setup[i]});
				}
			}
			availableDirty = false;
		}
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


	const liveSubscription = LiveExchangeService.observe("camera").subscribe(this.cameraUpdated);
	//LiveExchangeService.obtain("camera").then(this.cameraUpdated);
	LiveExchangeService.touch(); // TODO remove

	$scope.$on('$destroy', function () {
    	LiveExchangeService.observe("camera").unsubscribe(liveSubscription);
	});
}
CameraSectionController.$inject = ["$scope", "LiveExchangeService"];

var module = angular.module("cameraSection");
module.component("cameraSection", {
	templateUrl : "main-page/camera-section/camera-section.template.html",
	controller : CameraSectionController
});




