using System;
using System.IO;

namespace Irseny.Viol.Main.Image.Camera {
	public class CameraFactory : InterfaceFactory {
		public CameraFactory(int index) : base() {
			Index = index;
		}
		private readonly int Index;

		protected override bool CreateInternal() {
			var factory = Mycena.InterfaceFactory.CreateFromFile(Content.ContentMaster.Instance.Resources.InterfaceDefinitions.GetEntry("CameraImage"));
			Container = factory.CreateWidget("box_Root");
			return true;
		}
		protected override bool ConnectInternal() {
			Listing.EquipmentMaster.Instance.VideoCaptureStream.Updated += StreamStateChanged;
			return true;
		}
		protected override bool DisconnectInternal() {
			Listing.EquipmentMaster.Instance.VideoCaptureStream.Updated -= StreamStateChanged;
			return true;
		}
		protected override bool DestroyInternal() {
			Container.Dispose();
			return true;
		}
		private void StreamStateChanged(object sender, Listing.EquipmentUpdateArgs<int> args) {
			if (args.IndexChanged == Index) {
				Invoke(delegate {
					if (args.Available) {
						StartCapture();
					} else {
						StopCapture();
					}
				});
				Console.WriteLine("capture equipment available: " + args.Available);
			}
		}
		private void RetrieveImage(object sender, Capture.Video.CaptureImageEventArgs args) {
			Invoke(delegate {				
				var videoOut = Container.GetWidget<Gtk.Image>("img_VideoOut");
				//var imgMatrix = args.Image;
				using (var imgSource = args.Image) { // TODO: outsource disposing to shared reference
					//using (var imgMatrix = new Emgu.CV.Mat(args.Image.Size, Emgu.CV.CvEnum.DepthType.Cv8U, 1)) {
					//args.Image.ConvertTo(imgMatrix, Emgu.CV.CvEnum.DepthType.Cv8U);
					//Emgu.CV.CvInvoke.Threshold(args.Image, imgMatrix, 50, 255, Emgu.CV.CvEnum.ThresholdType.Binary);
					//Emgu.CV.CvInvoke.CvtColor(imgSource, imgMatrix, Emgu.CV.CvEnum.ColorConversion.Bgr2Gray);

					using (var imgStream = new MemoryStream()) {
						using (var bmp = imgSource.Bitmap) {
							bmp.Save(imgStream, System.Drawing.Imaging.ImageFormat.Bmp);
						}
//						if (imgSource.NumberOfChannels == 3 && imgSource.ElementSize == 1) {
//							byte[] buffer = new byte[imgSource.Width*imgSource.Height];
//							Array.Copy(imgSource.Data, buffer, buffer.Length);
//							Gdk.Pixdata data;
//							new Gdk.Pixbuf(buffer, Gdk.Colorspace.Rgb, false, 8, imgSource.Width, imgSource.Height, imgSource.Width);
//							Gdk.Pixbuf.FromPixdata(data, false);
//						}

						//new Gdk.Pixbuf()
						imgStream.Position = 0;
						//Gdk.Pixbuf.FromPixdata(new Gdk.Pixdata().)
						if (videoOut.Pixbuf != null) {
							videoOut.Pixbuf.Dispose();
						}
						videoOut.Pixbuf = new Gdk.Pixbuf(imgStream);
					}
					//}
				}
				videoOut.QueueDraw();
			});
			/*Invoke(delegate {
				args.Image.Dispose();
				Console.WriteLine("disposing captured image");
			});*/
			/*Invoke(delegate {
				args.Image.Dispose();
				long mem = GC.GetTotalMemory(false);
				Console.WriteLine("Memory being used: {0:0,0}", mem);
				
			});*/
		}
		public bool StartCapture() {
			Capture.Video.CaptureStream stream = Capture.Video.CaptureSystem.Instance.GetStream(Index);
			if (stream != null) {
				stream.ImageAvailable += RetrieveImage;
			}
			return false;
		}
		public bool StopCapture() {
			Capture.Video.CaptureStream stream = Capture.Video.CaptureSystem.Instance.GetStream(Index);
			if (stream != null) {
				stream.ImageAvailable -= RetrieveImage;
			}

			return true;
		}
	}
}

