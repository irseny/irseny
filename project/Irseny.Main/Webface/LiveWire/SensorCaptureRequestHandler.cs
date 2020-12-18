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

namespace Irseny.Main.Webface {
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
			string topic = TextParseTools.ParseString(subject.GetTerminal("topic", string.Empty), string.Empty);
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
			template.RemoveTerminal("requestId", false);
			var responseSubject = JsonString.CreateDict();
			{
				responseSubject.AddTerminal("type", "\"post\"");
				responseSubject.AddTerminal("topic", "\"sensorCapture\"");
				responseSubject.AddTerminal("position", StringifyTools.StringifyInt(positionStart));
				responseSubject.AddJsonString("data", JsonString.CreateArray());
			}

			response.AddJsonString("subject", responseSubject);
			// try to get sensor data
			// here we create an observer-relay pair that relay captured data to explicitly requesting clients
			// a request will be active for 10 seconds, then the client has to resend it
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


			long timeout = 10000; // TODO read from subject
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
				if (!sensor.Capturing) {
					if (!system.StartSensor(iSensor)) {
						lock (sync) {
							status = HttpStatusCode.InternalServerError;
						}
						readySignal.Set();
						return;
					}
				}
				lock (sync) {
					// when the observer times out its subscription to camera events should end
					// in order to save resources
					// the observer cancels the relay in the process
					var observer = new WebcamCaptureObserver(timeout, clientOrigin, relay, iSensor, template);
					IDisposable subscription = system.ObserveSensor(positionStart).Subscribe(observer);
					observer.Cancelled += delegate {
						subscription.Dispose();
					};
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
			// wait for the operation above to finish
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
		}

		/// <summary>
		/// Sensor observer that sets an <see cref="AutoResetEvent"/> as soon as 
		/// the first video frame is available or an error occured.
		/// </summary>
		private class WebcamCaptureObserver : ISensorObserver {
			LiveCaptureSubscription consumer;
			int clientOrigin;
			long timeout;
			Stopwatch watch;
			JsonString messageTemplate;
			int sensorIndex;

			public WebcamCaptureObserver(long timeout, int clientOrigin, LiveCaptureSubscription consumer, 
				int sensorIndex, JsonString messageTemplate) {

				if (consumer == null) throw new ArgumentNullException("subscription");
				this.consumer = consumer;
				this.messageTemplate = messageTemplate;
				this.sensorIndex = sensorIndex;
				this.clientOrigin = clientOrigin;
				this.watch = new Stopwatch();
				this.watch.Start();
				this.timeout = watch.ElapsedMilliseconds + timeout;
			}
			public bool IsCancelled {
				get { return !watch.IsRunning; }
			}
			public event EventHandler Cancelled;

			private void HandleTimeout() {
				// cancel operation if the timer has ran out
				if (IsCancelled) {
					return; // already cancelled
				}
				if (consumer.IsCancelled) {
					OnCancel();
					return;
				} 
				long timestamp = watch.ElapsedMilliseconds;
				if (timestamp >= timeout) {
					OnCancel();
				}
			}
			private void OnCancel() {
				if (!consumer.IsCancelled) {
					consumer.Cancel();
				}
				if (watch.IsRunning) {
					long elapsed = watch.ElapsedMilliseconds;
					watch.Stop();
					Cancelled(this, new EventArgs());
					LogManager.Instance.LogMessage(this, string.Format("Webcam video subscription {0} for client {1} ended after {2} seconds",
						consumer.SubscriptionID, clientOrigin, elapsed/1000));
				}
			}
			public void OnConnected(ISensorBase sensor) {
				HandleTimeout();
			}
			public void OnDisconnected(ISensorBase sensor) {
				// something went wrong
				OnCancel();
			}
			public void OnStarted(ISensorBase sensor) {
				HandleTimeout();
			}
			public void OnStopped(ISensorBase sensor) {
				// not exepcted
				OnCancel();
			}
			public void OnDataAvailable(SensorDataPacket packet) {
				HandleTimeout();
				if (IsCancelled) {
					return;
				}
				if (packet.DataType != SensorDataType.Video) {
					// only video data supported
					return;
				}
				var frame = packet.GenericData as VideoFrame;
				if (frame == null) {
					// unsupported content
					return;
				}
				if (frame.Data == null || frame.Width <= 0 || frame.Height <= 0) {
					// missing image data
					return;
				}
				// generate a base64 string from the video frame
				string imageString = string.Empty;
				PixelFormat bitmapFormat = VideoFrame.GetBitmapFormat(frame.Format);
				int pixelSize = VideoFrame.GetPixelSize(frame.Format);
				int imageSize = frame.Width*frame.Height*pixelSize;
				GCHandle pinnedImage = GCHandle.Alloc(frame.Data, GCHandleType.Pinned);
				IntPtr imagePointer = pinnedImage.AddrOfPinnedObject();
				// create a bitmap, copy the frame over, convert to jpeg
				// and generate a base64 string from that
				using (var bitmap = new Bitmap(frame.Width, frame.Height, bitmapFormat)) {
					BitmapData bitmapData = bitmap.LockBits(new Rectangle(0, 0, frame.Width, frame.Height), ImageLockMode.WriteOnly, bitmapFormat);
					Marshal.Copy(frame.Data, 0, bitmapData.Scan0, imageSize);
					bitmap.UnlockBits(bitmapData);

					using (var stream = new MemoryStream(imageSize)) {
						bitmap.Save(stream, ImageFormat.Jpeg);

						string sData = Convert.ToBase64String(stream.ToArray());
						imageString = string.Format("\"data:image/jpeg;base64,{0}\"", sData);
					}
				}
				pinnedImage.Free();
				// craete a message that contains the compressed image
				// and send it through the consumer to the client
				var message = new JsonString(messageTemplate);

				var subject = JsonString.CreateDict();
				{
					subject.AddTerminal("type", StringifyTools.StringifyString("post"));
					subject.AddTerminal("topic", StringifyTools.StringifyString("sensorCapture"));
					subject.AddTerminal("position", StringifyTools.StringifyInt(sensorIndex));
					var data = JsonString.CreateArray();
					{
						var entry = JsonString.CreateDict();
						{
							entry.AddTerminal("image", imageString);
							entry.AddTerminal("width", StringifyTools.StringifyInt(frame.Width));
							entry.AddTerminal("height", StringifyTools.StringifyInt(frame.Height));
						}
						data.AddJsonString(string.Empty, entry);
					}
					subject.AddJsonString("data", data);
				}
				message.AddJsonString("subject", subject, true);
				string str = message.ToJsonString();
				consumer.EnqueueMessage(message);
			}
		}
	}

}

