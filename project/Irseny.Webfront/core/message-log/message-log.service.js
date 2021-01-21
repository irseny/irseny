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

function MessageLogService() {
	const LogMode = {
		ReportMessageOnly: 0,
		ReportShortSource: 1,
		ReportDetailedSource: 2
	};
	Object.freeze(LogMode);

	var self = this;
	var activeMode = LogMode.ReportMessageOnly;

	this.setMode = function(mode) {
		activeMode = mode;
	};

	this.log = function(text) {
		var file = "";
		var line = -1;
		switch (activeMode) {
		case LogMode.ReportShortSource:
		case LogMode.ReportDetailedSource:

		break;
		default:
		break;
		}
		console.log(text);
	};
	this.logInfo = function(text) {
		console.info(text);
	};
	this.logError = function(text) {
		console.error(text);
	};
	this.logWarning = function(text) {
		console.warn(text);
	};
};

var module = angular.module("messageLog");
module.service("MessageLog", MessageLogService);
