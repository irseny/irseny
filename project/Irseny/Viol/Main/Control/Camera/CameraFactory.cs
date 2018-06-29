using System;
using System.IO;

namespace Irseny.Viol.Main.Control.Camera {
	public class CameraFactory : InterfaceFactory {
		public CameraFactory(int index) : base() {
			Index = index;
		}
		private readonly int Index;
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
				} else {
					StopCapture();
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
			
			return true;
		}
		protected override bool DestroyInternal() {
			Container.Dispose();
			return true;
		}
		private void StartCapture() {
			Capture.Video.CaptureSystem.Instance.Invoke(delegate {
				int streamId = Capture.Video.CaptureSystem.Instance.CreateStream();
				Capture.Video.CaptureStream stream = Capture.Video.CaptureSystem.Instance.GetStream(streamId);
				stream.CaptureStarted += delegate {
					Invoke(delegate {
						//StartImageUpdate(stream);
						Console.WriteLine("capture started");
					});
				};
				stream.CaptureStarted += delegate {
					Listing.EquipmentMaster.Instance.VideoCaptureStream.Update(Index, true, streamId);
				};
				stream.CaptureStopped += delegate {
					Invoke(delegate {
						StopCapture();
					});
				};
				stream.CaptureStopped += delegate {
					Listing.EquipmentMaster.Instance.VideoCaptureStream.Update(Index, false, -1);
				};
				stream.Start(new Capture.Video.CaptureSettings());
				if (!stream.Capturing) {
					Console.WriteLine("failed to start video capture stream");
				}
			});
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
			Console.WriteLine("stop capture stub");
		}
	}
}
