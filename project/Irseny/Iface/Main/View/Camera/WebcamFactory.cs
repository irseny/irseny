using System;
using System.IO;
using System.Diagnostics;
using System.Runtime.InteropServices;
using Irseny.Log;
using Irseny.Util;
using Irseny.Content;
using Irseny.Listing;
using Irseny.Capture.Video;

namespace Irseny.Iface.Main.View.Camera {
	public class WebcamFactory : InterfaceFactory {
		byte[] pixelBuffer = new byte[0];
		Gdk.Pixbuf activeImage = null;
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
			int width = 0;
			int height = 0;
			int totalBytes = 0;
			bool pixelsAvailable = false;
			using (var imgRef = args.ColorImage) {
				var imgSource = imgRef.Reference;
				if (imgSource.NumberOfChannels == 3 && imgSource.ElementSize == sizeof(byte)*3 && imgSource.DataPointer != IntPtr.Zero) {
					width = imgSource.Width;
					height = imgSource.Height;
					totalBytes = width*height*imgSource.ElementSize*sizeof(byte);
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
			if (pixelsAvailable) {
				Invoke(delegate {
					if (!Initialized) { // can be called after the capture is stopped
						return;
					}
					Gtk.Image videoOut = Container.GetWidget<Gtk.Image>("img_VideoOut");
					/*var pixels = new Gdk.Pixbuf(buffer, Gdk.Colorspace.Rgb, false, 8, width, height, stride, (byte[] buf) => {
						Console.WriteLine("pixbuf destroy"); // not called
					});*/
					//var pixels = new Gdk.Pixbuf(buffer); // unable to determine format
					//var pixels = new Gdk.Pixbuf(buffer, false); // header corrupt
					//
					//var data = new Gdk.Pixdata();
					//data.Deserialize((uint)buffer.Length, buffer); // unable to determine format
					//var pixels = Gdk.Pixbuf.FromPixdata(data, true);
					//var pixels = new Gdk.Pixbuf(buffer, false, 8, width, height, stride); // out of memory
					bool updatePixBuf = false;
					if (activeImage == null || activeImage.Width != width || activeImage.Height != height) {
						activeImage = new Gdk.Pixbuf(Gdk.Colorspace.Rgb, false, 8, width, height);
						updatePixBuf = true;
					}
					Marshal.Copy(pixelBuffer, 0, activeImage.Pixels, totalBytes);
					//if (updatePixBuf) {
					videoOut.Pixbuf = activeImage; // explicitly set to make updates visible
												   //}
					videoOut.QueueDraw();
				});
			}
		}
	}
}

