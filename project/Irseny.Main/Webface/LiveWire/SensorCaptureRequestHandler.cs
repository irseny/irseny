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
			// TODO set a timeout
			readySignal.WaitOne();
			if (subscription == null) {
				// failed
				return HttpStatusCode.NotFound;
			}
			subscription.Dispose();
			if (observer.Image == null || observer.Settings == null) {
				// no data captured
				return HttpStatusCode.InternalServerError;
			}
			VideoFrame frame = observer.Image;
			SensorSettings settings = observer.Settings;
			if (frame.Data == null) {
				return HttpStatusCode.InternalServerError;
			}
			// generate a base64 string from the video frame
			string imageString = string.Empty;

			PixelFormat bitmapFormat = VideoFrame.GetBitmapFormat(frame.Format);
			int pixelSize = VideoFrame.GetPixelSize(frame.Format);
			int imageSize = frame.Width*frame.Height*pixelSize;
			GCHandle pinnedImage = GCHandle.Alloc(frame.Data, GCHandleType.Pinned);
			IntPtr imagePointer = pinnedImage.AddrOfPinnedObject();

			// instead load from disk




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
			// respond with the image string
			var responseSubject = JsonString.CreateDict();
			{
				responseSubject.AddTerminal("type", StringifyTools.StringifyString("post"));
				responseSubject.AddTerminal("topic", StringifyTools.StringifyString("sensorCapture"));
				responseSubject.AddTerminal("position", StringifyTools.StringifyInt(positionStart));
				var data = JsonString.CreateArray();
				{
					var entry = JsonString.CreateDict();
					{
						entry.AddJsonString("settings", SensorSettings.ToJson(settings));
						entry.AddTerminal("image", imageString);
						
					}
					data.AddJsonString(string.Empty, entry);
				}
				responseSubject.AddJsonString("data", data);
			}
			response.AddJsonString("subject", responseSubject);
			return HttpStatusCode.OK;
		}

		/// <summary>
		/// Sensor observer that sets an <see cref="AutoResetEvent"/> as soon as 
		/// the first video frame is available or an error occured.
		/// </summary>
		private class SensorCaptureObserver : ISensorObserver {
			AutoResetEvent readySignal;
			public SensorSettings Settings { get; private set; }
			public VideoFrame Image { get; private set; }


			public SensorCaptureObserver(AutoResetEvent readySignal) {
				this.readySignal = readySignal;
				Image = null;
				Settings = null;
			}
			
			public void OnConnected(ISensorBase sensor) {
				// nothing to do
			}
			public void OnDisconnected(ISensorBase sensor) {
				// something went wrong
				readySignal.Set();
			}
			public void OnStarted(ISensorBase sensor) {
				if (sensor.SensorType == SensorType.Webcam) {
					Settings = ((WebcamCapture)sensor).GetSettings();
				}
			}
			public void OnStopped(ISensorBase sensor) {
				// not exepcted
				readySignal.Set();
			}
			public void OnDataAvailable(SensorDataPacket packet) {
				if (Image != null) {
					// only work with the first data packet coming in
					return;
				}
				if (packet.DataType != SensorDataType.Video) {
					// only video data supported
					readySignal.Set();
				}
				var data = packet.GenericData as VideoFrame;
				if (data == null) {
					// weird data format
					readySignal.Set();
				}
				Image = data;
				if (Settings == null) {
					Settings = ((WebcamCapture)packet.Sensor).GetSettings();
				}
				readySignal.Set();
			}
		}
	}

}

