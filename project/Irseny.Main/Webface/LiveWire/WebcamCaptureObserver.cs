﻿using System;
using System.Diagnostics;
using Irseny.Core.Util;
using Irseny.Core.Log;
using Irseny.Core.Sensors;
using Irseny.Core.Sensors.VideoCapture;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using System.Drawing;
using System.IO;

namespace Irseny.Main.Webface.LiveWire {
	/// <summary>
	/// Sensor observer that sets an <see cref="AutoResetEvent"/> as soon as 
	/// the first video frame is available or an error occured.
	/// </summary>
	public class WebcamCaptureObserver : ISensorObserver {
		LiveCaptureSubscription consumer;
		int clientOrigin;
		long timeout;
		bool includeImage;
		Stopwatch watch;
		JsonString messageTemplate;
		int sensorIndex;

		public WebcamCaptureObserver(long timeout, bool includeImage, int clientOrigin, LiveCaptureSubscription consumer, 
			int sensorIndex, JsonString messageTemplate) {

			if (consumer == null) throw new ArgumentNullException("subscription");
			this.consumer = consumer;
			this.messageTemplate = messageTemplate;
			this.sensorIndex = sensorIndex;
			this.clientOrigin = clientOrigin;
			this.watch = new Stopwatch();
			this.watch.Start();
			this.timeout = watch.ElapsedMilliseconds + timeout;
			this.includeImage = includeImage;
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
		public void OnSettingsChanged(ISensorBase sensor) {
			// no reaction required
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
			// generate a base64 string from the video frame
			string imageString = string.Empty;
			if (includeImage) {
				imageString = CompressImage(frame);
			}
			// craete a message that contains the compressed image
			// and send it through the consumer to the client
			var message = new JsonString(messageTemplate);

			var subject = JsonString.CreateDict();
			{
				subject.AddTerminal("type", StringifyTools.StringifyString("post"));
				subject.AddTerminal("topic", StringifyTools.StringifyString("sensorCapture"));
				subject.AddTerminal("position", StringifyTools.StringifyInt(sensorIndex));
				var dataArray = JsonString.CreateArray();
				{
					var dataEntry = JsonString.CreateDict();
					{
						if (includeImage) {
							dataEntry.AddTerminal("image", imageString);
						}
						dataEntry.AddTerminal("frameWidth", StringifyTools.StringifyInt(frame.Width));
						dataEntry.AddTerminal("frameHeight", StringifyTools.StringifyInt(frame.Height));
						var metadata = frame.Metadata;
						dataEntry.AddTerminal("frameRate", StringifyTools.StringifyInt(metadata.FrameRate));
						dataEntry.AddTerminal("frameTime", StringifyTools.StringifyInt(metadata.FrameTime));
						dataEntry.AddTerminal("frameTimeDeviation", StringifyTools.StringifyInt(metadata.FrameTimeDeviation));
					}
					dataArray.AddJsonString(string.Empty, dataEntry);
				}
				subject.AddJsonString("data", dataArray);
			}
			message.AddJsonString("subject", subject, true);
			string str = message.ToJsonString();
			consumer.EnqueueMessage(message);
		}
		/// <summary>
		/// Compresses the given video frame to JPEG format and encodes it as base64.
		/// </summary>
		/// <returns>The compressed and encoded image.</returns>
		/// <param name="frame">Video frame.</param>
		private string CompressImage(VideoFrame frame) {
			if (frame.Data == null || frame.Width <= 0 || frame.Height <= 0) {
				// missing image data
				return string.Empty;
			}
			string result = string.Empty;
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
					result = string.Format("\"data:image/jpeg;base64,{0}\"", sData);
				}
			}
			pinnedImage.Free();
			return result;
		}
	}
}
