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

function SensorListController($element, $timeout, MessageLog) {
	var self = this;

	var renaming = false; // set while renaming is active

	var templates = {
		webcam: {
			type: "Webcam",
			name: "Webcam",
			frameWidth: 320,
			frameHeight: 240
		}
	};
	/**
	 * Gets a template for a new sensor which consists of standard properties.
	 * The template can be used to add a new sensor in sensor-section.
	 * @param {string} type sensor type
	 * @return {Object} standard template with a data property
	 */
	this.getSensorTemplate = function(type) {
		if (typeof type != "string") {
			throw Error("type");
		}
		if (templates[type] == undefined) {
			return { data: {} };
		} else {
			return { data: JSON.parse(JSON.stringify(templates[type])) };
		}
	};
	/**
	 * Indicates whether the active sensor is getting renamed.
	 * @return {boolean} renaming active status
	 */
	this.isRenaming = function() {
		return renaming;
	};
	/**
	 * Begins active sensor renaming.
	 */
	this.startRenaming = function() {
		if (self.isRenaming()) {
			return;
		}
		renaming = true;
	};
	/**
	 * Stops sensor renaming.
	 * @param {boolean} apply confirms the name in the corresponding input field
	 */
	this.stopRenaming = function(nextName=undefined) {
		if (!self.isRenaming()) {
			return;
		}
		if (nextName != undefined) {
			var sensor = self.shared.getActiveSensor();
			sensor.setProperty('name', nextName);
			self.shared.exchangeSensor(sensor);
		}
		renaming = false;
	};
	/**
	 * Helper function to process renaming finishing events.
	 * @param {Object} ev event
	 */
	this.listenRenameFinish = function(ev) {
		if (ev == undefined || ev.detail == undefined) {
			self.stopRenaming(undefined);
		} else if (ev.detail.cancel) {
			self.stopRenaming(undefined);
		} else {
			self.stopRenaming(ev.detail.name);
		}
	};
};
SensorListController.$inject = ["$element", "$timeout", "MessageLog"];


var module = angular.module("sensorList");


module.directive("sensorList", function() {
	return {
		restrict: 'E',
		require: {
			shared: "^sensorSection"
		},
		bindToController: true,
		controller: SensorListController,
		controllerAs: "$ctrl",
		scope: true,
		templateUrl: "main-page/sensor-section/sensor-list/sensor-list.template.html"
	};
});
