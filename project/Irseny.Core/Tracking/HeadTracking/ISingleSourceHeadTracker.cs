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

namespace Irseny.Core.Tracking.HeadTracking {
	public interface ISingleSourceHeadTracker : IPoseTracker {
		/// <summary>
		/// Occurs when an input image has been processed.
		/// </summary>
		event EventHandler<ImageProcessedEventArgs> InputProcessed;
		/// <summary>
		/// Queues an image for pose detection. Creates a copy of the given reference.
		/// </summary>
		/// <param name="image">Image.</param>
		void QueueInput(Util.SharedRef<IRasterImageBase> image);
	}
}
