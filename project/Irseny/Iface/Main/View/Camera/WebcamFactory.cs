using System;
using System.IO;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Collections.Generic;
using Irseny.Log;
using Irseny.Util;
using Irseny.Content;
using Irseny.Listing;
using Irseny.Capture.Video;

namespace Irseny.Iface.Main.View.Camera {
	public class WebcamFactory : InterfaceFactory {
		byte[] pixelBuffer = new byte[0];
		//Gdk.Pixbuf activeImage = null;
		int imageWidth = 0;
		int imageHeight = 0;

		Queue<Emgu.CV.Mat> captureBuffer = new Queue<Emgu.CV.Mat>();
		//string videoOutStock = "gtk-missing-image";
		//Gtk.IconSize videoOutSize = Gtk.IconSize.Button;
		private readonly int streamIndex;
		public WebcamFactory(int index) : base() {
			this.streamIndex = index;
		}


		protected override bool CreateInternal() {
			var factory = ContentMaster.Instance.Resources.InterfaceFactory.GetEntry("WebcamView");
			Container = factory.CreateWidget("box_Root");
			return true;
		}
		protected override bool ConnectInternal() {
			/*var videoOut = Container.GetWidget<Gtk.Image>("img_VideoOut");
			videoOut.GetStock(out videoOutStock, out videoOutSize);*/
			var drawVideoOut = Container.GetWidget<Gtk.DrawingArea>("draw_VideoOut");
			drawVideoOut.Drawn += DrawImage;

			CaptureSystem.Instance.Invoke(delegate {
				int streamId = EquipmentMaster.Instance.VideoCaptureStream.GetEquipment(streamIndex, -1);
				if (streamId < 0) {
					LogManager.Instance.Log(LogMessage.CreateError(this, "Cannot find capture " + streamIndex));
					return;
				}
				CaptureStream stream = CaptureSystem.Instance.GetStream(streamId);
				if (stream == null) {
					LogManager.Instance.Log(LogMessage.CreateError(this, "Cannot find capture " + streamIndex));
					return;
				}
				stream.ImageAvailable += RetrieveImage;
			});
			return true;
		}
		protected override bool DisconnectInternal() {
			// nothing to do, the capture stream does not exist any more
			return true;
		}
		protected override bool DestroyInternal() {
			Container.Dispose();
			return true;
		}


