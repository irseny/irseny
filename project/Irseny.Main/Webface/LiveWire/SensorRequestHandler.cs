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

			int sensorStart = -2;
			int sensorEnd = 0;
			bool isGetRequest;
			string type = JsonString.ParseString(subject.GetTerminal("type", "error"), "error");
			if (type.Equals("get")) {
				isGetRequest = true;
			} else if (type.Equals("post")) {
				isGetRequest = false;
			} else {
				return HttpStatusCode.BadRequest;
			}

			string position = JsonString.ParseString(subject.GetTerminal("position", "all"), "all");
			if (position.Equals("all")) {
				sensorStart = 0;
				sensorEnd = 15; // TODO replace with actual capacity
			} else {
				sensorStart = JsonString.ParseInt(position, -1);
				sensorEnd = sensorStart;
			}
			if (sensorStart < 0) {
				return HttpStatusCode.NotFound;
			}

			var responseSubject = JsonString.CreateDict();
			responseSubject.AddTerminal("type", JsonString.StringifyString("post"));
			responseSubject.AddTerminal("topic", JsonString.StringifyString("camera"));
			responseSubject.AddTerminal("position", JsonString.StringifyString(position));
			response.AddJsonString("subject", responseSubject);

			if (isGetRequest) {
				return RespondGet(subject, responseSubject, sensorStart, sensorEnd);
			} else {
				if (sensorStart != sensorEnd) {
					return HttpStatusCode.BadRequest;
				}
				HttpStatusCode status = RespondPost(subject, responseSubject, sensorStart);
				// not only send the result to one recipient but create an update for all clients
				if (status == HttpStatusCode.OK) {
					var update = new JsonString(response);
					update.RemoveTerminal("requestId");
					update.AddTerminal("target", "\"all\"");
					server.PostUpdate(update);
				}
				return status;
			}
		}

		private HttpStatusCode RespondGet(JsonString requestSubject, JsonString responseSubject, int sensorStart, int sensorEnd) {
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
						EquipmentSettings settings = capture.GetSettings();
						entry.AddTerminal("inuse", "true");
						entry.AddJsonString("settings", EquipmentSettings.ToJson(settings));
					} else {
						entry.AddTerminal("inuse", "false");
					}
					data.AddJsonString(string.Empty, entry);
				}
				readySignal.Set();
			});
			readySignal.WaitOne();
			lock (dataSync) {
				responseSubject.AddJsonString("data", data);
			}
			return HttpStatusCode.OK;
		}
		private HttpStatusCode RespondPost(JsonString requestSubject, JsonString responseSubject, int sensorIndex) {
			object dataSync = new object();
			// first read the data from the data array
			JsonString data = requestSubject.GetJsonString("data");
			if (data == null || data.Type != JsonStringType.Array || data.Array.Count < 1) {
				return HttpStatusCode.BadRequest;
			}
			var targetMap = new Dictionary<int, EquipmentSettings>();
			for (int i = 0; i <= 0; i++) {
				JsonString sensor = data.GetJsonString(0);
				if (sensor == null) {
					return HttpStatusCode.BadRequest;
				}
				bool used = JsonString.ParseBool(sensor.GetTerminal("inuse", "false"), false);
				if (used) {
					JsonString jSettings = sensor.GetJsonString("settings");
					if (jSettings == null) {
						return HttpStatusCode.BadRequest;
					}
					var settings = EquipmentSettings.FromJson(jSettings, typeof(SensorProperty));
					targetMap.Add(sensorIndex + i, settings);
				}
			}

			// secondly satisfy the request 
			var status = HttpStatusCode.OK;
			var resultMap = new Dictionary<int, EquipmentSettings>();
			var readySignal = new ManualResetEvent(false);
			CaptureSystem.Instance.Invoke((object sender, EventArgs args) => {
				var system = (CaptureSystem)sender;
				for (int iSource = sensorIndex; iSource <= sensorIndex; iSource++) {
					// first determine which operations to perform
					int iTarget = iSource;
					bool connect = false;
					bool disconnect = false;
					bool start = false;
					bool stop = false;
					bool apply = false;


					var entry = data.GetJsonString(0);
					ISensorBase sensor = system.GetSensor(sensorIndex);
					EquipmentSettings sourceSettings = null;
					SensorType sourceType = SensorType.Webcam;
					if (sensor != null) {
						sourceSettings = ((WebcamCapture)sensor).GetSettings();
						int iType = sourceSettings.GetInteger(SensorProperty.Type, -1);
						if (iType > -1) {
							sourceType = (SensorType)iType;
						}
					}
					EquipmentSettings targetSettings = null;
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
							targetSettings = ((WebcamCapture)sensor).GetSettings();
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
							targetSettings = ((WebcamCapture)sensor).GetSettings();
						}
					}

					if (stop && success) {
						if (system.StopSensor(iTarget)) {
							LogManager.Instance.LogMessage(this, "Stopped sensor " + iTarget);
						} else {
							targetSettings = ((WebcamCapture)sensor).GetSettings();
							success = false;
							LogManager.Instance.LogError(this, "Failed to stop sensor " + iTarget);
						} 
						resultMap[iTarget] = targetSettings;
					}
					if (apply && success) {
						if (sensor != null) {
							if (((WebcamCapture)sensor).ApplySettings(targetSettings)) {
								LogManager.Instance.LogMessage(this, "Applied new settings to sensor " + iTarget);
							} else {
								LogManager.Instance.LogError(this, "Failed to apply new settings to sensor " + iTarget);
							}
							targetSettings = ((WebcamCapture)sensor).GetSettings();
							resultMap[iTarget] = targetSettings;
						}
					}

					if (start && success) {
						if (system.StartSensor(iSource)) {
							LogManager.Instance.LogMessage(this, "Started sensor " + iSource);
							targetSettings = ((WebcamCapture)sensor).GetSettings();
							resultMap[iTarget] = targetSettings;
						} else {
							LogManager.Instance.LogError(this, "Failed to start sensor " + iSource);
							targetSettings = ((WebcamCapture)sensor).GetSettings();
							resultMap[iTarget] = targetSettings;
							success = false;
						}
					}
					if (!success) {
						status = HttpStatusCode.InternalServerError;
					}
				}
				readySignal.Set();
			});
			// lastly create an update for all clients
			if (!readySignal.WaitOne(10000)) {
				return HttpStatusCode.InternalServerError;
			}
			if (status != HttpStatusCode.OK) {
				return status;
			}
			//var responseSubject = new JsonString(subject);
			{
				var updateData = JsonString.CreateArray();
				{
					for (int i = sensorIndex; i <= sensorIndex; i++) {
						var entry = JsonString.CreateDict();
						EquipmentSettings settings;
						if (resultMap.TryGetValue(i, out settings)) {
							entry.AddTerminal("inuse", "true");
							entry.AddJsonString("settings", EquipmentSettings.ToJson(settings));
						} else {
							entry.AddTerminal("inuse", "false");
						}
						updateData.AddJsonString(string.Empty, entry);
					}
				}
				responseSubject.AddJsonString("data", updateData);
			}
			return HttpStatusCode.OK;
		}
	}
}

