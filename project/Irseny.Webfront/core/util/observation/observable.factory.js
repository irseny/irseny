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

function Observable() {
	var self = this;
	var subscriptions = [];
	var nextSubscriptionId = 0;

	this.generateSubscriptionId = function() {
		return nextSubscriptionId++;
	};

	this.subscribe = function(observer) {
		if (typeof observer != "function") {
			throw Error("observer");
		}
		var id = self.generateSubscriptionId();
		subscriptions.push({
			id: id,
			observer: observer
		});
		return id;
	};
	this.unsubscribe = function(id) {
		var index = subscriptions.findIndex(function(sub) {
			return sub.id == id;
		});
		if (index < 0) {
			return false;
		}
		if (index >= subscriptions.length - 1) {
			var last = subscriptions.pop();
			subscriptions[index] = last;
		} else {
			subscriptions.pop();
		}
		return true;
	};
	this.notify = function(message) {
		subscriptions.forEach(function(sub) {
			sub.observer(message);
		});
	};
}

var module = angular.module("util");
module.factory("Observable", function() {
	return Observable;
});
