using System;
using System.Collections.Generic;
namespace Irseny.Tracap {
	public abstract class SingleImageCapTracker : CapTracker, ISingleImageCapTracker {
		readonly object inputSync = new object();
		readonly object processedEventSync = new object();
		CapTrackerOptions options;
		Queue<Util.SharedRef<Emgu.CV.Mat>> pendingImages = new Queue<Util.SharedRef<Emgu.CV.Mat>>();
		event EventHandler<ImageProcessedEventArgs> imageProcessed;

		public SingleImageCapTracker(CapTrackerOptions options) : base() {
			if (options == null) throw new ArgumentNullException("options");
			this.options = options;
		}
		public event EventHandler<ImageProcessedEventArgs> InputProcessed {
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
		/// <summary>
		/// Invokes the InputProcessed event.
		/// </summary>
		/// <param name="args">Arguments.</param>
		protected void OnInputProcessed(ImageProcessedEventArgs args) {
			if (args == null) throw new ArgumentNullException("args");
			EventHandler<ImageProcessedEventArgs> handler;
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
		/// <param name="image">Image ready for processing. Disposed after the method returns.</param>
		protected abstract bool Step(Util.SharedRef<Emgu.CV.Mat> image);

		public override bool Step() {
			if (!Running) {
				return false;
			}
			Util.SharedRef<Emgu.CV.Mat> image = null;
			lock (inputSync) {
				if (pendingImages.Count > 0) {
					image = pendingImages.Dequeue();
				}
			}
			if (image != null) {
				using (image) {
					bool result = false;
					if (image.Reference != null) {
						result = Step(image);
						return true;
					}
				}
			}
			return false;
		}
		public void QueueInput(Util.SharedRef<Emgu.CV.Mat> image) {
			if (image == null) throw new ArgumentNullException("image");
			lock (inputSync) {
				pendingImages.Enqueue(Util.SharedRef.Copy(image));
				while (pendingImages.Count > options.MaxImagesQueued && pendingImages.Count >= 0) {
					pendingImages.Dequeue().Dispose();
				}
			}

			OnInputAvailable(new EventArgs());
		}
		public override void Dispose() {
			base.Dispose();
			lock (processedEventSync) {
				imageProcessed = null;
			}
			lock (inputSync) {
				foreach (Util.SharedRef<Emgu.CV.Mat> image in pendingImages) {
					image.Dispose();
				}
				pendingImages.Clear();
			}
		}
	}
}
