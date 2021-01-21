// This file is part of Irseny.
//
// Copyright (C) 2021  Thilo Gabel
//
// Irseny is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
//
// Irseny is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
//
// You should have received a copy of the GNU General Public License
// along with this program.  If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;
using Irseny.Core.Util;

namespace Irseny.Core.Tracking {
	public abstract class SingleImageCapTracker : CapTracker {
		readonly object inputSync = new object();
		readonly object processedEventSync = new object();
		Queue<SharedRef<Emgu.CV.Mat>> pendingImages = new Queue<SharedRef<Emgu.CV.Mat>>();
		event EventHandler<ImageProcessedEventArgs> imageProcessed;


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
			if (!Running) {
				return;
			}
			lock (inputSync) {
				pendingImages.Enqueue(SharedRef.Copy(image));
				// TODO: get limit efficiently
				int imageLimit = 4;//GetSettings().GetInteger(TrackerProperty.MaxQueuedImages, 4);
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
		public abstract EquipmentSettings GetSettings();
	}
}
