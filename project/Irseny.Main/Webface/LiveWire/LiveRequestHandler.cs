using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Net;
using Irseny.Core.Util;
using Irseny.Core.Sensors.VideoCapture;
using Irseny.Core.Log;

namespace Irseny.Main.Webface {
	public class LiveRequestHandler {
		
		public LiveRequestHandler () {
		}
		public JsonString Respond(JsonString request, int origin) {
			if (request == null) throw new ArgumentNullException("request");
			var signal = new ManualResetEvent(false);
			var status = HttpStatusCode.OK;
			var response = JsonString.CreateDict();
			do {
				// set basic information before we drop out of this scope
				// this could alternatively also be read from the request
				response.AddTerminal("status", StringifyTools.StringifyInt((int)HttpStatusCode.OK));
				// the client can read the origin information and derive from that
				// if they can apply the optional request id
				response.AddTerminal("origin", StringifyTools.StringifyInt(origin));
				// preliminarily we intend to send the answer to the origin only
				// but this might get overwritten later
				response.AddTerminal("target", StringifyTools.StringifyInt(origin));
				if (request.Type != JsonStringType.Dict) {
					status = HttpStatusCode.BadRequest;
					break;
				}
				// the optional request id is only useful at the origin client side
				// for request-answer matching
				int requestId = TextParseTools.ParseInt(request.GetTerminal("requestId", "-1"), -1);
				if (requestId > -1) {
					response.AddTerminal("requestId", StringifyTools.StringifyInt(requestId));
				}

				JsonString subject = request.TryGetJsonString("subject");
				if (subject == null) {
					status = HttpStatusCode.BadRequest;
					break;
				}
				string topic = TextParseTools.ParseString(subject.GetTerminal("topic", "error"), "error");
				// answer by topic
				if (topic.Equals("camera")) {
					status = new CameraRequestHandler().Respond(subject, response);
				} else if (topic.Equals("sensorCapture")) {
					status = new SensorCaptureRequestHandler().Respond(subject, response);
				} else if (topic.Equals("origin")) {
					status = new OriginRequestHandler(LiveWireServer.ServerOrigin, origin).Respond(subject, response);
				} else {
					status = HttpStatusCode.BadRequest;
					break;
				}
			} while (false);
			// handle errors which occured by breaking out of scope above
			if (status != HttpStatusCode.OK) {
				CreateErrorResponse(response, status);
				LogManager.Instance.LogError(this, string.Format("Request\n{0}\nfailed with status code {1}",
					request.ToString(), status));
			}
			return response;
		}
//		private void AnswerOriginRequest(JsonString subject, string type, int origin, JsonString answer) {
//			// send the client and server origin ids as subject
//			// while the type must be 'get' which is answered with 'post'
//			HttpStatusCode status = HttpStatusCode.OK;
//			do {
//				if (!type.Equals("get")) {
//					status = HttpStatusCode.Forbidden;
//					break;
//				}
//				answer.AddTerminal("type", StringifyTools.StringifyString("post"), true);
//				answer.AddTerminal("origin", StringifyTools.StringifyInt(LiveWireServer.ServerOrigin), true);
//				var sub = JsonString.CreateDict();
//				{
//					sub.AddTerminal("topic", StringifyTools.StringifyString("origin"));
//					var data = JsonString.CreateDict();
//					{
//						data.AddTerminal("serverOrigin", StringifyTools.StringifyInt(LiveWireServer.ServerOrigin)); // TODO read from server
//						data.AddTerminal("clientOrigin", StringifyTools.StringifyInt(origin));
//					}
//					sub.AddJsonString("data", data);
//				}
//				answer.AddJsonString("subject", sub);
//			} while (false);
//			if (status != HttpStatusCode.OK) {
//				CreateErrorAnswer(answer, status);
//			}
//		}
//		private void AnswerCameraRequest(JsonString subject, string type, JsonString answer) {
//			HttpStatusCode status = HttpStatusCode.OK;
//			int camStart = -2;
//			int camNo = 0;
//			do {
//				if (!type.Equals("get")) {
//					status = HttpStatusCode.BadRequest;
//					break;
//				}
//				string position = subject.GetTerminal("position", "-2");
//				if (position.Equals("all")) {
//					camStart = 0;
//					camNo = 16; // TODO replace with actual capacity
//				} else {
//					camStart = TextParseTools.ParseInt(position, -1);
//					camNo = 1;
//				}
//				if (camStart < 0) {
//					status = HttpStatusCode.NotFound;
//					break;
//				}
//				var subjectAnswer = JsonString.CreateDict();
//				{
//					subjectAnswer.AddTerminal("type", StringifyTools.StringifyString("post"));
//					subjectAnswer.AddTerminal("position", StringifyTools.StringifyString(position));
//					var data = JsonString.CreateArray();
//					var readySignal = new ManualResetEvent(false);
//					CaptureSystem.Instance.Invoke(delegate {
//						for (int i = camStart; i < camNo; i++) {
//							var entry = JsonString.CreateDict();
//							CaptureStream stream = CaptureSystem.Instance.GetStream(i);
//							if (stream == null) {							
//								entry.AddTerminal("status", StringifyTools.StringifyString("hidden"));
//							} else {
//								CaptureSettings settings = stream.GetSettings();
//								entry.AddJsonString("settings", settings.ToJson());
//								entry.AddTerminal("status", StringifyTools.StringifyString("active"));
//							}
//							data.AddJsonString(string.Empty, entry);
//						}
//						readySignal.Set();
//					});
//					readySignal.WaitOne();
//					subjectAnswer.AddJsonString("data", data);
//				}
//				answer.AddJsonString("subject", subjectAnswer);
//				answer.ToString();
//
//			} while (false);
//			if (status != HttpStatusCode.OK) {
//				CreateErrorAnswer(answer, status);
//			}
//			return;
//		}
		private void CreateErrorResponse(JsonString response, HttpStatusCode status) {
			switch (response.Type) {
			case JsonStringType.Dict:
				response.AddTerminal("status", StringifyTools.StringifyInt((int)status), true);
				break;
			default:
				// TODO pass client info to generate a sensible answer
				response.AddTerminal("status", StringifyTools.StringifyInt((int)status));
				break;
			}
		}
	}
}

