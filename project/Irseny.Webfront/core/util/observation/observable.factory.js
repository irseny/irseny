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
