using System;
using Size2i = System.Drawing.Size;

namespace Irseny.Tracap {
	public interface ICapTrackerOptions {


		/// <summary>
		/// Gets the amount of smoothing to apply to detected head positions.
		/// </summary>
		/// <value>The smoothing amount.</value>
		int Smoothing { get; }

		/// <summary>
		/// Gets a size that retrieved images are likely to have.
		/// </summary>
		/// <value>The approximate image size.</value>
		Size2i ApproximateImageSize { get; }

		/// <summary>
		/// Gets the image queue length limit. Older images will be discarded if this limit is surpassed.
		/// </summary>
		/// <value>The maximum number of images queued at a time.</value>
		int MaxQueuedImages { get; }
		/// <summary>
		/// Gets the number of capture streams to retrieve images from.
		/// </summary>
		/// <value>The stream number.</value>
		int StreamNo { get; }
		/// <summary>
		/// Gets the identifier of the cap model.
		/// </summary>
		/// <value>The model identifier.</value>
		int ModelId { get; }
		/// <summary>
		/// Gets the id of a capture stream that was previously added.
		/// </summary>
		/// <returns>The stream identifier.</returns>
		/// <param name="index">Index.</param>
		int GetStreamId(int index);
		/// <summary>
		/// Adds the given id of a capture stream for image retrieval.
		/// </summary>
		/// <param name="id">Capture stream identifier.</param>
		void AddStreamId(int id);
	}
}

