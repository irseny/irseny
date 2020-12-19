using System;
using System.Net;
using Irseny.Core.Util;
using System.Threading;
using Irseny.Core.Sensors;
using Irseny.Core.Sensors.VideoCapture;
using System.Collections.Generic;

namespace Irseny.Main.Webface {
	public class SensorRequestHandler {
		LiveWireServer server;

		public SensorRequestHandler(LiveWireServer server) {
			if (server == null) throw new ArgumentNullException("server");
			this.server = server;
		}
		public HttpStatusCode Respond(JsonString subject, JsonString response) {
			if (subject == null) throw new ArgumentNullException("subject");
			if (response == null) throw new ArgumentNullException("response");

			HttpStatusCode status = HttpStatusCode.OK;
			int sensorStart = -2;
			int sensorEnd = 0;
			do {
				bool isGetRequest;
				string type = TextParseTools.ParseString(subject.GetTerminal("type", "error"), "error");
				if (type.Equals("get")) {
					isGetRequest = true;
				} else if (type.Equals("post")) {
					isGetRequest = false;
				} else {
					status = HttpStatusCode.BadRequest;
					break;
				}

				string position = TextParseTools.ParseString(subject.GetTerminal("position", "all"), "all");
				if (position.Equals("all")) {
					sensorStart = 0;
					sensorEnd = 15; // TODO replace with actual capacity
				} else {
					sensorStart = TextParseTools.ParseInt(position, -1);
					sensorEnd = sensorStart;
				}
				if (sensorStart < 0) {
					status = HttpStatusCode.NotFound;
					break;
				}
				var subjectAnswer = JsonString.CreateDict();
				subjectAnswer.AddTerminal("type", StringifyTools.StringifyString("post"));
				subjectAnswer.AddTerminal("topic", StringifyTools.StringifyString("camera"));
				subjectAnswer.AddTerminal("position", StringifyTools.StringifyString(position));


				if (isGetRequest) {
					object dataSync = new object();
					JsonString data;
					lock (dataSync) { // TODO lock required?
						data = JsonString.CreateArray();
					}
					var readySignal = new ManualResetEvent(false);
					CaptureSystem.Instance.Invoke((object sender, EventArgs args) => {
						var system = (CaptureSystem)sender;
						for (int i = sensorStart; i <= sensorEnd; i++) {
							var entry = JsonString.CreateDict();
							ISensorBase sensor = system.GetSensor(i);

							if (sensor == null) {
								entry.AddTerminal("inuse", "false");
							} else if (sensor.SensorType == SensorType.Webcam) {
								var capture = (WebcamCapture)sensor;
								SensorSettings settings = capture.GetSettings();
								entry.AddTerminal("inuse", "true");
								entry.AddJsonString("settings", SensorSettings.ToJson(settings));

							} else {
								entry.AddTerminal("inuse", "false");
							}
							lock (dataSync) {
								data.AddJsonString(string.Empty, entry);
							}
						}
						readySignal.Set();
					});
					readySignal.WaitOne();
					lock (dataSync) {
						subjectAnswer.AddJsonString("data", data);
					}
					response.AddJsonString("subject", subjectAnswer);
				} else {
					object dataSync = new object();
					// first read the data from the data array
					JsonString data = subject.GetJsonString("data");
					if (data == null || data.Type != JsonStringType.Array || data.Array.Count <= sensorEnd) {
						status = HttpStatusCode.BadRequest;
						break;
					}
					var targetMap = new Dictionary<int, SensorSettings>();
					for (int i = 0; i <= sensorEnd - sensorStart; i++) {
						JsonString sensor = data.GetJsonString(i);
						bool used = TextParseTools.ParseBool(sensor.GetTerminal("inuse", "false"), false);
						if (used) {
							JsonString jSettings = data.GetJsonString("settings");
							if (jSettings == null) {
								status = HttpStatusCode.BadRequest;
								break;
							}
							var settings = SensorSettings.FromJson(jSettings, typeof(SensorProperty));
							targetMap.Add(sensorStart + i, settings);
						}
					}
					// secondly satisfy the request 
					var resultMap = new Dictionary<int, SensorSettings>();
					var readySignal = new ManualResetEvent(false);
					CaptureSystem.Instance.Invoke((object sender, EventArgs args) => {
						var system = (CaptureSystem)sender;
						for (int i = sensorStart; i < sensorEnd; i++) {
							bool connect = false;
							bool disconnect = false;
							bool start = false;
							bool stop = false;
							SensorType targetType;
							SensorSettings targetSettings;
							if (!targetMap.TryGetValue(i, out targetSettings)) {
								targetSettings = null;
							}
							var entry = data.GetJsonString(i);
							ISensorBase sourceSensor = system.GetSensor(i);
							SensorSettings sourceSettings = null;
							if (sourceSensor != null) {
								sourceSettings = ((WebcamCapture)sourceSensor).GetSettings();
							}
							if (sourceSensor == null && targetSettings != null) {
								connect = true;
							} else if (sourceSensor != null && targetSettings == null) {
								disconnect = true;
							} else if (sourceSensor == null && targetSettings == null) {
								// nothing to do
							} else {
								int iTargetType = targetSettings.GetInteger(SensorProperty.Type, -1);
								if (iTargetType > -1) {
									targetType = (SensorType)iTargetType;
									if (sourceSensor.SensorType != targetType) {
										disconnect = true;
										connect = true;
									}
								}
							}
							// TODO add rename capabilities
							// TODO handle settings update
							if (targetSettings != null) {
								int iTargetCapturing = targetSettings.GetInteger(SensorProperty.Capturing, 0);
								if (iTargetCapturing != 0) {
									start = true;
								} else {
									stop = true;
								}
							}
							if (disconnect) {
								if (system.DisconnectSensor(i)) {
									resultMap.Remove(i);
								} else {
									resultMap[i] = ((WebcamCapture)sourceSensor).GetSettings();
								}
							}
							if (connect) {
								var targetSensor = new WebcamCapture(targetSettings);
								if (system.ConnectSensor(targetSensor, i) != i) {
									resultMap.Remove(i);
								} else {
									resultMap[i] = targetSensor.GetSettings();
								}
							}
							if (stop) {
								if (system.StopSensor(i)) {

								} else {

								}
							}
							if (start) {
								if (system.StartSensor(i)) {
									// TODO update result settings
								} else {

								}
							}
						}
					});
					// lastly create an update for all clients
					var update = new JsonString(response);
					{
						update.RemoveTerminal("requestId", false);
						update.AddTerminal("target", "\"all\"");
						var updateSubject = new JsonString(subject);
						{
							var updateData = JsonString.CreateArray();
							{
								for (int i = sensorStart; i <= sensorEnd; i++) {
									var entry = JsonString.CreateDict();
									SensorSettings settings;
									if (resultMap.TryGetValue(i, out settings)) {
										entry.AddTerminal("inuse", "true");
										entry.AddJsonString("settings", SensorSettings.ToJson(settings));
									} else {
										entry.AddTerminal("inuse", "false");
									}
								}
							}
							updateSubject.AddJsonString("data", updateData);
						}
						update.AddJsonString("subject", updateSubject);
					}
					server.AddUpdate(update);
					// return a generic response
					var responseSubject = JsonString.CreateDict();
					{
						var responseData = JsonString.CreateArray();
						responseSubject.AddJsonString("data", responseData);
					}
					response.AddJsonString("subject", responseSubject);



				}




			} while (false);
			return status;
		}
	}
}

