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
};

var module = angular.module("messageLog");
module.service("MessageLog", MessageLogService);
