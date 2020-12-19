function TaskScheduleService($timeout) {
	var self = this;
	var timeoutTasks = {};
	var intervalTasks = {};

	this.hasTimeout = function(name) {
		if (typeof name != "string") {
			return false;
		}
		return timeoutTasks[name] != undefined;
	};
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
			var timeout = { promises: [], task: task };
			timeoutTasks[name] = timeout;
			var promise = $timeout(self.resolveTimeout, timeout, true, name);
			timeout.promises.push(promise);
		}
	};
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
	this.resolveTimeout = function(name) {
		if (typeof name != "string") {
			throw Error("name");
		}
		var legacy = timeoutTasks[name];
		if (legacy == undefined) {
			return;
		}
		// stop the task from getting triggered again
		// after the first timeout runs out
		legacy.promises.forEach(function(promise) {
			$timeout.cancel(promise);
		});
		delete timeoutTasks[name];
		legacy.task();
	};
};
TaskScheduleService.$inject = ["$timeout"];
var module = angular.module("taskSchedule");
module.service("TaskScheduleService", TaskScheduleService);
