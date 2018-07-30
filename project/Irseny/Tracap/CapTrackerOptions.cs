using System;
using System.Collections.Generic;
using Size = System.Drawing.Size;

namespace Irseny.Tracap {
	public class CapTrackerOptions : ICapTrackerOptions {
		List<int> streams;
		/// <summary>
		/// Creates an instance of this class with default values.
		/// </summary>
		public CapTrackerOptions() {
			this.streams = new List<int>(2);
			this.Smoothing = 0;
			this.ApproximateImageSize = new Size(640, 480);
			this.MaxQueuedImages = 8;
			this.ModelId = 0;
		}

		/// <summary>
		/// Creates a copy of the given instance.
		/// </summary>
		/// <param name="source">Source.</param>
		public CapTrackerOptions(CapTrackerOptions source) {
			if (source == null) throw new ArgumentNullException("source");
			this.streams = new List<int>(source.streams);
			this.Smoothing = source.Smoothing;
			this.ApproximateImageSize = source.ApproximateImageSize;
			this.MaxQueuedImages = source.MaxQueuedImages;
			this.ModelId = source.ModelId;
		}

		public int Smoothing { get; set; }
		public Size ApproximateImageSize { get; set; }
		public int MaxQueuedImages { get; set; }
		public int ModelId { get; set; }
		public int StreamNo {
			get { return streams.Count; }
		}
		public int GetStreamId(int index) {
			if (index < 0 || index >= streams.Count) {
				return -1;
			} else {
				return streams[index];
			}
		}
		public void AddStreamId(int id) {
			streams.Add(id);
		}
	}
}