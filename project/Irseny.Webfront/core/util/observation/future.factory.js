function Future() {
	var self = this;
	var results = [];
	var errors = [];
	var resultHandlers = [];
	var errorHandlers = [];

	this.resolve = function(result) {
		results.push(result);
		resultHandlers.forEach(function(handler) {
			handler(result);
		});
	};
	this.reject = function(error) {
		errors.push(error);
		errorHandlers.forEach(function(handler) {
			handler(error);
		});
	};
	this.then = function(handler) {
		// TODO create new future to be resolved/rejected when this handler is executed
		resultHandlers.push(handler);
		results.forEach(function(result) {
			handler(result);
		});
	};
	this.else = function(handler) {
		errorHandlers.push(handler);
		errors.forEach(function(error) {
			handler(error);
		});
	};
}
var module = angular.module("util");
module.factory("Future", function() {
	return Future;
});
