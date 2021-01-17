using System;
using System.Net;
using Irseny.Core.Util;
using Irseny.Core.Sensors;
using Irseny.Core.Sensors.VideoCapture;
using System.Threading;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using System.IO;
using System.Diagnostics;
using Irseny.Core.Log;

namespace Irseny.Main.Webface.LiveWire {
	/// <summary>
	/// Clients do not receive captured sensor data by default. They need to request an optional subscription
	/// to this information which is then active for a limited time.
	/// This class handles such requests and instantiates subscription mechanisms to relay generated data to 
	/// the particular clients.
	/// </summary>
	public class SensorCaptureRequestHandler : StandardRequestHandler {
		LiveWireServer server;
		int clientOrigin;

		public SensorCaptureRequestHandler(LiveWireServer server, int clientOrigin) : base() {
			this.server = server;
			this.clientOrigin = clientOrigin;
		}
		public override HttpStatusCode Respond(JsonString subject, JsonString response) {
			if (subject == null) throw new ArgumentNullException("subject");
			if (response == null) throw new ArgumentNullException("response");
			// read properties from the request
			string topic = JsonString.ParseString(subject.GetTerminal("topic", string.Empty), string.Empty);
			if (!topic.Equals("sensorCapture")) {
				return HttpStatusCode.BadRequest;
			}
			int positionStart, positionEnd;
			if (!ReadPosition(subject, out positionStart, out positionEnd)) {
				return HttpStatusCode.BadRequest;
			}
			if (positionStart != positionEnd) {
				// not supported
				return HttpStatusCode.BadRequest;
			}
			var template = new JsonString(response);
			template.RemoveTerminal("requestId");
			var responseSubject = JsonString.CreateDict();
			{
				responseSubject.AddTerminal("type", "\"post\"");
				responseSubject.AddTerminal("topic", "\"sensorCapture\"");
				responseSubject.AddTerminal("position", JsonString.Stringify(positionStart));
				responseSubject.AddJsonString("data", JsonString.CreateArray());
			}

			response.AddJsonString("subject", responseSubject);

			// the client can request optional information 
			// captured images can take a lot of bandwidth and are there off by default
			int timeout = 10000;
			bool includeImage = false;
			var dataArray = subject.GetJsonString("data");
			if (dataArray != null) {
				var data = dataArray.GetJsonString(0);
				if (data != null) {
					timeout = JsonString.ParseInt(data.GetTerminal("timeout", ""), (int)timeout);
					includeImage = JsonString.ParseBool(data.GetTerminal("includeImage", "false"), false);
				}
			}

			// try to get sensor data
			// here we create an observer-relay pair that relays captured data to explicitly requesting clients
			// a request will be active for a couple of seconds, then the client has to resend it
			// the sensor observer generates data when the webcam is active 
			// which is send to the client through LiveWire subscription (relay)

			// TODO advance this functionality
			// TODO distinguish between different types of requests (start/stop/only for live stats e.g. fps/+image data)

			// TODO need a lock here too?
			int iSensor = positionStart;
			var readySignal = new AutoResetEvent(false);
			object sync = new object();
			ulong relayID = LiveCaptureSubscription.GenerateSubscriptionID(clientOrigin, SensorType.Webcam, iSensor);
			LiveCaptureSubscription relay = new LiveCaptureSubscription(relayID);

			HttpStatusCode status = HttpStatusCode.InternalServerError;


			CaptureSystem.Instance.Invoke((object sender, EventArgs args) => {
				var system = (CaptureSystem)sender;
				// make sure the sensor is started
				// and subscribe to it if successful
				ISensorBase sensor = system.GetSensor(iSensor);
				if (sensor == null || sensor.SensorType != SensorType.Webcam) {
					lock (sync) {
						status = HttpStatusCode.NotFound;
					}
					readySignal.Set();
					return;
				}
				lock (sync) {
					// when the observer times out its subscription to camera events should end
					// in order to save resources
					// the observer cancels the relay in the process
					var observer = new WebcamCaptureObserver(timeout, includeImage, clientOrigin, relay, iSensor, template);
					IDisposable subscription = system.ObserveSensor(positionStart).Subscribe(observer);
					observer.Cancelled += delegate {
						subscription.Dispose();
					};
					LogManager.Instance.LogMessage(this, string.Format("Subscribed client {0} to webcam {1} as {2}",
						clientOrigin, iSensor, relayID));
					status = HttpStatusCode.OK;
				}
				readySignal.Set();
			});
			// cancellation makes sure that a client is only fed by one observer-relay pair at a time
			// search for conflicting subscriptions and cancel them
			// conflicts have the same ID as generated above
			var activeConsumers = server.GetSubscriptions(clientOrigin);
			foreach (var c in activeConsumers) {
				if (c.SubscriptionID == relayID) {
					c.Cancel();
					LogManager.Instance.LogMessage(this, string.Format("Replaced webcam video subscription {0} for client {1}",
						relayID, clientOrigin));
				}
			}
			// wait for the external operation above to finish
			// starting a webcam can take a few seconds
			if (!readySignal.WaitOne(10000)) {
				relay.Dispose();
				return HttpStatusCode.InternalServerError;
			}
			// since we have no synchronization other than on readySignal we make use of an additional lock
			// otherwise the ready signal could be set, while we see an old status code value
			// TODO check validity of statement above
			lock (sync) {
				if (status != HttpStatusCode.OK) {
					relay.Dispose();
					return status;
				}
				if (!server.Subscribe(clientOrigin, relay)) {
					relay.Dispose();
					return HttpStatusCode.InternalServerError;
				}
				return HttpStatusCode.OK;
			}
			// TODO create an update in case the sensor was started
		}



	}

}