		private void RetrieveImage(object sender, Capture.Video.ImageCapturedEventArgs args) {
			/*int width = 0;
			int height = 0;
			MemoryStream imgStream;
			using (var imgRef = args.Image) {
				var imgSource = imgRef.Reference;
				width = imgSource.Width;
				height = imgSource.Height;
				imgStream = new MemoryStream(imgSource.Width*imgSource.Height*imgSource.ElementSize);
				using (var bmp = imgSource.Bitmap) {
					bmp.Save(imgStream, System.Drawing.Imaging.ImageFormat.MemoryBmp);
				}
				imgStream.Position = 0;
			}
			Invoke(delegate {
				Gtk.Image videoOut = Container.GetWidget<Gtk.Image>("img_VideoOut");
				videoOut.Pixbuf = new Gdk.Pixbuf(imgStream, width, height);
				videoOut.QueueDraw();
				imgStream.Dispose();
			});*/
			//byte[] buffer = null;
			imageWidth = 0;
			imageHeight = 0;
			int totalBytes = 0;
			bool pixelsAvailable = false;
			using (var imgRef = args.GrayImage) {
				var imgSource = imgRef.Reference;
				//if (imgSource.NumberOfChannels == 3 && imgSource.ElementSize == sizeof(byte)*3 && imgSource.DataPointer != IntPtr.Zero) {
				//	imageWidth = imgSource.Width;
				//	imageHeight = imgSource.Height;
				//	totalBytes = imageWidth*imageHeight*imgSource.ElementSize*sizeof(byte);
				//	if (pixelBuffer.Length < totalBytes) {
				//		pixelBuffer = new byte[totalBytes];
				//	}
				//	// we could write data that is currently read below, though this should only result in visual artifacts sometimes
				//	Marshal.Copy(imgSource.DataPointer, pixelBuffer, 0, totalBytes);
				//	pixelsAvailable = true;
				//} else {
				//	Debug.WriteLine(this.GetType().Name + ": Retrieved image has wrong format.");
				//}
				if (imgSource.NumberOfChannels == 1 && imgSource.ElementSize == sizeof(byte) && imgSource.DataPointer != IntPtr.Zero) {
					imageWidth = imgSource.Width;
					imageHeight = imgSource.Height;
					totalBytes = imageWidth*imageHeight*sizeof(byte);
					if (pixelBuffer.Length < totalBytes) {
						pixelBuffer = new byte[totalBytes];
					}
					// we could write data that is currently read below, though this should only result in visual artifacts sometimes
					Marshal.Copy(imgSource.DataPointer, pixelBuffer, 0, totalBytes);
					pixelsAvailable = true;
				} else {
					Debug.WriteLine(this.GetType().Name + ": Retrieved image has wrong format.");
				}
			}


			if (!pixelsAvailable) {
				return;
			}
			Invoke(delegate {
				if (!Initialized) {
					return;
				}
				Gtk.Widget drawImageOut = Container.GetWidget("draw_VideoOut");
				drawImageOut.QueueDraw();
			});

			Invoke(delegate {
				if (!Initialized) { // can be called after the capture is stopped
					return;
				}
				//Gtk.Image videoOut = Container.GetWidget<Gtk.Image>("img_VideoOut");
				//bool updatePixBuf = false;
				//if (activeImage == null || activeImage.Width != width || activeImage.Height != height) {
				//	activeImage = new Gdk.Pixbuf(Gdk.Colorspace.Rgb, false, 8, width, height);
				//	updatePixBuf = true;
				//}
				//Marshal.Copy(pixelBuffer, 0, activeImage.Pixels, totalBytes);

				//videoOut.Pixbuf = activeImage; // explicitly set to make updates visible

				//videoOut.QueueDraw();

				// other attempts
				//var pixels = new Gdk.Pixbuf(buffer, Gdk.Colorspace.Rgb, false, 8, width, height, stride, (byte[] buf) => {
				//	Console.WriteLine("pixbuf destroy"); // not called
				//});
				//var pixels = new Gdk.Pixbuf(buffer); // unable to determine format
				//var pixels = new Gdk.Pixbuf(buffer, false); // header corrupt
				//
				//var data = new Gdk.Pixdata();
				//data.Deserialize((uint)buffer.Length, buffer); // unable to determine format
				//var pixels = Gdk.Pixbuf.FromPixdata(data, true);
				//var pixels = new Gdk.Pixbuf(buffer, false, 8, width, height, stride); // out of memory

				//Gtk.Widget drawImageOut = Container.GetWidget("draw_VideoOut");
				//drawImageOut.QueueDraw();
				//});
				//Invoke(delegate {
				//	if (!Initialized) {
				//		return;
				//	}
				//	Gtk.Widget drawImageOut = Container.GetWidget("draw_VideoOut");
				//	drawImageOut.QueueDraw();
			});

		}
		private void DrawImage(object sender, Gtk.DrawnArgs args) {
			if (!Initialized) {
				return;
			}
			// TODO: drawing operations increase memory usage, there must be a memory leak located in here
			Gtk.DrawingArea target = (Gtk.DrawingArea)sender;
			Gdk.Window window = target.Window;
			int targetWidth = window.Width;
			int targetHeight = window.Height;

			// memory leaks seem to originate from the context creation call
			using (Cairo.Context context = Gdk.CairoHelper.Create(window)) {
				if (captureBuffer.Count > 0) {
					// display capture
					for (int i = 0; i < imageWidth*imageHeight; i++) {
						pixelBuffer[i] = (byte)(pixelBuffer[i]*4);
					}
					using (var surface = new Cairo.ImageSurface(pixelBuffer, Cairo.Format.A8, imageWidth, imageHeight, imageWidth)) {
						context.Rectangle(0, 0, window.Width, window.Height);
						context.SetSource(surface);

						context.Fill();
					}
					context.Translate(window.Width/2, window.Height/2);
					context.Arc(0, 0, (window.Width < window.Height ? window.Width : window.Height) / 2 - 10, 0, 2*Math.PI);
					context.StrokePreserve();

					context.SetSourceRGB(0.3, 0.4, 0.6);
					context.Fill();
				} else {
					// display fallback
					var imgFallback = Container.GetGadget<Gtk.Image>("img_MissingVideo");
					int imgWidth = imgFallback.Pixbuf.Width;
					int imgHeight = imgFallback.Pixbuf.Height;
					int startX = (targetWidth - imgWidth)/2;
					int startY = (targetHeight - imgHeight)/2;

					Gdk.CairoHelper.SetSourcePixbuf(context, imgFallback.Pixbuf, startX, startY);
					context.Rectangle(startX, startY, imgWidth, imgHeight);
					context.Fill();
				}
			}
			//Console.WriteLine("displaying image " + imageWidth + " " + imageHeight);
		}
	}
}

