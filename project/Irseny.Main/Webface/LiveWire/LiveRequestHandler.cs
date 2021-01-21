using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Net;
using Irseny.Core.Util;
using Irseny.Core.Sensors.VideoCapture;
using Irseny.Core.Log;

namespace Irseny.Main.Webface.LiveWire {
	public class LiveRequestHandler {

		LiveWireServer server;
		int clientOrigin;

		public LiveRequestHandler(LiveWireServer server, int clientOrigin) {
			if (server == null) throw new ArgumentNullException("server");
			this.server = server;
			this.clientOrigin = clientOrigin;
		}
		public JsonString Respond(JsonString request) {
			if (request == null) throw new ArgumentNullException("request");
			var signal = new ManualResetEvent(false);
			var status = HttpStatusCode.OK;
			var response = JsonString.CreateDict();
			do {
				// set basic information before we drop out of this scope
				// this could alternatively also be read from the request
				response.AddTerminal("status", JsonString.Stringify((int)HttpStatusCode.OK));
				// the client can read the origin information and derive from that
				// if they can apply the optional request id
				response.AddTerminal("origin", JsonString.Stringify(clientOrigin));
				// preliminarily we intend to send the answer to the origin only
				// but this might get overwritten later
				response.AddTerminal("target", JsonString.Stringify(clientOrigin));
				if (request.Type != JsonStringType.Dict) {
					status = HttpStatusCode.BadRequest;
					break;
				}
				// the optional request id is only useful at the origin client side
				// for request-answer matching
				int requestId = JsonString.ParseInt(request.GetTerminal("requestId", "-1"), -1);
				if (requestId > -1) {
					response.AddTerminal("requestId", JsonString.Stringify(requestId));
				}

				JsonString subject = request.GetJsonString("subject");
				if (subject == null) {
					status = HttpStatusCode.BadRequest;
					break;
				}
				string topic = JsonString.ParseString(subject.GetTerminal("topic", "error"), "error");
				// answer by topic
				if (topic.Equals("sensor") || topic.Equals("camera")) {
					status = new SensorRequestHandler(server).Respond(subject, response);
				} else if (topic.Equals("tracker")) {
					status = new TrackerRequestHandler(server).Respond(subject, response);
				} else if (topic.Equals("sensorCapture")) {
					status = new SensorCaptureRequestHandler(server, clientOrigin).Respond(subject, response);
				} else if (topic.Equals("origin")) {
					status = new OriginRequestHandler(LiveWireServer.ServerOrigin, clientOrigin).Respond(subject, response);
				} else {
					status = HttpStatusCode.BadRequest;
					break;
				}
			} while (false);
			// handle errors which occured by breaking out of scope above
			if (status != HttpStatusCode.OK) {
				CreateErrorResponse(request, response, status);
				LogManager.Instance.LogError(this, string.Format("Request\n{0}\nfailed with status code {1}",
					request.ToString(), status));
			}
			return response;
		}
		private void CreateErrorResponse(JsonString request, JsonString response, HttpStatusCode status) {
			// add status code information
			if (response.Type == JsonStringType.Array) {
				// unexpected -> give minimal response
				var error = JsonString.CreateDict();
				error.AddTerminal("status", JsonString.Stringify((int)status));
				response.AddJsonString(string.Empty, error);
				return;
			}
			response.AddTerminal("status", JsonString.Stringify((int)status));
			// add the original message to failed updates
			// that way it is easier to recognize the failed operation
			if (response.GetTerminal("requestId", string.Empty).Length == 0) {
				response.AddJsonString("request", new JsonString(request));
			}
		}
	}
}

