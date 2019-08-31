using System;
using System.IO;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Collections.Generic;
using Size2i = System.Drawing.Size;
using Irseny.Log;
using Irseny.Util;
using Irseny.Content;
using Irseny.Listing;
using Irseny.Capture.Video;

namespace Irseny.Iface.Main.View.Camera {
	public class WebcamFactory : InterfaceFactory {
		Queue<Emgu.CV.Mat> captureBuffer = new Queue<Emgu.CV.Mat>();
		readonly object captureSync = new object();
		Size2i captureSize = new Size2i(0, 0);
		bool captureRunning = false;

		readonly int streamIndex;
		public WebcamFactory(int index) : base() {
			this.streamIndex = index;
		}


		protected override bool CreateInternal() {
			var factory = ContentMaster.Instance.Resources.InterfaceFactory.GetEntry("WebcamView");
			Container = factory.CreateWidget("box_Root");
			return true;
		}
		protected override bool ConnectInternal() {
			var drawVideoOut = Container.GetWidget<Gtk.DrawingArea>("draw_VideoOut");
			drawVideoOut.Drawn += DrawImage;

			CaptureSystem.Instance.Invoke(delegate {
				int streamId = EquipmentMaster.Instance.VideoCaptureStream.GetEquipment(streamIndex, -1);
				if (streamId < 0) {
					LogManager.Instance.Log(LogEntry.CreateError(this, "Cannot find capture " + streamIndex));
					return;
				}
				CaptureStream stream = CaptureSystem.Instance.GetStream(streamId);
				if (stream == null) {
					LogManager.Instance.Log(LogEntry.CreateError(this, "Cannot find capture " + streamIndex));
					return;
				}
				stream.ImageAvailable += RetrieveImage;
				stream.CaptureStarted += CaptureStarted;
				if (stream.Capturing) {
					CaptureStarted(stream, new StreamEventArgs(stream, -1));
				}
				stream.CaptureStopped += CaptureStopped;
			});
			return true;
		}
		protected override bool DisconnectInternal() {
			// nothing to do, the capture stream does not exist any more
			return true;
		}
		protected override bool DestroyInternal() {
			Container.Dispose();
			lock (captureSync) {
				while (captureBuffer.Count > 0) {
					captureBuffer.Dequeue().Dispose();
				}
			}
			captureRunning = false;
			return true;
		}
		private void CaptureStarted(object sender, StreamEventArgs args) {
			lock (captureSync) {
				captureRunning = true;
			}
		}
		private void CaptureStopped(object sender, StreamEventArgs args) {
			lock (captureSync) {
				captureRunning = false;
			}
		}
		private void RetrieveImage(object sender, Capture.Video.ImageCapturedEventArgs args) {
			Size2i windowSize;
			lock (captureSync) {
				windowSize = captureSize;
			}
			// discard if the target size is unknown
			if (windowSize.Width < 1 || windowSize.Height < 1) {
				return;
			}
			// discard if many captures are already pending for display
			int captureNo;
			lock (captureSync) {
				captureNo = captureBuffer.Count;
			}
			if (captureNo >= 4) {
				return;
			}

			using (var imgRef = args.ColorImage) {
				var imgSource = imgRef.Reference;
				if (imgSource.NumberOfChannels != 3 || imgSource.ElementSize != sizeof(byte)*3) {
					LogManager.Instance.LogError(this, "Captured image has wrong format");
					return;
				}
				// choose target size and preserve aspect ratio
				Size2i targetSize = new Size2i(0, 0);
				float aspectRatio = (float)imgSource.Width/(float)imgSource.Height;
				{
					int width = Math.Min(windowSize.Width, imgSource.Width);
					int height = (int)Math.Round(width/aspectRatio);
					if (height <= windowSize.Height && height > targetSize.Height) {
						targetSize.Width = width;
						targetSize.Height = height;
					}
				}
				{
					int height = Math.Min(windowSize.Height, imgSource.Height);
					int width = (int)Math.Round(height*aspectRatio);
					if (width <= windowSize.Width && width > targetSize.Width) {
						targetSize.Width = width;
						targetSize.Height = height;
					}
				}
				if (targetSize.Width < 1 || targetSize.Height < 1) {
					return;
				}
				// resize and buffer capture
				Emgu.CV.Mat capture = new Emgu.CV.Mat(targetSize, imgSource.Depth, imgSource.NumberOfChannels);
				Emgu.CV.CvInvoke.Resize(imgSource, capture, targetSize);
				lock (captureSync) {
					captureBuffer.Enqueue(capture);
				}
			}
			Invoke(delegate {
				if (!Initialized) {
					return;
				}
				Gtk.Widget drawImageOut = Container.GetWidget("draw_VideoOut");
				drawImageOut.QueueDraw();
			});
		}
		private void DrawImage(object sender, Gtk.DrawnArgs args) {
			if (!Initialized) {
				return;
			}
			// TODO: drawing operations increase memory usage, there must be a memory leak located in here
			Gtk.DrawingArea target = (Gtk.DrawingArea)sender;
			Gdk.Window window = target.Window;
			// get the latest capture
			// the capture will remain in the queue
			// so that it can be displayed again
			Emgu.CV.Mat capture = null;
			if (captureRunning) {
				lock (captureSync) {
					captureSize = new Size2i(window.Width, window.Height);
					int captureNo = captureBuffer.Count;
					if (captureNo > 0) {
						// delete old captures
						while (captureBuffer.Count > 1) {
							captureBuffer.Dequeue().Dispose();
						}
						// keep the capture in the queue
						capture = captureBuffer.Peek();
					}
				}
			}
			// test the latest capture for satisfactory size
			if (capture != null) {
				// skip drawing oversized captures
				if (capture.Width > window.Width || capture.Height > window.Height) {
					return;
				}

			}
			// continue displaying the same image if no new image is available
			if (captureRunning && capture == null) {
				return;
			}
			// draw on widget
			// with gtk-sharp memory leaks seem to originate from this context creation call
			using (Cairo.Context context = Gdk.CairoHelper.Create(window)) {
				if (capture != null) {
					int totalBytes = capture.ElementSize*capture.Width*capture.Height;
					byte[] pixels = new byte[totalBytes];
					Marshal.Copy(capture.DataPointer, pixels, 0, totalBytes);
					int startX = (window.Width - capture.Width)/2;
					int startY = (window.Height - capture.Height)/2;
					/*using (var pixbuf = new Gdk.Pixbuf(Gdk.Colorspace.Rgb, false, 8, capture.Width, capture.Height)) {
						Marshal.Copy(pixels, 0, pixbuf.Pixels, totalBytes);
						Gdk.CairoHelper.SetSourcePixbuf(context, pixbuf, startX, startY);
						context.Rectangle(startX, startY, pixbuf.Width, pixbuf.Height);
						context.Fill();
					}*/
					using (var pixbuf = new Gdk.Pixbuf(pixels, Gdk.Colorspace.Rgb, false, 8, capture.Width, capture.Height, capture.Width*3)) {
						Gdk.CairoHelper.SetSourcePixbuf(context, pixbuf, startX, startY);
						context.Rectangle(startX, startY, pixbuf.Width, pixbuf.Height);
						context.Fill();
					}
					// this does not work as expected as the surface uses a custom stride value
					/*using (var surface = new Cairo.ImageSurface(Cairo.Format.RGB24, capture.Width, capture.Height)) {
						Marshal.Copy(capture.DataPointer, surface.Data, 0, totalBytes); // writing to data copy here
						context.SetSourceSurface(surface, startX, startY);
						context.Rectangle(startX, startY, capture.Width, capture.Height);
						context.Fill();
					}*/
				} else {
					// display fallback
					var imgFallback = Container.GetGadget<Gtk.Image>("img_MissingVideo");
					Gdk.Pixbuf pixbuf = imgFallback.Pixbuf;
					int startX = (window.Width - pixbuf.Width)/2;
					int startY = (window.Height - pixbuf.Height)/2;
					Gdk.CairoHelper.SetSourcePixbuf(context, imgFallback.Pixbuf, startX, startY);
					context.Rectangle(startX, startY, pixbuf.Width, pixbuf.Height);
					context.Fill();
				}
			}
			//Console.WriteLine("displaying image " + window.Width + " " + window.Height);
		}
	}
}

