using System;
using System.Collections.Generic;
using Size = System.Drawing.Size;

namespace Irseny.Tracap {
	public class CapTrackerOptions {
		List<int> streams;
		/// <summary>
		/// Creates an instance of this class with default values.
		/// </summary>
		public CapTrackerOptions() {
			streams = new List<int>(2);
			Threshold = 253;
			Smoothing = 0;
			MaxPointNo = 1000;
			ApproximateImageSize = new Size(640, 480);
			MaxClusterNo = 3;
			MaxImagesQueued = 8;
		}
		/// <summary>
		/// Creates a copy of the given instance.
		/// </summary>
		/// <param name="source">Source.</param>
		public CapTrackerOptions(CapTrackerOptions source) {
			if (source == null) throw new ArgumentNullException("source");
			this.streams = new List<int>(source.streams);
			this.Threshold = source.Threshold;
			this.Smoothing = source.Smoothing;
			this.MaxPointNo = source.MaxPointNo;
			this.MaxClusterNo = source.MaxClusterNo;
			this.ApproximateImageSize = source.ApproximateImageSize;
			this.MaxImagesQueued = source.MaxImagesQueued;
		}
		/// <summary>
		/// Gets or sets the brightness threshold that a pixel must pass to get identified as a point.
		/// </summary>
		/// <value>The brightness threshold.</value>
		public int Threshold { get; set; }
		/// <summary>
		/// Gets or sets the amount of smoothing to apply to detected head positions.
		/// </summary>
		/// <value>The smoothing amount.</value>
		public int Smoothing { get; set; }
		/// <summary>
		/// Gets or sets the maximum number of points that should be detecatble.
		/// If this limit is surpassed further points might get discarded.
		/// </summary>
		/// <value>The max point no.</value>
		public int MaxPointNo { get; set; }
		/// <summary>
		/// Gets or sets the maximum number of clusters that should be detectable.
		/// if this limit is surpassed further clusters might get discarded.
		/// </summary>
		/// <value>The max cluster no.</value>
		public int MaxClusterNo { get; set; }
		/// <summary>
		/// Gets or sets a size that the images retrieved are likely to have.
		/// </summary>
		/// <value>The size of the approximate image.</value>
		public Size ApproximateImageSize { get; set; }
		/// <summary>
		/// Gets or sets the image queue length limit. Older images will be discarded if this limit is surpassed.
		/// </summary>
		/// <value>The maximum number of images queued at a time.</value>
		public int MaxImagesQueued { get; set; }
		/// <summary>
		/// Gets the number of capture streams to retrieve images from.
		/// </summary>
		/// <value>The stream number.</value>
		public int StreamNo {
			get { return streams.Count; }
		}
		/// <summary>
		/// Gets the id of a capture stream that was previously added.
		/// </summary>
		/// <returns>The stream identifier.</returns>
		/// <param name="index">Index.</param>
		public int GetStreamId(int index) {
			if (index < 0 || index >= streams.Count) {
				return -1;
			} else {
				return streams[index];
			}
		}
		/// <summary>
		/// Adds the given id of a capture stream for image retrieval.
		/// </summary>
		/// <param name="id">Capture stream identifier.</param>
		public void AddStreamId(int id) {
			streams.Add(id);
		}
	}
}