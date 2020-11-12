


function LiveWireUpdateHandler(MessageLog) {
	var observable = new Observable(function() {});

	this.tryHandleUpdate = function(msg) {
		// TODO send to clients

		if (msg.type != "update") {
			return false;
		}
		//for (var client in self.clients) {
		//	client.callback(msg);
		//};
		console.log("wire notified of " + JSON.stringify(msg));
		observable.notify(msg);

		return true;
	};
	this.receiveUpdate = function() {
		return observable;
	};
}

function LiveWireRequestHandler(MessageLog, connection) {
	var self = this;
	var pendingAnswers = [];
	var nextRequestId = 0;

	this.generateRequestId = function() {
		return nextRequestId++;
	};
	this.tryHandleAnswer = function(answer) {
		if (answer.type != "request") {
			return false;
		}
		// try to match the answer to a previous request
		// in order to resolve the request
		var index = pendingAnswers.findIndex(function(pending) {
			return pending.id == answer.id;
		});
		if (index < 0) {
			MessageLog.log("No matching request for received answer ".concat(answer.id, ". Dropping packet"));
		} else {
			MessageLog.log("wire received answer: " + JSON.stringify(answer.subject));
			pendingAnswers[index].future.resolve(answer.subject);
			// remove the request if no more answers are to come
			if (answer.final) {
				if (pendingAnswers.length > 1) {
					var last = pendingAnswers.pop();
					pendingAnswers[index] = last;
				} else {
					pendingAnswers.pop();
				}
			// update the access time so that the request is kept alive
			} else {
				pendingAnswers[index].accessed = performance.now();
			}
			return true;
		}
		return false;
	};
	this.requestUpdate = function(subject) {
		var request = {
			type: "request",
			id: self.generateRequestId(),
			subject: subject,
			final: false
		};
		var future = new Future(function() {});
		var pending = {
			id: request.id,
			future: future,
			timeout: 30000,
			accessed: performance.now()
		};
		pendingAnswers.push(pending);
		connection.sendMessage(JSON.stringify(request));

		return future;
	};
	this.cleanup = function() {
		var now = performance.now();
		pendingAnswers = pendingAnswers.filter(function(pending) {
			var accessSpan = now - pending.accessed;
			if (pending.timeout < accessSpan) {
				pending.future.reject({ timeout: true });
				MessageLog.log("Request ".concat(pending.id, " timed out after ", accessSpan/1000, " seconds"));
				return false;
			} else {
				return true;
			}
		});
	};
}

function LiveWireService(MessageLog, $interval) {
	var self = this;
	this.connection = null;
	this.connectionOpen = false;
	this.port = 9234;

	this.pendingMessages = [];
	this.requestHandler = new LiveWireRequestHandler(MessageLog, this);
	this.updateHandler = new LiveWireUpdateHandler(MessageLog);

	this.serveCycle = null;
	this.serveInterval = 10000;

	this.isConnected = function() {
		if (self.connection == null) {
			return false;
		}
		return self.connectionOpen;
	};
	this.connectInProgress = function() {
		return self.connection == null && !self.connectionOpen;
	};

	this.connect = function() {
		if (self.isConnected()) {
			return;
		}
		MessageLog.log("Connecting to live server");
		var server = window.location.hostname;
		self.connection = new WebSocket("ws:" + server + ":" + self.port);
		self.connection.onopen = this.connected;
		self.connection.ondisconnected = this.disconnected;
		self.connection.onmessage = this.messageReceived;
		self.connection.onerror = this.connectionFailed;
		// serve cycle active when a connection is desired
		self.startServeCycle();
	};
	this.disconnect = function() {
		if (self.isConnected()) {
			self.connection.close();
			self.connection = null;
			self.connectionOpen = false;
		}
		self.stopServeCycle();
	};
	this.connected = function(ev) {
		// disconnect right away if disconnect has been called
		if (self.connection == null) {
			this.close();
			return;
		}
		self.connectionOpen = true;
		MessageLog.log("Connected to live server");

		// handle pending messages
		var toSend = self.pendingMessages;
		self.pendingMessages = [];
		toSend.forEach(function(msg) {
			self.sendMessage(msg);
		});


		// test
		self.requestUpdate({
			sender : "minority",
			type : "config",
			topic : "camera",
			position : "all",
			payload : "TFG56RT"
		}).resolve(function(answer) {
			MessageLog.log("wire got an answer: " + JSON.stringify(answer));
		});


	};
	this.disconnected = function(ev) {
		self.connectionOpen = false;
		self.connection = null;
		MessageLog.log("Disconnected from live server");
	};
	this.connectionFailed = function(ev) {
		self.connectionOpen = false;
		self.connection = null;
		self.stopCleanup();
		MessageLog.log("Lost connection to live server");
	};
	this.messageReceived = function(ev) {

		//console.log("wire received: " + ev.data);
		var msg = JSON.parse(ev.data);
		var handled = false;
		handled |= self.requestHandler.tryHandleAnswer(msg);
		handled |= self.updateHandler.tryHandleUpdate(msg);
		if (!handled) {
			MessageLog.log("Live service could not make sense of message: " + ev.data);
		}
	};
	this.sendMessage = function(msg) {
		//console.log("sending: " + msg);
		if (self.isConnected()) {
			self.connection.send(msg);
		} else {
			self.pendingMessages.push(msg);
		}
	};
	this.requestUpdate = function(subject) {
		return self.requestHandler.requestUpdate(subject);
	};
	this.receiveUpdate = function() {
		return self.receiveHandler.receiveUpdate();
	};
	this.startServeCycle = function() {
		if (self.cleanupCycle != null) {
			self.stopServeCycle();
		}
		self.serveCycle = $interval(self.serve, self.serveInterval);
	};
	this.stopServeCycle = function() {
		if (self.serveCycle == null) {
			return;
		}
		$interval.cancel(self.serveCycle);
		self.serveCycle = null;
	};
	this.serve = function() {
		// reconnecting
		if (!self.isConnected()) {
			if (self.connectInProgress()) {
				MessageLog.log("Still trying to connect to live server");
			} else {
				MessageLog.log("Trying to reconnect to live server");
				self.connect();
			}
		}
		// cleanup
		self.requestHandler.cleanup();
	};
	// start the connection
	this.connect();
};

var module = angular.module("liveConnection");
module.service("LiveWireService", LiveWireService);
// TODO sort out the necessary ones
LiveWireService.$inject = ["MessageLog", "$interval"];
//LiveWireRequestHandler.$inject = ["MessageLog"];

