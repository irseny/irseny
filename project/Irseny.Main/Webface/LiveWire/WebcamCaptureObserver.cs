// This file is part of Irseny.
//
// Copyright (C) 2021  Thilo Gabel
//
// Irseny is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
//
// Irseny is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
//
// You should have received a copy of the GNU General Public License
// along with this program.  If not, see <https://www.gnu.org/licenses/>.

using System;
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
	/// Sensor observer that relays captured data to a <see cref="LiveCaptureSubscription"/> instance.
	/// The Observer is only active for a limited time period after which it cancels itself.
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
				subject.AddTerminal("type", JsonString.StringifyString("post"));
				subject.AddTerminal("topic", JsonString.StringifyString("sensorCapture"));
				subject.AddTerminal("position", JsonString.Stringify(sensorIndex));
				var dataArray = JsonString.CreateArray();
				{
					var dataEntry = JsonString.CreateDict();
					{
						if (includeImage && imageString.Length > 2) {
							dataEntry.AddTerminal("image", imageString);
						}
						dataEntry.AddTerminal("frameWidth", JsonString.Stringify(frame.Width));
						dataEntry.AddTerminal("frameHeight", JsonString.Stringify(frame.Height));
						var metadata = frame.Metadata;
						dataEntry.AddTerminal("frameRate", JsonString.Stringify(metadata.FrameRate));
						dataEntry.AddTerminal("frameTime", JsonString.Stringify(metadata.FrameTime));
						dataEntry.AddTerminal("frameTimeDeviation", JsonString.Stringify(metadata.FrameTimeDeviation));
					}
					dataArray.AddJsonString(string.Empty, dataEntry);
				}
				subject.AddJsonString("data", dataArray);
			}
			message.AddJsonString("subject", subject);
			string str = message.ToJsonString();
			consumer.EnqueueMessage(message);
		}
		/// <summary>
		/// Compresses the given video frame to JPEG format and encodes it as base64.
		/// </summary>
		/// <returns>The compressed and encoded image.</returns>
		/// <param name="frame">Video frame.</param>
		private string CompressImage(VideoFrame frame) {
			if (frame.PixelData == null || frame.Width <= 0 || frame.Height <= 0) {
				// missing image data
				return null;
			}
			string result = null;
			PixelFormat bitmapFormat = VideoFrame.GetBitmapFormat(frame.PixelFormat);
			int pixelSize = VideoFrame.GetPixelSize(frame.PixelFormat);
			int imageSize = frame.Width*frame.Height*pixelSize;
			GCHandle pinnedImage = GCHandle.Alloc(frame.PixelData, GCHandleType.Pinned);
			IntPtr imagePointer = pinnedImage.AddrOfPinnedObject();
			// create a bitmap, copy the frame over, convert to jpeg
			// and generate a base64 string from that
			using (var bitmap = new Bitmap(frame.Width, frame.Height, bitmapFormat)) {
				BitmapData bitmapData = bitmap.LockBits(new Rectangle(0, 0, frame.Width, frame.Height), ImageLockMode.WriteOnly, bitmapFormat);
				Marshal.Copy(frame.PixelData, 0, bitmapData.Scan0, imageSize);
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

