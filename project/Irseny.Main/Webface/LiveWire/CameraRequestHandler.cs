using System;
using System.Net;
using Irseny.Core.Util;
using System.Threading;
using Irseny.Core.Sensors;
using Irseny.Core.Sensors.VideoCapture;

namespace Irseny.Main.Webface {
	public class CameraRequestHandler {
		public CameraRequestHandler () {}
		public HttpStatusCode Respond(JsonString subject, JsonString response) {
			if (subject == null) throw new ArgumentNullException("subject");
			if (response == null) throw new ArgumentNullException("response");

			HttpStatusCode status = HttpStatusCode.OK;
			int camStart = -2;
			int camNo = 0;
			do {
				string type = TextParseTools.ParseString(subject.GetTerminal("type", "error"), "error");
				if (!type.Equals("get")) {
					status = HttpStatusCode.BadRequest;
					break;
				}

				string position = TextParseTools.ParseString(subject.GetTerminal("position", "all"), "all");
				if (position.Equals("all")) {
					camStart = 0;
					camNo = 16; // TODO replace with actual capacity
				} else {
					camStart = TextParseTools.ParseInt(position, -1);
					camNo = 1;
				}
				if (camStart < 0) {
					status = HttpStatusCode.NotFound;
					break;
				}
				var subjectAnswer = JsonString.CreateDict();
				{
					subjectAnswer.AddTerminal("type", StringifyTools.StringifyString("post"));
					subjectAnswer.AddTerminal("topic", StringifyTools.StringifyString("camera"));
					subjectAnswer.AddTerminal("position", StringifyTools.StringifyString(position));
					var data = JsonString.CreateArray();
					var readySignal = new ManualResetEvent(false);
					CaptureSystem.Instance.Invoke(delegate {
						for (int i = camStart; i < camNo; i++) {
							var entry = JsonString.CreateDict();
							WebcamCapture stream = CaptureSystem.Instance.GetStream(i);
							if (stream == null) {							
								entry.AddTerminal("status", StringifyTools.StringifyString("unused"));
							} else {
								SensorSettings settings = stream.GetSettings();
								entry.AddJsonString("settings", settings.ToJson());
								entry.AddTerminal("status", StringifyTools.StringifyString("active"));
							}
							data.AddJsonString(string.Empty, entry);
						}
						readySignal.Set();
					});
					readySignal.WaitOne();
					subjectAnswer.AddJsonString("data", data);
				}
				response.AddJsonString("subject", subjectAnswer);

			} while (false);
			return status;
		}
	}
}

