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
			var inuse = subject.data[i].inuse;

			if (typeof inuse != "boolean") {
				return [];
			}
			var data = undefined;
			if (inuse) {
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

	this.createEquipmentObtainer = function(branch, index) {
		var result = new Future();
		if (arguments.length < 2) {
			for (var i = 0; i < branch.length; i++) {
				result.resolve({ index: i, data: branch[i]});
			}
		} else {
			result.resolve({ index: index, data: branch[index]});
		}
		return result;
	};

}
/**
 * LiveExchange communicates with the server through LiveWire.
 * Specifically it handles setup changes and mirrors the state on the server.
 * Users of the service can themself be notified of changes or obtain the current state.
 */
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
	/**
	 * Handles setup changes. For internal use only.
	 * @param {Object} subject received update
	 */
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

	/**
	 * Sends out requests to the server to send the full, currently active setup.
	 */
	this.requestFullSetup = function() {
		// send requests for all equipment types
		var sensorSubject = {
			type: "get",
			topic: "sensor",
			position: "all"
		};
		// but send the requests masked as updates
		// so that the results come back as updates and not responses
		// (updates are relayed to observers - this instance should be subscribed to LiveWireSerive)
		LiveWireService.sendUpdate(sensorSubject);

		/*var trackerSubject = {
			type: "get",
			topic: "tracker",
			position: "all"
		};
		LiveWireService.sendUpdate(trackerSubject);*/
	};
	/**
	 * Returns an observer which can be used to observer setup changes.
	 * @param {string} topic setup filter; accepted values are "sensor", "camear", "sensorCapture"
	 * @return setup changes observable
	 */
	this.getObserver = function(topic) {
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
	/**
	 * Returns a future that provides the current setup.
	 * @param {string} topic setup filter; accepted values are "sensor", "camera"
	 * @return current setup providing future
	 */
	this.getObtainer = function(topic, index) {
		switch (topic) {
		case "camera":
		case "sensor":
			return updateHandler.createEquipmentObtainer(setup.cameras);
		default:
			throw new Error("topic");
		}
	};
	/**
	 * Initializes this instance. For internal use only.
	 */
	this.start = function() {
		LiveWireService.getUpdateObserver().subscribe(self.receiveUpdate);
		self.requestFullSetup();
	};
	this.start();
};

var module = angular.module("liveConnection");
module.service("LiveExchangeService", LiveExchangeService);
LiveExchangeService.$inject = ["MessageLog", "LiveWireService"];
