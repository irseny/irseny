using System;

namespace Irseny.Tracking {
	public class ImageProcessedEventArgs : EventArgs {
		Util.SharedRef<Emgu.CV.Mat> image;
		/// <summary>
		/// Creates an instance of this class that holds the given image.
		/// </summary>
		/// <param name="image">Image argument. 
		/// Copies of the given instance are created when the image is used through the respective property of this class.</param>
		public ImageProcessedEventArgs(Util.SharedRef<Emgu.CV.Mat> image) {
			if (image == null) throw new ArgumentNullException("image");
			this.image = image;
		}
		/// <summary>
		/// Gets the image argument.
		/// </summary>
		/// <value>The image. Note that the instance returned is a copy and should be disposed after usage.</value>
		public Util.SharedRef<Emgu.CV.Mat> Image {
			get {
				return Util.SharedRef.Copy(image);
			}
		}
	}
}
