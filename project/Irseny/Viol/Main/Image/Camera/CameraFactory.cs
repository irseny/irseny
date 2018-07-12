﻿using System;
using System.IO;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace Irseny.Viol.Main.Image.Camera {
	public class CameraFactory : InterfaceFactory {
		byte[] pixelBuffer = new byte[0];
		Gdk.Pixbuf imgShow;
		string videoOutStock = "gtk-missing-image";
		Gtk.IconSize videoOutSize = Gtk.IconSize.Button;
		private readonly int index;
		public CameraFactory(int index) : base() {
			this.index = index;
		}


		protected override bool CreateInternal() {
			var factory = Mycena.InterfaceFactory.CreateFromFile(Content.ContentMaster.Instance.Resources.InterfaceDefinitions.GetEntry("CameraImage"));
			Container = factory.CreateWidget("box_Root");
			imgShow = null; // target size unknown
			return true;
		}
		protected override bool ConnectInternal() {
			Listing.EquipmentMaster.Instance.VideoCaptureStream.Updated += StreamStateChanged;
			var videoOut = Container.GetWidget<Gtk.Image>("img_VideoOut");
			videoOut.GetStock(out videoOutStock, out videoOutSize);
			return true;
		}
		protected override bool DisconnectInternal() {
			StopCapture();
			Listing.EquipmentMaster.Instance.VideoCaptureStream.Updated -= StreamStateChanged;
			return true;
		}
		protected override bool DestroyInternal() {
			if (imgShow != null) {
				imgShow.Dispose();
				imgShow = null;
			}
			Container.Dispose();
			return true;
		}
		private void StreamStateChanged(object sender, Listing.EquipmentUpdateArgs<int> args) {
			if (args.Index == index) {
				bool start = args.Active;
				Invoke(delegate {
					if (start) {
						StartCapture();
					} else {
						StopCapture();
					}
				});
			}
		}
		private void RetrieveImage(object sender, Capture.Video.CaptureImageEventArgs args) {
			if (!Initialized) {
				return;
			}
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
			using (var imgRef = args.Image) {
				var imgSource = imgRef.Reference;
				if (imgSource.NumberOfChannels == 3 && imgSource.ElementSize == sizeof(byte) * 3 && imgSource.DataPointer != IntPtr.Zero) {
					width = imgSource.Width;
					height = imgSource.Height;
					totalBytes = width * height * imgSource.ElementSize * sizeof(byte);
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
					if (Initialized) { // can be called after the capture is stopped
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
						if (imgShow == null || imgShow.Width != width || imgShow.Height != height) {
							if (imgShow != null) {
								imgShow.Dispose();
							}
							imgShow = new Gdk.Pixbuf(Gdk.Colorspace.Rgb, false, 8, width, height);
							updatePixBuf = true;
						}
						Marshal.Copy(pixelBuffer, 0, imgShow.Pixels, totalBytes);
						if (!updatePixBuf) {
							videoOut.Pixbuf = imgShow;
						}
						videoOut.QueueDraw();
					}
				});
			}
		}

		public bool StartCapture() {
			if (!Initialized) {
				return false;
			}
			Capture.Video.CaptureSystem.Instance.Invoke(delegate {
				int streamId = Listing.EquipmentMaster.Instance.VideoCaptureStream.GetEquipment(index, -1);
				if (streamId > -1) { // listing will change again when the stream is available
					Capture.Video.CaptureStream stream = Capture.Video.CaptureSystem.Instance.GetStream(streamId);
					if (stream != null) {
						stream.ImageAvailable += RetrieveImage;
					}
				}
			});

			return true;
		}
		public bool StopCapture() {
			if (!Initialized) {
				return false; // occurs when the page is removed, although the listing update event should be unsubscribed
			}
			Capture.Video.CaptureSystem.Instance.Invoke(delegate {
				int streamId = Listing.EquipmentMaster.Instance.VideoCaptureStream.GetEquipment(index, -1);
				if (streamId > -1) { // already removed if the stream is no longer available
					Capture.Video.CaptureStream stream = Capture.Video.CaptureSystem.Instance.GetStream(streamId);
					if (stream != null) {
						stream.ImageAvailable -= RetrieveImage;
					}
				}
			});
			// reset default image
			if (imgShow != null) {
				imgShow.Dispose();
				imgShow = null;
			}
			Gtk.Image videoOut = Container.GetWidget<Gtk.Image>("img_VideoOut");
			videoOut.SetFromStock(videoOutStock, videoOutSize);
			return true;
		}
	}
}

