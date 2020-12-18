function CameraUpdateHandler() {

	this.handleUpdate = function(subject, branch, observer) {
		// extract and check status information
		if (subject.type != "post" || subject.data == undefined || subject.position == undefined) {
			return [];
		}
		if (!Array.isArray(subject.data)) {
			return [];
		}
		var offset = -1;
		var length = 0;
		if (subject.position == "all") {
			offset = 0;
			length = subject.data.length;
		} else {
			offset = Number.parseInt(subject.position, 10);
			length = Math.min(1, subject.data.length);
			if (!Number.isInteger(offset)) {
				offset = -1;
			}
		}
		if (offset < 0) {
			return [];
		}
		// extract all updated cameras
		var result = [];
		for (var i = 0; i < length; i++) {
			var status = subject.data[i].status;

			if (typeof status != 'string') {
				return [];
			}
			var data = undefined;
			if (status == "active") {
				if (subject.data[i].settings == undefined) {
					return [];
				}
				data = subject.data[i].settings;
			}

			result.push({
				index: i + offset,
				data: data
			});
		}
		result.forEach(function(entry) {
			branch[entry.index] = entry.data;
		});
		result.forEach(function(entry) {
			observer.notify(entry);
		});

		return result;
	};
	this.handleCapture = function(subject, observer) {
		if (subject.type != "post" || subject.data == undefined || subject.position == undefined) {
			return [];
		}
		if (!Array.isArray(subject.data)) {
			return [];
		}
		var offset = -1;
		var length = 0;
		if (subject.position == "all") {
			offset = 0;
			length = subject.data.length;
		} else {
			offset = Number.parseInt(subject.position, 10);
			length = Math.min(1, subject.data.length);
			if (!Number.isInteger(offset)) {
				offset = -1;
			}

		}
		if (offset < 0) {
			return [];
		}
		var result = [];
		for (var i = 0; i < length; i++) {
			result.push({
				index: i + offset,
				data: subject.data[i]
			});
		}
		result.forEach(function(entry) {
			observer.notify(entry);
		});
		return result;
	};
	this.obtainUpdates = function(branch) {
		var result = new Future();
		for (var i = 0; i < branch.length; i++) {
			result.resolve({ index: i, data: branch[i]});
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
		sensorCaptureObserver: new Observable(),
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
		case "sensor":
			updated = updateHandler.handleUpdate(subject, setup.cameras, setup.cameraObserver);
		break;
		case "sensorCapture":
			updated = updateHandler.handleCapture(subject, setup.sensorCaptureObserver);
		break;
		default:
			updated = [];
		break;
		}
		if (updated == undefined || updated.length == 0) {
			MessageLog.logWarning("LiveExchange can not make sense of\n".concat(JSON.stringify(subject)));
		}
	};

	this.requestFullEquipment = function() {
		// send requests for all equipment types
		var sensorSubject = {
			type: "get",
			topic: "sensor",
			position: "all"
		};
		// but send the requests masked as updates
		// so that the results come back as updates and not requests
		// (updates are relayed to observers and this instance should be subscribed to LiveWireSerive)
		LiveWireService.sendUpdate(sensorSubject);

		var trackerSubject = {
			type: "get",
			topic: "tracker",
			position: "all"
		};
		LiveWireService.sendUpdate(trackerSubject);
	};
	/**
	 *
	 */
	this.observe = function(topic) {
		switch (topic) {
		case "sensor":
		case "camera":
			return setup.cameraObserver;
		case "sensorCapture":
			return setup.sensorCaptureObserver;
		default:
			throw new Error("topic");
		}
	};
	this.obtain = function(topic) {
		switch (topic) {
		case "camera":
		case "sensor":
			return updateHandler.obtainUpdates(setup.cameras);
		default:
			throw new Error("topic");
		}
	};
	this.$onInit = function() {
		console.log("init from live exchange");
	};
	this.$onDestroy = function() {
		console.log("destroy from live exchange");
	};
	this.start = function() {
		LiveWireService.receiveUpdate().subscribe(self.receiveUpdate);
		self.requestFullEquipment();
	};
	this.start();
};

var module = angular.module("liveConnection");
module.service("LiveExchangeService", LiveExchangeService);
LiveExchangeService.$inject = ["MessageLog", "LiveWireService"];
