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
			streams = new List<int>(2);
			Smoothing = 0;
			ApproximateImageSize = new Size(640, 480);
			MaxQueuedImages = 8;
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
		}

		public int Smoothing { get; set; }
		public Size ApproximateImageSize { get; set; }
		public int MaxQueuedImages { get; set; }
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