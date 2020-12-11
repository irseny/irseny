using System;
using System.Drawing.Imaging;

namespace Irseny.Core.Sensors.VideoCapture {
	/// <summary>
	/// Container for image data captured by a video camera.
	/// </summary>
	public class VideoFrame {
		
		public VideoFrame(int width, int height, VideoFramePixelFormat format, byte[] data) {
			if (width < 0 || width > 16384) throw new ArgumentOutOfRangeException("width");
			if (height < 0 || height > 16884) throw new ArgumentOutOfRangeException("height");
			if (data == null) throw new ArgumentNullException("data");
			Width = width;
			Height = height;
			Format = format;
			Data = data;
		}
		public int Width { get; private set; }
		public int Height { get; private set; }
		public VideoFramePixelFormat Format { get; private set; }
		public byte[] Data { get; private set; }

		/// <summary>
		/// Gets the bitmap format that corresponds to the given video frame format.
		/// </summary>
		/// <returns>The target format.</returns>
		/// <param name="format">Source format.</param>
		public static PixelFormat GetBitmapFormat(VideoFramePixelFormat format) {
			switch (format) {
			case VideoFramePixelFormat.Gray8:
				return PixelFormat.Alpha;
			case VideoFramePixelFormat.Gray16:
				return PixelFormat.Format16bppGrayScale;
			case VideoFramePixelFormat.RGB24:
				return PixelFormat.Format24bppRgb;
			case VideoFramePixelFormat.ARGB32:
				return PixelFormat.Format32bppArgb;
			default:
				throw new ArgumentException("No complementary format found", "format");
			}
		}
		/// <summary>
		/// Gets the size of a single hypothetical pixel in memory.
		/// </summary>
		/// <returns>The pixel size.</returns>
		/// <param name="format">Format of the pixel.</param>
		public static int GetPixelSize(VideoFramePixelFormat format) {
			switch (format) {
			case VideoFramePixelFormat.Gray8:
				return sizeof(byte)*1;
			case VideoFramePixelFormat.Gray16:
				return sizeof(byte)*2;
			case VideoFramePixelFormat.RGB24:
				return sizeof(byte)*3;
			case VideoFramePixelFormat.ARGB32:
				return sizeof(byte)*4;
			default:
				return -1;
			}
		}
	}
}

