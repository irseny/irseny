using System;
using System.Net;
using Irseny.Core.Util;
using System.Threading;
using Irseny.Core.Sensors;
using Irseny.Core.Sensors.VideoCapture;
using System.Collections.Generic;
using Irseny.Core.Log;

namespace Irseny.Main.Webface.LiveWire {
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
							JsonString jSettings = sensor.GetJsonString("settings");
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
						for (int iSource = sensorStart; iSource <= sensorEnd; iSource++) {
							// first determine which operations to perform
							int iTarget = iSource;
							bool connect = false;
							bool disconnect = false;
							bool start = false;
							bool stop = false;
							bool apply = false;


							var entry = data.GetJsonString(iSource);
							ISensorBase sensor = system.GetSensor(iSource);
							SensorSettings sourceSettings = null;
							SensorType sourceType = SensorType.Webcam;
							if (sensor != null) {
								sourceSettings = ((WebcamCapture)sensor).GetSettings();
								int iType = sourceSettings.GetInteger(SensorProperty.Type, -1);
								if (iType > -1) {
									sourceType = (SensorType)iType;
								}
							}
							SensorSettings targetSettings = null;
							SensorType targetType = SensorType.Webcam;
							if (targetMap.TryGetValue(iSource, out targetSettings)) {
								int iType = targetSettings.GetInteger(SensorProperty.Type, -1);
								if (iType > -1) {
									targetType = (SensorType)iType;
								}
							}
							if (sourceSettings == null && targetSettings != null) {
								connect = true;
							} else if (sourceSettings != null && targetSettings == null) {
								disconnect = true;
							} else if (sourceSettings == null && targetSettings == null) {
								// nothing to do
							} else {
								if (sourceType != targetType) {
									disconnect = true;
									connect = true;
								}
								apply = true;
							}
							if (targetSettings != null) {
								int iTargetCapturing = targetSettings.GetInteger(SensorProperty.Capturing, -1);
								if (iTargetCapturing < 0) {
									// ignore if not available
								} else if (iTargetCapturing != 0) {
									start = true;
								} else {
									stop = true;
								}
							}
							// TODO make restrictions or otherwise cover remaining possible cases
							bool success = true;
							// perform the operations which were activated above
							if (disconnect && success) {
								if (system.DisconnectSensor(iSource)) {
									LogManager.Instance.LogMessage(this, "Disconnected sensor " + iSource);
									sensor = null;
									// already removed from result map
								} else {
									// nothing changed
									LogManager.Instance.LogError(this, "Failed to disconnect sensor " + iSource);
									resultMap[iSource] = sourceSettings;
									success = false;
								}
							}
							if (connect && success) {
								sensor = new WebcamCapture(targetSettings);
								int iNext = system.ConnectSensor(sensor, iTarget);
								if (iNext == iTarget) {
									LogManager.Instance.LogMessage(this, "Connected sensor " + iNext);
									targetSettings = sensor.GetSettings();
									resultMap[iTarget] = targetSettings;
								} else if (iNext < 0) {
									// failed, leave result map entry empty
									LogManager.Instance.LogError(this, "Failed to connect new sensor");
									sensor = null;
									success = false;
								} else {
									LogManager.Instance.LogMessage(this, "Connected sensor " + iNext);
									// sensor connected with different index, which we can accept
									iTarget = iNext;
									resultMap[iNext] = targetSettings;
									targetSettings = sensor.GetSettings();
								}
							}

							if (stop && success) {
								if (system.StopSensor(iTarget)) {
									LogManager.Instance.LogMessage(this, "Stopped sensor " + iTarget);
								} else {
									targetSettings = sensor.GetSettings();
									success = false;
									LogManager.Instance.LogError(this, "Failed to stop sensor " + iTarget);
								} 
								resultMap[iTarget] = targetSettings;
							}
							if (apply && success) {
								if (sensor != null) {
									if (sensor.ApplySettings(targetSettings)) {
										LogManager.Instance.LogMessage(this, "Applied new settings to sensor " + iTarget);
									} else {
										LogManager.Instance.LogError(this, "Failed to apply new settings to sensor " + iTarget);
									}
									targetSettings = sensor.GetSettings();
									resultMap[iTarget] = targetSettings;
								}
							}
								
							if (start && success) {
								if (system.StartSensor(iSource)) {
									LogManager.Instance.LogMessage(this, "Started sensor " + iSource);
									targetSettings = sensor.GetSettings();
									resultMap[iTarget] = targetSettings;
								} else {
									LogManager.Instance.LogError(this, "Failed to start sensor " + iSource);
									targetSettings = sensor.GetSettings();
									resultMap[iTarget] = targetSettings;
								}
							}
						}
						readySignal.Set();
					});
					// lastly create an update for all clients
					if (!readySignal.WaitOne(10000)) {
						status = HttpStatusCode.InternalServerError;
						break;
					}

					var responseSubject = new JsonString(subject);
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
								updateData.AddJsonString(string.Empty, entry);
							}
						}
						responseSubject.AddJsonString("data", updateData);
					}
					response.AddJsonString("subject", responseSubject);
					
					var update = new JsonString(response);
					update.RemoveTerminal("requestId", false);
					update.AddTerminal("target", "\"all\"");


				}
			} while (false);
			return status;
		}
	}
}

