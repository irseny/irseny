using System;
using System.IO;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace Irseny.Viol.Main.Image.Camera {
	public class CameraFactory : InterfaceFactory {
		byte[] pixelBuffer = new byte[0];
		Gdk.Pixbuf imgShow;
		public CameraFactory(int index) : base() {
			Index = index;
		}
		private readonly int Index;

		protected override bool CreateInternal() {
			var factory = Mycena.InterfaceFactory.CreateFromFile(Content.ContentMaster.Instance.Resources.InterfaceDefinitions.GetEntry("CameraImage"));
			Container = factory.CreateWidget("box_Root");
			imgShow = null; // target size unknown
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
			if (imgShow != null) {
				imgShow.Dispose();
				imgShow = null;
			}
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
			}
		}
		private void RetrieveImage(object sender, Capture.Video.CaptureImageEventArgs args) {
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
				if (imgSource.NumberOfChannels == 3 && imgSource.ElementSize == sizeof(byte)*3 && imgSource.DataPointer != IntPtr.Zero) {
					width = imgSource.Width;
					height = imgSource.Height;
					totalBytes = width*height*imgSource.ElementSize*sizeof(byte);
					if (pixelBuffer.Length < totalBytes) {
						pixelBuffer = new byte[totalBytes];
					}
					Marshal.Copy(imgSource.DataPointer, pixelBuffer, 0, totalBytes);
					pixelsAvailable = true;
				} else {
					Debug.WriteLine(this.GetType().Name + ": Retrieved image unusable.");
				}
			}
			Invoke(delegate {
				if (pixelsAvailable) {
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
		[DllImport("kernel32.dll", EntryPoint = "CopyMemory", SetLastError = false)]
		public static extern void CopyMemory(IntPtr dest, IntPtr src, uint count);

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

