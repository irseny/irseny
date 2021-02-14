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

namespace Irseny.Core.Shared {
	/// <summary>
	/// Shared interface for raster image containers.
	/// </summary>
	public interface IRasterImageBase {
		/// <summary>
		/// Gets the image width.
		/// </summary>
		/// <value>The width.</value>
		int Width { get; }
		/// <summary>
		/// Gets the image height
		/// </summary>
		/// <value>The height.</value>
		int Height { get; }
		/// <summary>
		/// Gets the formatting information of pixels in the image.
		/// </summary>
		/// <value>The pixel format.</value>
		RasterImagePixelFormat PixelFormat { get; }
		/// <summary>
		/// Gets the size in bytes of a single pixel in memory.
		/// </summary>
		/// <value>The size of a pixel.</value>
		int PixelSize { get; }
		/// <summary>
		/// Gets the image data buffer.
		/// </summary>
		/// <value>The pixel data.</value>
		byte[] PixelData { get; }
	}
}

