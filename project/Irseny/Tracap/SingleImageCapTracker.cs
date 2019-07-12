﻿using System;
using System.Collections.Generic;
using Irseny.Util;

namespace Irseny.Tracap {
	public abstract class SingleImageCapTracker : CapTracker, ISingleImageCapTracker {
		readonly object inputSync = new object();
		readonly object processedEventSync = new object();
		TrackerSettings settings;
		Queue<SharedRef<Emgu.CV.Mat>> pendingImages = new Queue<SharedRef<Emgu.CV.Mat>>();
		event EventHandler<ImageProcessedEventArgs> imageProcessed;

		public SingleImageCapTracker(TrackerSettings settings) : base() {
			if (settings == null) throw new ArgumentNullException("settings");
			// TODO: move settings passing to Start()
			this.settings = settings;
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
		protected abstract bool Step(SharedRef<Emgu.CV.Mat> image);

		public override bool Step() {
			if (!Running) {
				return false;
			}

			SharedRef<Emgu.CV.Mat> image = null;
			lock (inputSync) {
				if (pendingImages.Count > 0) {
					image = pendingImages.Dequeue();
				}
			}
			while (image != null) {
				using (image) {
					bool result = false;
					if (image.Reference != null) {
						result = Step(image);
						return true;
					}
				}

				lock (inputSync) {
					if (pendingImages.Count > 0) {
						image = pendingImages.Dequeue();
					} else {
						image = null;
					}
				}
			}
			return false;
		}
		public void QueueInput(SharedRef<Emgu.CV.Mat> image) {
			if (image == null) throw new ArgumentNullException("image");
			lock (inputSync) {
				pendingImages.Enqueue(SharedRef.Copy(image));
				int imageLimit = settings.GetInteger(TrackerProperty.MaxQueuedImages, 4);
				while (pendingImages.Count > imageLimit && pendingImages.Count >= 0) {
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
				foreach (SharedRef<Emgu.CV.Mat> image in pendingImages) {
					image.Dispose();
				}
				pendingImages.Clear();
			}
		}
	}
}
