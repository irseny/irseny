using System;

namespace Irseny.Tracap {
	public class ImageEventArgs : EventArgs {
		public ImageEventArgs(Emgu.CV.Mat image) {
			if (image == null) throw new ArgumentNullException("image");
			Image = image;
		}
		public Emgu.CV.Mat Image { get; private set; }
	}
}

