function CameraUpdateHandler() {

	this.handleUpdate = function(subject, cameras, observer) {
		// extract and check status information
		if (subject.type != "post" || subject.data == undefined || subject.position == undefined) {
			return [];
		}
		if (!Array.isArray(subject.data)) {
			return result;
		}
		var iStart = -1;
		if (subject.position == "all") {
			iStart = 0;
		} else {
			iStart = Number.parseInt(subject.position, 10);
			if (!Number.isInteger(iStart)) {
				iStart = -1;
			}
		}
		if (iStart < 0) {
			return [];
		}
		// extract all updated cameras
		var result = [];
		for (var i = iStart; i < subject.data.length; i++) {
			var status = subject.data[i].status;

			if (typeof status != 'string') {
				return [];
			}
			var camera = undefined;
			if (status == "active") {
				if (subject.data[i].settings == undefined) {
					return [];
				}
				camera = subject.data[i].settings;
			}

			result.push({
				index: i,
				data: camera
			});
		}
		result.forEach(function(entry) {
			cameras[entry.index] = entry.data;
		});
		result.forEach(function(entry) {
			observer.notify(entry);
		});

		return result;
	};
	this.obtainUpdates = function(cameras) {
		var result = new Future();
		for (var i = 0; i < cameras.length; i++) {
			result.resolve({ index: i, data: cameras[i]});
		}
		return result;
	};
}

function LiveExchangeService(MessageLog, LiveWireService) {
	const FixedEquipmentNo = 16;
	var self = this;
	var updateHandler = new CameraUpdateHandler();
	var setup = {
		profile: {
			available: [],
			active: ""
		},
		config: {
			autoApplyChanges: true
		},
		cameras: Array(FixedEquipmentNo),
		trackers: Array(FixedEquipmentNo),
		inputDevices: Array(FixedEquipmentNo),
		outputDevices: Array(FixedEquipmentNo),
		cameraObserver: new Observable(),
		trackerObserver: new Observable(),
		inputObserver: new Observable(),
		outputObserver: new Observable(),
		observers: {
			"camera": new Observable(),
			"tracker": new Observable(),
			"outputDevice": new Observable(),
			"inputDevice": new Observable()
		}
	};

	this.receiveUpdate = function(subject) {
		var updated = [];
		switch (subject.topic) {
		case "camera":
			updated = updateHandler.handleUpdate(subject, setup.cameras, setup.cameraObserver);
		break;
		default:
			updated = [];
		break;
		}
		if (updated == undefined || updated.length == 0) {
			MessageLog.logWarning("No data extracted from ".concat(JSON.stringify(subject)));
		}
	};

	this.touch = function() {
		MessageLog.log("Live exchange requesting camera update from server");
		var subject = {
			type: "get",
			topic: "camera",
			position: "all",
		};
		LiveWireService.requestUpdate(subject).then(self.receiveUpdate);
	};
	this.observe = function(topic) {
		switch (topic) {
		case "camera":
			return setup.cameraObserver;
		default:
			throw new Error("topic");
		}
	};
	this.obtain = function(topic) {
		switch (topic) {
		case "camera":
			return updateHandler.obtainUpdates(setup.cameras);
		break;
		default:
			throw new Error("topic");
		}
	};
	this.start = function() {
		LiveWireService.receiveUpdate().notify(self.receiveUpdate);
	};
	this.start();
};

var module = angular.module("liveConnection");
module.service("LiveExchangeService", LiveExchangeService);
LiveExchangeService.$inject = ["MessageLog", "LiveWireService"];
