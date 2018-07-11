using System;
using System.IO;

namespace Irseny.Viol.Main.Control.Camera {
	public class CameraFactory : InterfaceFactory {
		private readonly int index;

		public CameraFactory(int index) : base() {
			this.index = index;
		}

		protected override bool CreateInternal() {
			var factory = Mycena.InterfaceFactory.CreateFromFile(Content.ContentMaster.Instance.Resources.InterfaceDefinitions.GetEntry("CameraControl"));
			Container = factory.CreateWidget("box_Root");
			return true;
		}
		protected override bool ConnectInternal() {
			var btnCapture = Container.GetWidget<Gtk.ToggleButton>("btn_Capture");
			/*btnCapture.Toggled += delegate {
				if (btnCapture.Active) {
					StartCapture();
				} else {
					StopCapture();
				}
			};*/
			btnCapture.Clicked += delegate {
				if (btnCapture.Active) {
					StartCapture();
					Console.WriteLine("start");
				} else {
					StopCapture();
					Console.WriteLine("stop");
				}
				/*Console.WriteLine("capture button clicked");
				var btn = Container.GetWidget<Gtk.ToggleButton>("btn_Capture");
				Console.WriteLine("button active: " + btn.Active);
				StartCapture();*/
			};
			// TODO: connect value setting widgets with value visualizers
			// update video sources

			return true;
		}
		protected override bool DisconnectInternal() {
			StopCapture();
			return true;
		}
		protected override bool DestroyInternal() {
			Container.Dispose();
			return true;
		}
		private void StartCapture() {
			bool streamAvailable = Listing.EquipmentMaster.Instance.VideoCaptureStream.GetState(index) != Irseny.Listing.EquipmentState.Missing;
			if (!streamAvailable) {
				Capture.Video.CaptureSystem.Instance.Invoke(delegate {
					int streamId = Capture.Video.CaptureSystem.Instance.CreateStream();
					Capture.Video.CaptureStream stream = Capture.Video.CaptureSystem.Instance.GetStream(streamId);
					stream.CaptureStarted += delegate {
						Listing.EquipmentMaster.Instance.VideoCaptureStream.Update(index, Listing.EquipmentState.Active, streamId);
					};
					stream.CaptureStarted += delegate {
						Capture.Video.CaptureSettings settings = stream.Settings;
						Invoke(delegate {
							// TODO: apply settings
						});
					};
					stream.Start(new Capture.Video.CaptureSettings()); // TODO: get settings
				});
			} else {
				Irseny.Log.LogManager.Instance.Log(Irseny.Log.LogMessage.CreateWarning(this, "Unable to start capture: still running"));
			}
		}
		/*private void StartImageUpdate(Capture.Video.CaptureStream stream) {
			stream.ImageAvailable += (object sender, Capture.Video.CaptureImageEventArgs args) => {
				Invoke(delegate {
					var videoOut = Container.GetWidget<Gtk.Image>("img_VideoOut");
					//var imgMatrix = args.Image;
					var imgMatrix = new Emgu.CV.Mat(args.Image.Size, Emgu.CV.CvEnum.DepthType.Cv8U, 1);
					//args.Image.ConvertTo(imgMatrix, Emgu.CV.CvEnum.DepthType.Cv8U);
					//Emgu.CV.CvInvoke.Threshold(args.Image, imgMatrix, 50, 255, Emgu.CV.CvEnum.ThresholdType.Binary);
					Emgu.CV.CvInvoke.CvtColor(args.Image, imgMatrix, Emgu.CV.CvEnum.ColorConversion.Bgr2Gray);

					var imgStream = new MemoryStream();
					imgMatrix.Bitmap.Save(imgStream, System.Drawing.Imaging.ImageFormat.Bmp);
					imgStream.Position = 0;
					//Gdk.Pixbuf.FromPixdata(new Gdk.Pixdata().)
					videoOut.Pixbuf = new Gdk.Pixbuf(imgStream);
					imgMatrix.Dispose();
					videoOut.QueueDraw();

				});
			};
		}*/
		private void StopCapture() {
			bool streamAvailable = Listing.EquipmentMaster.Instance.VideoCaptureStream.GetState(index) != Irseny.Listing.EquipmentState.Missing;
			if (streamAvailable) {
				int streamId = Listing.EquipmentMaster.Instance.VideoCaptureStream.GetEquipment(index, -1);
				if (streamId > -1) {
					Listing.EquipmentMaster.Instance.VideoCaptureStream.Update(index, Listing.EquipmentState.Missing, -1);
					Capture.Video.CaptureSystem.Instance.Invoke(delegate {
						Capture.Video.CaptureSystem.Instance.DestroyStream(streamId);
						Console.WriteLine("stream destroyed");
					});
				}

			} else {
				Irseny.Log.LogManager.Instance.Log(Irseny.Log.LogMessage.CreateWarning(this, "Unable to stop capture: unavailable"));
			}
		}
	}
}
