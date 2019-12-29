using System;

namespace Irseny.Core.Capture.Video {
	public class ImageCapturedEventArgs : StreamEventArgs {
		Util.SharedRef<Emgu.CV.Mat> colorImage;
		Util.SharedRef<Emgu.CV.Mat> grayImage;

		public ImageCapturedEventArgs(CaptureStream stream, int streamId, Util.SharedRef<Emgu.CV.Mat> colorImage, Util.SharedRef<Emgu.CV.Mat> grayImage) : base(stream, streamId) {
			if (colorImage == null) throw new ArgumentNullException("colorImage");
			if (grayImage == null) throw new ArgumentNullException("grayImage");
			this.colorImage = colorImage;
			this.grayImage = grayImage;
		}
		public Util.SharedRef<Emgu.CV.Mat> ColorImage {
			get { return Util.SharedRef.Copy(colorImage); }
		}
		public Util.SharedRef<Emgu.CV.Mat> GrayImage {
			get { return Util.SharedRef.Copy(grayImage); }
		}
	}
}
