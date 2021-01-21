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

namespace Irseny.Core.Tracking {
	public class ImageProcessedEventArgs : EventArgs {
		Util.SharedRef<Emgu.CV.Mat> image;
		/// <summary>
		/// Creates an instance of this class that holds the given image.
		/// </summary>
		/// <param name="image">Image argument. 
		/// Copies of the given instance are created when the image is used through the respective property of this class.</param>
		public ImageProcessedEventArgs(Util.SharedRef<Emgu.CV.Mat> image) {
			if (image == null) throw new ArgumentNullException("image");
			this.image = image;
		}
		/// <summary>
		/// Gets the image argument.
		/// </summary>
		/// <value>The image. Note that the instance returned is a copy and should be disposed after usage.</value>
		public Util.SharedRef<Emgu.CV.Mat> Image {
			get {
				return Util.SharedRef.Copy(image);
			}
		}
	}
}
