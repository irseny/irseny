// This file is part of Irseny.
//
// Copyright (C) 2021  Thilo Gabel
//
// Irseny is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
//
// Irseny is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
//
// You should have received a copy of the GNU General Public License
// along with this program.  If not, see <https://www.gnu.org/licenses/>.

/**
 * Helper for interpreting update messages.
 * A standard update message contains state information that is made available
 * through eqiupment setup for later access. An example for this are sensor settings.
 * A capture update message contains data that is only relevant for a small
 * period of time and not critical for operation,
 * e.g. a single video frame.
 */
function CameraUpdateHandler() {
	/**
	 * Handles an update message.
	 * Updates setup data and notifies observers of setup changes.
	 * @param {Object} subject subject of an update message
	 * @param {Array} branch setup branch to modify
	 * @param {Object} observer observable to notify in case of changes
	 * @return {Array} changed entries
	 */
	this.handleUpdate = function(subject, branch, observer) {
		// extract and check status information
		if (subject.type != "post" || subject.data == undefined) {
			return [];
		}
		if (!Array.isArray(subject.data)) {
			if (typeof subject.data != "object") {
				return [];
			}
			// alternative format: data contains index and entry
			var result = [];
			for (var key in subject.data) {
				var index = Number.parseInt(key);
				if (!Number.isInteger(index)) {
					return [];
				}
				var data = undefined;
				if (subject.data[index].inuse) {
					data = subject.data[index].settings;
					if (data == undefined) {
						return [];
					}
				}
				result.push({
					index: index,
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
		}
		if (subject.position == undefined) {
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
	/**
	 * Handles a capture update message.
	 * The captured data is only relayed to observers and not cached.
	 * @param {Object} subject of the update message
	 * @param {observer} observable through which observers are notified
	 * @return {Array} updated data
	 */
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
	/**
	 * Returns a future that provides the current setup.
	 * @param {Array} branch data that the future shall provide
	 * @param {Number} index optional restriction to a single entry
	 * @return {Object} future with the given data already cached
	 */
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
	function receiveUpdate(subject) {
		var updated = [];
		switch (subject.topic) {
		case "camera":
		case "sensor":
			updated = updateHandler.handleUpdate(subject, setup.cameras, setup.cameraObserver);
		break;
		case "sensorCapture":
			updated = updateHandler.handleCapture(subject, setup.sensorCaptureObserver);
		break;
		case "tracker":
			updated = updateHandler.handleUpdate(subject, setup.trackers, setup.trackerObserver);
		break;
		case "trackerCapture":
			updated = updateHandler.handleCapture(subject, setup.trackerCaptureObserver);
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
	function requestFullSetup() {
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

		var trackerSubject = {
			type: "get",
			topic: "tracker",
			data: Object.assign({}, Array(FixedEquipmentNo).fill(null))
		};
		LiveWireService.sendUpdate(trackerSubject);
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
		case "tracker":
			return setup.trackerObserver;
		case "trackerCapture":
			return setup.trackerCaptureObserver;
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
		case "tracker":
			return updateHandler.createEquipmentObtainer(setup.trackers);
		default:
			throw new Error("topic");
		}
	};
	/**
	 * Initializes this instance. For internal use only.
	 */
	function init() {
		LiveWireService.getUpdateObserver().subscribe(receiveUpdate);
		requestFullSetup();
	};
	init();
};

var module = angular.module("liveConnection");
module.service("LiveExchangeService", LiveExchangeService);
LiveExchangeService.$inject = ["MessageLog", "LiveWireService"];
