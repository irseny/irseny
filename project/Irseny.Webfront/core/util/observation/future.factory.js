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
	this.catch = function(handler) {
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
