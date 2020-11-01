function LiveWireService() {
	var self = this;
	this.connection = null;


	this.init = function() {
		if (this.connection) {
			return;
		}

		console.log("init from LiveWireService");
		var server = window.location.hostname;
		this.connection = new WebSocket("ws:" + server + ":9234");
		this.connection.onopen = this.connected;
		this.connection.ondisconnected = this.disconnected;
		this.connection.onmessage = this.messageReceived;
		this.connection.onerror = this.connectionFailed;
	};
	this.quit = function() {
		if (this.connection) {
			this.connection.close();
			this.connection = null;
		}
	};
	this.connected = function(ev) {
		self.sendMessage("hello from client")
	};
	this.disconnected = function(ev) {
		console.log("disconnected");
	};
	this.connectionFailed = function(ev) {
		console.log("websocket failed with error: " + ev);
	};
	this.messageReceived = function(ev) {
		console.log("received: " + ev.data);
	};
	this.sendMessage = function(msg) {
		console.log("sending: " + msg);
		self.connection.send(msg);
	};

	// start the connection
	this.init();
};

var module = angular.module("liveWire");
module.service("LiveWire", LiveWireService);
