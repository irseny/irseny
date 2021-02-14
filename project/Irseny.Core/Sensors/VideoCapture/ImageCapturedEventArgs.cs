﻿// This file is part of Irseny.
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
using Irseny.Core.Shared;

namespace Irseny.Core.Sensors.VideoCapture {
	public class ImageCapturedEventArgs : StreamEventArgs {
		Util.SharedRef<IRasterImageBase> colorImage;
		Util.SharedRef<IRasterImageBase> grayImage;

		public ImageCapturedEventArgs(
			WebcamCapture stream, int streamId, Util.SharedRef<IRasterImageBase> colorImage, 
			Util.SharedRef<IRasterImageBase> grayImage) : base(stream, streamId) {
			if (colorImage == null) throw new ArgumentNullException("colorImage");
			if (grayImage == null) throw new ArgumentNullException("grayImage");
			this.colorImage = colorImage;
			this.grayImage = grayImage;
		}
		public Util.SharedRef<IRasterImageBase> ColorImage {
			get { return Util.SharedRef.Copy(colorImage); }
		}
		public Util.SharedRef<IRasterImageBase> GrayImage {
			get { return Util.SharedRef.Copy(grayImage); }
		}
	}
}
