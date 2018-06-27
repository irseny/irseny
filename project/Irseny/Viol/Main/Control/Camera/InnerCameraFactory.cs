using System;
using System.IO;

namespace Irseny.Viol.Main.Control.Camera {
	public class InnerCameraFactory : InterfaceFactory {
		public InnerCameraFactory() : base() {
		}
		protected override bool CreateInternal() {
			var factory = Mycena.InterfaceFactory.CreateFromFile(Content.ContentMaster.Instance.Resources.InterfaceDefinitions.GetEntry("InnerCameraControl"));
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
						StartImageUpdate(stream);
						Console.WriteLine("capture started");
					});
				};
				stream.CaptureStopped += delegate {
					Invoke(delegate {
						StopCapture();
					});
				};
				stream.Start(new Capture.Video.CaptureSettings());
				if (!stream.Capturing) {
					Console.WriteLine("failed to start video capture stream");
				}
			});
		}
		private void StartImageUpdate(Capture.Video.CaptureStream stream) {
			stream.ImageAvailable += (object sender, Capture.Video.CaptureImageEventArgs args) => {
				Invoke(delegate {
					var videoOut = Container.GetWidget<Gtk.Image>("img_VideoOut");
					var imgMatrix = args.Image;
					var imgStream = new MemoryStream();
					imgMatrix.Bitmap.Save(imgStream, System.Drawing.Imaging.ImageFormat.Bmp);
					imgStream.Position = 0;
					//Gdk.Pixbuf.FromPixdata(new Gdk.Pixdata().)
					videoOut.Pixbuf = new Gdk.Pixbuf(imgStream);
					imgMatrix.Dispose();
					videoOut.QueueDraw();

				});
			};
		}
		private void StopCapture() {
			Console.WriteLine("stop capture stub");
		}
	}
}
