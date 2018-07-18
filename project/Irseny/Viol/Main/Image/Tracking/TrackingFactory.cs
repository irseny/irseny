using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using Irseny.Content;
using Irseny.Listing;

namespace Irseny.Viol.Main.Image.Tracking {
	public class TrackingFactory : InterfaceFactory {
		int trackerIndex;
		byte[] pixelBuffer = new byte[0];
		Gdk.Pixbuf activeImage = null;
		/*string videoOutStock = "gtk-missing-image";
		Gtk.IconSize videoOutSize = Gtk.IconSize.Button;*/

		public TrackingFactory(int trackerIndex) : base() {
			this.trackerIndex = trackerIndex;
		}
		protected override bool CreateInternal() {
			var factory = Mycena.InterfaceFactory.CreateFromFile(ContentMaster.Instance.Resources.InterfaceDefinitions.GetEntry("TrackingImage"));
			Container = factory.CreateWidget("box_Root");
			return true;
		}
		protected override bool ConnectInternal() {
			EquipmentMaster.Instance.HeadTracker.Updated += TrackerStateChanged;
			/*var videoOut = Container.GetWidget<Gtk.Image>("img_VideoOut");
			videoOut.GetStock(out videoOutStock, out videoOutSize);*/
			return true;
		}

		protected override bool DisconnectInternal() {
			EquipmentMaster.Instance.HeadTracker.Updated -= TrackerStateChanged;
			StopCapture();
			return true;
		}

		protected override bool DestroyInternal() {
			// TODO: fix bug: removing a started tracker will make subsequently added trackers not receive images
			Container.Dispose();
			return true;
		}
		private void TrackerStateChanged(object sender, EquipmentUpdateArgs<int> args) {
			if (args.Index == trackerIndex) {
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
		private void StartCapture() {
			if (!Initialized) {
				return;
			}
			Tracap.DetectionSystem.Instance.Invoke(delegate {
				int trackerId = EquipmentMaster.Instance.HeadTracker.GetEquipment(trackerIndex, -1);
				if (trackerId > -1) {
					var tracker = Tracap.DetectionSystem.Instance.GetDetector<Tracap.ISingleImageCapTracker>(trackerIndex, null);
					if (tracker != null) {
						tracker.InputProcessed += RetrieveImage;
					}
				}
			});
		}
		private void StopCapture() {
			Tracap.DetectionSystem.Instance.Invoke(delegate {
				int trackerId = EquipmentMaster.Instance.HeadTracker.GetEquipment(trackerIndex, -1);
				if (trackerId > -1) {
					var tracker = Tracap.DetectionSystem.Instance.GetDetector<Tracap.ISingleImageCapTracker>(trackerIndex, null);
					if (tracker != null) {
						tracker.InputProcessed -= RetrieveImage;
					}
				}
			});
			if (!Initialized) {
				return;
			}
			/*if (activeImage != null) {
				activeImage.Dispose();
				activeImage = null;
			}
			var imgVideoOut = Container.GetWidget<Gtk.Image>("img_VideoOut");
			imgVideoOut.SetFromStock(videoOutStock, videoOutSize);*/
		}
		private void RetrieveImage(object sender, Tracap.ImageProcessedEventArgs args) {
			int width = 0;
			int height = 0;
			int totalBytes = 0;
			bool pixelsAvailable = false;
			using (var imgRef = args.Image) {
				var imgSource = imgRef.Reference;
				if (imgSource.NumberOfChannels == 1 && imgSource.ElementSize == sizeof(byte)) {
					width = imgSource.Width;
					height = imgSource.Height;
					totalBytes = width * height * imgSource.ElementSize;
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
						int pixel = b << 16 | b << 8 | b;
						Marshal.WriteInt32(target, p * 3, pixel); // works as expected, but where is the forth byte located?
					}
					if (updatePixBuf) {
						if (videoOut.Pixbuf != null) {
							videoOut.Pixbuf.Dispose();
						}
						videoOut.Pixbuf = activeImage;
					}
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
