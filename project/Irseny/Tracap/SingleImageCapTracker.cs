using System;
using System.Collections.Generic;
namespace Irseny.Tracap {
	public abstract class SingleImageCapTracker : CapTracker, ISingleImageCapTracker {
		readonly object inputSync = new object();
		readonly object processedEventSync = new object();
		Queue<Util.SharedRef<Emgu.CV.Mat>> pendingImages = new Queue<Util.SharedRef<Emgu.CV.Mat>>();
		event EventHandler<ImageEventArgs> imageProcessed;

		public SingleImageCapTracker() : base() {
		}
		public event EventHandler<ImageEventArgs> InputProcessed {
			add {
				lock (processedEventSync) {
					imageProcessed += value;
				}
			}
			remove {
				lock (processedEventSync) {
					imageProcessed -= value;
				}
			}
		}
		protected void OnInputProcessed(ImageEventArgs args) {
			if (args == null) throw new ArgumentNullException("args");
			EventHandler<ImageEventArgs> handler;
			lock (processedEventSync) {
				handler = imageProcessed;
			}
			if (handler != null) {
				handler(this, args);
			}
		}
		/// <summary>
		/// Processes the given image.
		/// </summary>
		/// <returns>Whether the operation was successful.</returns>
		/// <param name="image">Non disposed an available image to process. May be disposed after the method returns.</param>
		protected abstract bool Step(Util.SharedRef<Emgu.CV.Mat> image);

		public override bool Step() {
			Util.SharedRef<Emgu.CV.Mat> image = null;
			lock (inputSync) {
				if (pendingImages.Count > 0) {
					image = pendingImages.Dequeue();
				}
			}
			if (image != null && image.Reference != null) {
				bool result = false;
				if (image.Reference != null) {
					result = Step(image);
				}
				image.Dispose();
				return true;
			}
			return false;
		}
		public void QueueInput(Util.SharedRef<Emgu.CV.Mat> image) {
			if (image == null) throw new ArgumentNullException("image");
			lock (inputSync) {
				pendingImages.Enqueue(Util.SharedRef.Copy(image));
			}
			OnInputAvailable(new EventArgs());
		}
	}
}
