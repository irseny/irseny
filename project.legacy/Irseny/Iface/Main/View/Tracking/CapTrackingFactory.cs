using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using Irseny.Content;
using Irseny.Listing;
using Irseny.Util;
using Irseny.Log;
using Irseny.Tracking;

namespace Irseny.Iface.Main.View.Tracking {
	public class CapTrackingFactory : InterfaceFactory {
		int trackerIndex;
		byte[] pixelBuffer = new byte[0];
		Gdk.Pixbuf activeImage = null;
		Gdk.Color backgroundColor = new Gdk.Color(0xFF, 0xFF, 0xFF) { Pixel = 0xFF };
		/*string videoOutStock = "gtk-missing-image";
		Gtk.IconSize videoOutSize = Gtk.IconSize.Button;*/

		public CapTrackingFactory(int trackerIndex) : base() {
			this.trackerIndex = trackerIndex;
		}
		protected override bool CreateInternal() {
			var factory = ContentMaster.Instance.Resources.InterfaceFactory.GetEntry("CapTrackingView");
			Container = factory.CreateWidget("box_Root");
			return true;
		}
		protected override bool ConnectInternal() {
			/*var videoOut = Container.GetWidget<Gtk.Image>("img_VideoOut");
			videoOut.GetStock(out videoOutStock, out videoOutSize);*/
			DetectionSystem.Instance.Invoke(delegate {
				int trackerId = EquipmentMaster.Instance.HeadTracker.GetEquipment(trackerIndex, -1);
				if (trackerId < 0) {
					LogManager.Instance.LogError(this, "Tracker " + trackerIndex + " not available");
					return;
				}

				var tracker = DetectionSystem.Instance.GetTracker<Irseny.Tracking.ISingleImageCapTracker>(trackerIndex, null);

				if (tracker == null) {
					LogManager.Instance.LogError(this, "Tracker " + trackerIndex + "not available");
					return;

				}
				tracker.InputProcessed += RetrieveImage;
				// TODO: connect start and stop signals
			});
			return true;
		}

		protected override bool DisconnectInternal() {
			DetectionSystem.Instance.Invoke(delegate {
				int trackerId = EquipmentMaster.Instance.HeadTracker.GetEquipment(trackerIndex, -1);
				if (trackerId < 0) {
					return;
				}
				var tracker = DetectionSystem.Instance.GetTracker<Irseny.Tracking.ISingleImageCapTracker>(trackerIndex, null);

				if (tracker == null) {
					return;

				}
				tracker.InputProcessed -= RetrieveImage;
			});
			return true;
		}

		protected override bool DestroyInternal() {
			// TODO: fix bug: removing a started tracker will make subsequently added trackers not receive images
			Container.Dispose();
			return true;
		}

		//private void StopCapture() {
		//	Tracap.DetectionSystem.Instance.Invoke(delegate {
		//		int trackerId = EquipmentMaster.Instance.HeadTracker.GetEquipment(trackerIndex, -1);
		//		if (trackerId > -1) {
		//			var tracker = Tracap.DetectionSystem.Instance.GetTracker<Tracap.ISingleImageCapTracker>(trackerIndex, null);
		//			if (tracker != null) {
		//				tracker.InputProcessed -= RetrieveImage;
		//			}
		//		}
		//	});
		//	if (!Initialized) {
		//		return;
		//	}
		//	var imgVideoOut = Container.GetWidget<Gtk.Image>("img_VideoOut");
		//	var imgMissingVideo = Container.GetGadget<Gtk.Image>("img_MissingVideo");
		//	imgVideoOut.Pixbuf = imgMissingVideo.Pixbuf;
		//	imgVideoOut.QueueDraw();
		//}
		private void RetrieveImage(object sender, Irseny.Tracking.ImageProcessedEventArgs args) {
			int width = 0;
			int height = 0;
			int totalBytes = 0;
			bool pixelsAvailable = false;
			using (var imgRef = args.Image) {
				var imgSource = imgRef.Reference;
				if (imgSource.NumberOfChannels == 1 && imgSource.ElementSize == sizeof(byte)) {
					width = imgSource.Width;
					height = imgSource.Height;
					totalBytes = width*height*imgSource.ElementSize;
					if (pixelBuffer.Length < totalBytes) {
						pixelBuffer = new byte[totalBytes];
					}

					Marshal.Copy(imgSource.DataPointer, pixelBuffer, 0, totalBytes);
					pixelsAvailable = true;
				} else {
					Debug.WriteLine(this.GetType().Name + ": Retrieved image has wrong format.");
				}
			}
			if (pixelsAvailable) {
				Invoke(delegate {
					if (!Initialized) {
						return;
					}
					Gtk.Image videoOut = Container.GetWidget<Gtk.Image>("img_VideoOut");
					bool updatePixBuf = false;
					if (activeImage == null || activeImage.Width != width || activeImage.Height != height) {
						activeImage = new Gdk.Pixbuf(Gdk.Colorspace.Rgb, false, 8, width, height);
						updatePixBuf = true;
					}
					IntPtr target = activeImage.Pixels;
					int bufferLength = pixelBuffer.Length - 1; // prevent writing outside buffer bounds
					for (int p = 0; p < bufferLength; p++) {
						int b = pixelBuffer[p];
						int pixel = b<<16 | b<<8 | b;
						Marshal.WriteInt32(target, p*3, pixel); // works as expected, but where is the forth byte located?
					}
					videoOut.Pixbuf = activeImage;
					videoOut.QueueDraw();
				});
				/*Invoke(delegate {
					if (!Initialized) {
						return;
					}
					Gtk.Image videoOut = Container.GetWidget<Gtk.Image>("img_VideoOut");
					bool updatePixBuf = false;
					if (activeImage == null || activeImage.Width != width || activeImage.Height != height) {
						activeImage = new Gdk.Pixbuf(Gdk.Colorspace.Rgb, false, 8, width, height);
						updatePixBuf = true;
					}
					for (int i = 0; i < 3; i++) {
						Marshal.Copy(pixelBuffer, 0, activeImage.Pixels + totalBytes * i, totalBytes);
					}
					if (updatePixBuf) {
						if (videoOut.Pixbuf != null) {
							videoOut.Pixbuf.Dispose();
						}
						videoOut.Pixbuf = activeImage;
					}
					videoOut.QueueDraw();
				});*/
			}
		}

	}
}
