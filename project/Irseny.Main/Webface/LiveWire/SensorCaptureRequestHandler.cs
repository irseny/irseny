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

namespace Irseny.Main.Webface {
	public class SensorCaptureRequestHandler : StandardRequestHandler {
		public SensorCaptureRequestHandler() : base() {
		}
		public override HttpStatusCode Respond(JsonString subject, JsonString response) {
			if (subject == null) throw new ArgumentNullException("subject");
			if (response == null) throw new ArgumentNullException("response");
			// read properties from the request
			string topic = TextParseTools.ParseString(response.GetTerminal("topic", string.Empty), string.Empty);
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
			// try to get sensor data
			var readySignal = new AutoResetEvent(false);
			SensorCaptureObserver observer = null;
			IDisposable subscription = null;
			CaptureSystem.Instance.Invoke((object sender, EventArgs args) => {
				var system = (CaptureSystem)sender;
				ISensorBase sensor = system.GetSensor(positionStart);
				if (sensor == null || sensor.SensorType != SensorType.Webcam) {
					readySignal.Set();
					return;
				}
				if (!sensor.Capturing) {
					if (!system.StartSensor(positionStart)) {
						readySignal.Set();
						return;
					}
				}
				observer = new SensorCaptureObserver(readySignal);
				subscription = system.ObserveSensor(positionStart).Subscribe(observer);
			});
			readySignal.WaitOne();
			if (subscription == null) {
				// failed
				return HttpStatusCode.NotFound;
			}
			subscription.Dispose();
			if (observer.Result == null) {
				// no data captured
				return HttpStatusCode.NotFound;
			}
			VideoFrame frame = observer.Result;
			// generate a base64 string from the video frame
			string imageString = string.Empty;
			IntPtr imageData = Marshal.UnsafeAddrOfPinnedArrayElement(frame.Data, 0);
			PixelFormat bitmapFormat = VideoFrame.GetBitmapFormat(frame.Format);
			using (var bitmap = new Bitmap(frame.Width, frame.Height, frame.Width, bitmapFormat, imageData)) {
				using (var stream = new MemoryStream(bitmap.Width*bitmap.Height*sizeof(byte))) {
					bitmap.Save(stream, ImageFormat.Jpeg);
					imageString = Convert.ToBase64String(stream.ToArray());
				}
			}
			// respond with the image string
			var responseSubject = JsonString.CreateDict();
			{
				responseSubject.AddTerminal("type", StringifyTools.StringifyString("post"));
				responseSubject.AddTerminal("topic", StringifyTools.StringifyString("sensorCapture"));
				responseSubject.AddTerminal("position", StringifyTools.StringifyInt(positionStart));
				var data = JsonString.CreateArray();
				{
					data.AddTerminal(string.Empty, StringifyTools.StringifyString(imageString));
				}
				response.AddJsonString("data", data);
			}
			response.AddJsonString("response", responseSubject);
			return HttpStatusCode.OK;
		}

		/// <summary>
		/// Sensor observer that sets an <see cref="AutoResetEvent"/> as soon as 
		/// the first video frame is available or an error occured.
		/// </summary>
		private class SensorCaptureObserver : ISensorObserver {
			AutoResetEvent readySignal;
			public VideoFrame Result { get; private set; }


			public SensorCaptureObserver(AutoResetEvent readySignal) {
				this.readySignal = readySignal;
				Result = null;
			}
			
			public void OnConnected(ISensorBase sensor) {
				// nothing to do
			}
			public void OnDisconnected(ISensorBase sensor) {
				// something went wrong
				readySignal.Set();
			}
			public void OnStarted(ISensorBase sensor) {
				// ok
			}
			public void OnStopped(ISensorBase sensor) {
				// not exepcted
				readySignal.Set();
			}
			public void OnDataAvailable(SensorDataPacket packet) {
				if (packet.DataType != SensorDataType.Video) {
					// only video data supported
					readySignal.Set();
				}
				var data = packet.GenericData as VideoFrame;
				if (data == null) {
					// weird data format
					readySignal.Set();
				}
				Result = data;
				readySignal.Set();
			}
		}
	}

}

