


function LiveWireUpdateHandler(MessageLog) {
	var observable = new Observable();

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
	var pendingRequests = [];
	var nextRequestId = 0;
	var observable = new Observable();
	var clientOrigin = -1;
	var serverOrigin = 0;


	this.generateRequestId = function() {
		return nextRequestId++;
	};
	this.handleMessage = function(message) {
		var badStatus = false;
		if (message.status != undefined && message.status != 200) {
			MessageLog.logError("LiveWire request failed with status code ".concat(
				message.status, " in response: ", JSON.stringify(message)));
			badStatus = true;
		}

		// a typical response for a client's request contains an origin that matches the origin of the client
		// responses for origin request are a special case
		// the origin of the client is in invalid state and the server origin in the response matches 0
		var isRequest = false;
		if (message.requestId != undefined) {
			if (message.origin == clientOrigin) {
				isRequest = true;
			}
			if (clientOrigin < 0 && message.subject.data.serverOrigin == serverOrigin) {
				isRequest = true;
			}
		}

		// a message without a subject cannot be handled
		// unless it is a response with an error status code
		if (message.subject == undefined && !(badStatus && isRequest)) {
			MessageLog.logError("LiveWire received message with missing subject: ".concat(
				JSON.stringify(message)));
			return false;
		}
		if (isRequest) {
			return self.handleResponse(message);
		} else {
			return self.handleUpdate(message);
		}
	};
	this.handleUpdate = function(update) {
		if (update.subject == undefined) {
			return false;
		}
		observable.notify(update.subject);
		return true;
	};
	this.handleResponse = function(response) {
		// try to match the answer to a previous request
		// in order to resolve the request
		var index = pendingRequests.findIndex(function(pending) {
			return pending.requestId == response.requestId;
		});
		if (index < 0) {
			MessageLog.logError("LiveWire has not sent a matching request for response ".concat(response.requestId, ". Dropping packet"));
			return false;
		}
		// in case of a bad status code in the response reject the request and
		// give more information with regards to which request went wrong
		if (response.status != undefined && response.status != 200) {
			var pending = pendingRequests[index];
			MessageLog.logError("for request ".concat(JSON.stringify(pending.request)));
			pending.future.reject({status: response.status});
		} else {
			// otherwise the request can be resolved
			// and the response has a subject (tested above)
			pendingRequests[index].future.resolve(response.subject);
		}
		// remove the request if no more responses are to come
		if (response.final == undefined || response.final == true) {
			if (index < pendingRequests.length - 1) {
				var last = pendingRequests.pop();
				pendingRequests[index] = last;
			} else {
				pendingRequests.pop();
			}
		} else {
			// update the access time so that the request is kept alive
			pendingRequests[index].accessed = performance.now();
			pendingRequests[index].responseNo  += 1;
		}
		return true;
	};
	this.requestUpdate = function(subject) {
		var request = {
			status: 200,
			requestId: self.generateRequestId(),
			origin: clientOrigin,
			target: serverOrigin,
			subject: subject,
			final: true
		};
		var future = new Future();
		var pending = {
			request: request,
			requestId: request.requestId,
			responseNo: 0,
			future: future,
			timeout: 25000,
			accessed: performance.now()
		};
		pendingRequests.push(pending);
		connection.sendMessage(JSON.stringify(request));

		return future;
	};
	this.sendUpdate = function(subject) {
		var message = {
			status: 200,
			origin: clientOrigin,
			target: serverOrigin,
			subject: subject,
			final: true
		};
		connection.sendMessage(JSON.stringify(message));
	};
	this.receiveUpdate = function() {
		return observable;
	};
	this.cleanup = function() {
		var now = performance.now();
		pendingRequests = pendingRequests.filter(function(pending) {
			var accessSpan = now - pending.accessed;
			if (pending.timeout < accessSpan) {
				pending.future.reject({timeout: true});
				MessageLog.logWarning("Request ".concat(pending.requestId, " timed out after ",
					Math.floor(accessSpan/1000), " seconds and ", pending.responseNo, " responses: ", JSON.stringify(pending.request)));
				return false;
			} else {
				return true;
			}
		});
	};
	this.requestOrigin = function() {
		var subject = {
			type: "get",
			topic: "origin"
		};
		var future = self.requestUpdate(subject);
		future.then(function(result) {
			var accepted = false;
			do {
				if (!(result.data.clientOrigin > 0)) {
					break;
				}
				if (!(result.data.serverOrigin >= 0)) {
					break;
				}
				clientOrigin = result.data.clientOrigin;
				// the server origin always stays 0
				MessageLog.logInfo("LiveWire client was assigned origin id ".concat(clientOrigin));
				accepted = true;
			} while (false);
			if (!accepted) {
				MessaeLog.logError("LiveWire received unexpected origin response ".concat(result, ". Leaving unconfigured"));
			}
		});
		future.else(function(reason) {
			MessageLog.logError("LiveWire did not received an origin response from the server");
		});
	};
	this.clearOrigin = function() {
		clientOrigin = -1;
		serverOrigin = -1;
	};
}

function LiveWireService(MessageLog, $interval) {
	var self = this;
	this.connection = null;
	this.connectionOpen = false;
	this.port = 9234;

	this.pendingMessages = [];
	this.requestHandler = new LiveWireRequestHandler(MessageLog, this);

	this.serveCycle = null;
	this.serveInterval = 5000;

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
		MessageLog.logInfo("LiveWire establishing a connection");
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
		self.requestHandler.clearOrigin();
	};
	this.connected = function(ev) {
		// disconnect right away if disconnect has been called
		if (self.connection == null) {
			this.close();
			return;
		}
		self.connectionOpen = true;
		MessageLog.logInfo("LiveWire connected to server");
		self.requestHandler.requestOrigin();
		// handle pending messages
		var toSend = self.pendingMessages;
		self.pendingMessages = [];
		toSend.forEach(function(msg) {
			self.sendMessage(msg);
		});
	};
	this.disconnected = function(ev) {
		self.connectionOpen = false;
		self.connection = null;
		self.requestHandler.clearOrigin();
		MessageLog.logWarning("LiveWire disconnected from server");
	};
	this.connectionFailed = function(ev) {
		self.connectionOpen = false;
		self.connection = null;
		self.stopServeCycle();
		self.requestHandler.clearOrigin();
		MessageLog.logError("LiveWire lost server connection");
	};
	this.messageReceived = function(ev) {
		var msg = null;
		try {
			msg = JSON.parse(ev.data);
		} catch (error) {
			MessageLog.logError("LiveWire failed to parse message: ".concat(
				ev.data, " ", error));
		}
		if (msg != null) {
			var handled = self.requestHandler.handleMessage(msg);
			if (!handled) {
				MessageLog.logError("LiveWire could not make sense of message: " + ev.data);
			}
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
	this.sendUpdate = function(subject) {
		self.requestHandler.sendUpdate(subject);
	};
	this.receiveUpdate = function() {
		return self.requestHandler.receiveUpdate();
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
				MessageLog.logInfo("Still trying to connect to live server");
			} else {
				MessageLog.logInfo("Trying to reconnect to live server");
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

