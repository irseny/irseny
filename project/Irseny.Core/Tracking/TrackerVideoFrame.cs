using System;
using Irseny.Core.Shared;

namespace Irseny.Core.Tracking {
	/// <summary>
	/// Video frame produced by trackers with one byte per pixel (gray scale image).
	/// </summary>
	public class TrackerVideoFrame : IRasterImageBase {
		/// <summary>
		/// Initializes a new instance of the <see cref="Irseny.Core.Tracking.TrackerVideoFrame"/> class.
		/// </summary>
		/// <param name="width">Image width.</param>
		/// <param name="height">Image height.</param>
		/// <param name="pixelData">Pixel data.</param>
		public TrackerVideoFrame(int width, int height, byte[] pixelData) {
			if (width < 0 || width >= 35536) throw new ArgumentOutOfRangeException("width");
			if (height < 0 || height >= 35536) throw new ArgumentOutOfRangeException("height");
			if (pixelData == null) throw new ArgumentNullException("pixelData");
			Width = width;
			Height = height;
			PixelData = pixelData;
		}

		public int Width { get; private set; }

		public int Height { get; private set; }

		public RasterImagePixelFormat PixelFormat {
			get { return RasterImagePixelFormat.Gray8; }
		}

		public int PixelSize {
			get { return 1; }
		}
				
		public byte[] PixelData { get; private set; }
	}
}

