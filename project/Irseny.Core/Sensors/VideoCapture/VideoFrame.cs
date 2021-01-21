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
using System.Drawing.Imaging;

namespace Irseny.Core.Sensors.VideoCapture {
	/// <summary>
	/// Container for image data captured by a video camera.
	/// </summary>
	public class VideoFrame {
		/// <summary>
		/// Initializes a new instance of the <see cref="Irseny.Core.Sensors.VideoCapture.VideoFrame"/> class.
		/// </summary>
		/// <param name="width">Width.</param>
		/// <param name="height">Height.</param>
		/// <param name="format">Pixel format.</param>
		/// <param name="data">Pixel data.</param>
		/// <param name="metadata">Associated metadata.</param>
		public VideoFrame(int width, int height, VideoFramePixelFormat format, byte[] data, VideoFrameMetadata metadata) {
			if (width < 0 || width > 16384) throw new ArgumentOutOfRangeException("width");
			if (height < 0 || height > 16884) throw new ArgumentOutOfRangeException("height");
			if (data == null) throw new ArgumentNullException("data");
			if (metadata.FrameRate < 0) throw new ArgumentOutOfRangeException("metadata.FrameRate");
			if (metadata.FrameTime < 0) throw new ArgumentOutOfRangeException("metadata.FrameTime");
			if (metadata.FrameTimeDeviation < 0) throw new ArgumentOutOfRangeException("metadata.FrameTimeVariance");
			Width = width;
			Height = height;
			Format = format;
			Data = data;
			Metadata = metadata;
		}
		/// <summary>
		/// Gets the image width.
		/// </summary>
		/// <value>The width.</value>
		public int Width { get; private set; }
		/// <summary>
		/// Gets the image height.
		/// </summary>
		/// <value>The height.</value>
		public int Height { get; private set; }
		/// <summary>
		/// Gets the pixel format.
		/// </summary>
		/// <value>The pixel format.</value>
		public VideoFramePixelFormat Format { get; private set; }
		/// <summary>
		/// Gets the image data buffer.
		/// </summary>
		/// <value>The image data buffer.</value>
		public byte[] Data { get; private set; }
		/// <summary>
		/// Gets the metadata associated with the video capture.
		/// </summary>
		/// <value>The video metadata.</value>
		public VideoFrameMetadata Metadata { get; private set; }
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

