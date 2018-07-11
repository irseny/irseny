using System;

namespace Irseny.Tracap {
	public class ImageEventArgs : EventArgs {
		Util.SharedRef<Emgu.CV.Mat> image;
		public ImageEventArgs(Util.SharedRef<Emgu.CV.Mat> image) {
			if (image == null) throw new ArgumentNullException("image");
			this.image = image;
		}
		public Util.SharedRef<Emgu.CV.Mat> Image {
			get {
				return Util.SharedRef.Copy(image);
			}
		}
	}
}
