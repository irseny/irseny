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

/**
 * Handles messages going through LiveWireService.
 * The helper functions contain most of the message parsing and processing logic
 * of the functionality provided by LiveWireService.
 */
function LiveWireRequestHandler(MessageLog, connection) {
	var self = this;
	var pendingRequests = [];
	var nextRequestId = 0;
	var observable = new Observable();
	var clientOrigin = -1;
	var serverOrigin = 0;

	/**
	 * Generates a new, unique request ID.
	 * @return {number} id
	 */
	function generateRequestId() {
		return nextRequestId++;
	};
	/**
	 * Handles a message that was received and parsed.
	 * Checks the status of the messsage, outputs error information
	 * and decides further actions for message processing.
	 * upon its type.
	 * @param {Object} message message
	 * @return {boolean} whether the message could be processed or an error occured
	 */
	this.handleMessage = function(message) {
		var badStatus = false;
		if (message.status != undefined && message.status != 200) {
			var suffix = "with status code ";
			switch (message.status) {
			case undefined:
				suffix = "";
			break;
			case 200:
				suffix = suffix.concat("200 - OK");
			break;
			case 400:
				suffix = suffix.concat("400 - Bad request");
			break;
			case 404:
				suffix = suffix.concat("404 - Not found");
			break;
			case 500:
				suffix = suffix.concat("500 - Internal server error");
			break;
			default:
				suffix = suffix.concat(message.status);
			break;
			}
			MessageLog.logError("LiveWire received error message ".concat(
				suffix, "\n", JSON.stringify(message)));
			badStatus = true;
		}
		// a typical response for a client's request contains an origin that matches the origin of the client
		// responses for origin request are a special case
		// the origin of the client is in invalid state and the server origin in the response matches 0
		// here we assume require that all responses sent from the server have a corresponding request
		var isRequest = false;
		if (message.requestId != undefined) {
			isRequest = true;
		}
		// a typical message comes with a subject
		// unless it contain a bad status code and cannot be processed anyway
		if (!badStatus && message.subject == undefined) {
			MessageLog.logError("LiveWire received message with missing subject:\n".concat(
				JSON.stringify(message)));
		}
		if (isRequest) {
			return handleResponse(message);
		} else {
			return handleUpdate(message);
		}
	};
	/**
	 * Handles an update message.
	 * The subject of the message is forwarded to all update observers in the process.
	 * @param {Object} update message
	 * @return {boolean} whether the message was processed successfully
	 */
	function handleUpdate(update) {
		if (update.status != undefined && update.status != 200) {
			MessageLog.logError("LiveWire update failed");
			return update.subject == undefined;
		}
		if (update.subject == undefined) {
			MessageLog.logError("LiveWire received an update without a subject:\n".concat(
				JSON.stringify(update), "\nDropping packet"));
			return false;
		}

		observable.notify(update.subject);
		return true;
	};
	/**
	 * Handles a message with request ID attached.
	 * The subject of the message is forwarded to the corresponding request future.
	 * @param {Object} response message that is likely to correspond to a request.
	 * @return {boolean} whether the message was processed successfully
	 */
	function handleResponse(response) {
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
	/**
	 * Generates a request with the given and sends it to the server.
	 * @param {Object} subject the content of the request
	 * @return {Object} future that will be resolved or rejected when the server sends a response or the request times out
	 */
	this.sendRequest = function(subject) {
		var request = {
			status: 200,
			requestId: generateRequestId(),
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
	/**
	 * Generates an update message which is send to the server.
	 * @param {Object} subject the content of the update
	 */
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
	/**
	 * Returns the observer object that is notified of all update messages.
	 * @return {Object} update observable
	 */
	this.getUpdateObserver = function() {
		return observable;
	};
	/**
	 * Performs reoccuring cleanup operations.
	 * Rejects timed out requests.
	 */
	this.cleanup = function() {
		var now = performance.now();
		pendingRequests = pendingRequests.filter(function(pending) {
			var accessSpan = now - pending.accessed;
			if (pending.timeout < accessSpan) {
				pending.future.reject({timeout: true});
				MessageLog.logWarning("Livewire request\n".concat(JSON.stringify(pending.request), "\ntimed out after ",
					Math.floor(accessSpan/1000), " seconds and ", pending.responseNo, " responses"));
				return false;
			} else {
				return true;
			}
		});
	};
	/**
	 * Requuests a unique client ID from the server
	 * and handles the response.
	 * If this operation fails then the messages send to the server may contain invalid properties,
	 * which might start to produce issues in the future.
	 */
	this.requestOrigin = function() {
		var subject = {
			type: "get",
			topic: "origin"
		};
		var future = self.sendRequest(subject);
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
				MessaeLog.logError("LiveWire received unexpected origin response\n".concat(result, "\nLeaving unconfigured"));
			}
		});
		future.catch(function(reason) {
			MessageLog.logError("LiveWire did not received an origin response from the server. Leaving unconfigured");
		});
	};
	/**
	 * Resets the client and server IDs received previously.
	 */
	this.clearOrigin = function() {
		clientOrigin = -1;
		serverOrigin = 0;
	};
}
/**
 * Serves a WebSocket connection with the server to communicate all equipment synchronization.
 * @param {Object} MessageLog
 * @param {Object} $interval
 */
