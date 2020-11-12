function LiveExchangeService(MessageLog, LiveWireService) {
	var self = this;
	self.setup = {
		profile: {
			available: [],
			active: ""
		},
		config: {
			autoApplyChanges: true
		},
		sensors: [],
		trackers: [],
		inputDevices: [],
		outputDevices: []
	};

	this.receiveUpdate = function(msg) {
		console.log("exchange received message " + JSON.stringify(msg));
	};

	this.start = function() {
		LiveWireService.receiveUpdate.notify(self.receiveUpdate);
	};
	this.start();
};

var module = angular.module("liveConnection");
module.service("LiveExchangeService", LiveExchangeService);
LiveExchangeService.$inject = ["MessageLog", "LiveWireService"];
