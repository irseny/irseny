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
 * Service for task scheduling.
 * A task is identified by a name and it can have multiple timeouts attached.
 * As soon as a timeout expires the task is fired.
 * The task does not fire again, unless also added again after firing.
 *
 * @param {Object} $timeout angularjs timeout service
 */
function TaskScheduleService($timeout) {
	var self = this;
	var timeoutTasks = {};
	var intervalTasks = {};

	/**
	 * Returns whether a task with the given name is scheduled.
	 * @param {string} name task identifier
	 * @return {boolean} indicates whether the task is scheduled
	 */
	this.hasTimeout = function(name) {
		if (typeof name != "string") {
			return false;
		}
		return timeoutTasks[name] != undefined;
	};
	/**
	 * Schedules a task. Adds a timeout for the given task identifier.
	 * If already existent the task function is updated.
	 * @param {string} name task identifier
	 * @param {number} timeout timeout in milliseconds
	 * @param {function} task task to execute
	 */
	this.addTimeout = function(name, timeout, task) {
		if (typeof name != "string") {
			throw Error("name");
		}
		if (!Number.isInteger(timeout)) {
			throw Error("timeout");
		}
		if (typeof task != "function") {
			throw Error("task");
		}
		var legacy = timeoutTasks[name];
		if (legacy != undefined) {
			// if a timeout of the same name already exists
			// we just add the new promise and update the task
			legacy.task = task;
			var promise = $timeout(self.resolveTimeout, timeout, true, name);
			legacy.promises.push(promise);
		} else {
			// otherwise we create a new timeout that calls resolveTimeout()
			var wrapper = { promises: [], task: task };
			timeoutTasks[name] = wrapper;
			var promise = $timeout(self.resolveTimeout, timeout, true, name);
			wrapper.promises.push(promise);
		}
	};
	/**
	 * Cancels a task.
	 * @param {string} name identifies the task to cancel
	 * @return {boolean} indicates whether there was a task to cancel
	 */
	this.cancelTimeout = function(name) {
		if (typeof name != "string") {
			throw Error("name");
		}
		var legacy = timeoutTasks[name];
		if (legacy == undefined) {
			return false;
		}
		// cancel all promises associated with the timeout
		legacy.promises.forEach(function(promise) {
			$timeout.cancel(promise);
		});
		delete timeoutTasks[name];
		return true;
	};
	/**
	 * Reserved for internal use.
	 * Executes the task identified by the given name.
	 * The task is removed and so prevented from executing again.
	 * @param {string} name task identifier
	 * @return {boolean} indicates whether there was a task to execute
	 */
	this.resolveTimeout = function(name) {
		if (typeof name != "string") {
			throw Error("name");
		}
		var legacy = timeoutTasks[name];
		if (legacy == undefined) {
			return false;
		}
		// stop the task from getting triggered again
		// after the first timeout runs out
		legacy.promises.forEach(function(promise) {
			$timeout.cancel(promise);
		});
		// a task might add itself again
		// so first remove the task, then execute it
		delete timeoutTasks[name];
		legacy.task();
		return true;
	};
};
TaskScheduleService.$inject = ["$timeout"];
var module = angular.module("taskSchedule");
module.service("TaskScheduleService", TaskScheduleService);