function LiveWireService(MessageLog, $interval) {
	var self = this;
	var connection = undefined;
	var connectionOpen = false;
	const port = 9243;

	var pendingMessages = [];
	var requestHandler = new LiveWireRequestHandler(MessageLog, this);

	var serveCycle = undefined;
	const serveInterval = 5000;


	/**
	 * Indicates whether a connection with the server has been initiated.
	 * @return {boolean} whether the WebSocket connection to the server is open
	 */
	this.isConnected = function() {
		if (connection == undefined) {
			return false;
		}
		return connectionOpen;
	};
	/**
	 * Inicates whether a server connection is being initiated.
	 * @return {boolean} whether the WebSocket connection is opened
	 */
	this.connectInProgress = function() {
		return connection != undefined && !connectionOpen;
	};
	/**
	 * Connects this instance to the server.
	 */
	this.connect = function() {
		if (self.isConnected()) {
			return;
		}
		MessageLog.logInfo("LiveWire establishing a connection");
		var server = window.location.hostname;
		connection = new WebSocket("ws:" + server + ":" + port);
		connection.onopen = listenConnected;
		connection.ondisconnected = listenDisconnected;
		connection.onmessage = listenMessageReceived;
		connection.onerror = listenConnectionFailed;
		// serve cycle active when a connection is desired
		startServeCycle();
	};
	/**
	 * Disconnects this instance from the server.
	 */
	this.disconnect = function() {
		if (self.isConnected()) {
			connection.close();
			connection = undefined;
			connectionOpen = false;
		}
		stopServeCycle();
		requestHandler.clearOrigin();
	};
	/**
	 * Finishes the connection initiation process.
	 * Called when WebSocket connection initiation has succeeded.
	 */
	function listenConnected(ev) {
		// disconnect right away if disconnect has been called
		if (connection == undefined) {
			this.close();
			return;
		}
		connectionOpen = true;
		MessageLog.logInfo("LiveWire connected to server");
		requestHandler.requestOrigin();
		// handle pending messages
		var toSend = pendingMessages;
		pendingMessages = [];
		toSend.forEach(function(msg) {
			self.sendMessage(msg);
		});
	};
	/**
	 * Finishes the disconnecting process.
	 * Called whether the WebSocket connection has closed.
	 */
	function listenDisconnected(ev) {
		connectionOpen = false;
		connection = undefined;
		requestHandler.clearOrigin();
		MessageLog.logWarning("LiveWire disconnected from server");
	};
	/**
	 * Finishes the connection initiation process momentarily.
	 * Called when the WebSocket connection initiation has failed.
	 */
	function listenConnectionFailed(ev) {
		connectionOpen = false;
		connection = undefined;
		requestHandler.clearOrigin();
		MessageLog.logError("LiveWire lost server connection");
	};
	/**
	 * Handles a message from the server.
	 * Called when a new message is received through the WebSocket connection.
	 */
	function listenMessageReceived(ev) {
		var msg = undefined;
		try {
			msg = JSON.parse(ev.data);
		} catch (error) {
			MessageLog.logError("LiveWire failed to parse message\n".concat(
				ev.data, "\n", error));
		}
		if (msg != undefined) {
			var handled = requestHandler.handleMessage(msg);
			if (!handled) {
				MessageLog.logError("LiveWire could not make sense of message\n".concat(ev.data));
			}
		}
	};
	/**
	 * Sends a message to the server through the WebSocket connection
	 * If the connection is disconnected, the message is cached to
	 * be send later instead.
	 * @param {Object} msg message to be sent
	 */
	this.sendMessage = function(msg) {
		//console.log("sending: " + msg);
		if (self.isConnected()) {
			connection.send(msg);
		} else {
			pendingMessages.push(msg);
		}
	};
	/**
	 * Sends a request message to the server.
	 * @param {Object} subject message content
	 * @return {Object} request future that is resolved or rejected when the server responds
	 */
	this.sendRequest = function(subject) {
		return requestHandler.sendRequest(subject);
	};
	/**
	 * Sends an update message to the server.
	 * @param {Object} subject message content
	 */
	this.sendUpdate = function(subject) {
		requestHandler.sendUpdate(subject);
	};
	/**
	 * Returns the observerable that is notified on receiving update messages.
	 * This is the main mechanism for receiving update messages.
	 * @return {Object} update message observable
	 */
	this.getUpdateObserver = function() {
		return requestHandler.getUpdateObserver();
	};
	/**
	 * Starts the serve cycle so that it is executed recurrently.
	 */
	function startServeCycle() {
		if (serveCycle != undefined) {
			stopServeCycle();
		}
		serveCycle = $interval(serve, serveInterval);
	};
	/**
	 * Stops the serve cycle.
	 */
	function stopServeCycle() {
		if (serveCycle == undefined) {
			return;
		}
		$interval.cancel(serveCycle);
		serveCycle = undefined;
	};
	/**
	 * Executes reoccuring connection and cleanup operations.
	 */
	function serve() {
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
		requestHandler.cleanup();
	};
	// start the connection
	this.connect();
};

var module = angular.module("liveConnection");
module.service("LiveWireService", LiveWireService);
// TODO sort out the necessary ones
LiveWireService.$inject = ["MessageLog", "$interval"];
//LiveWireRequestHandler.$inject = ["MessageLog"];

