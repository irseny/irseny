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

namespace Irseny.Core.Inco.Device {
	public struct KeyState {
		float raw;
		float smooth;

		public KeyState(float raw, float smooth) {
			this.raw = raw;
			this.smooth = smooth;
		}
		public KeyState(bool raw, bool smooth) {
			this.raw = raw ? 1.0f : 0.0f;
			this.smooth = smooth ? 1.0f : 0.0f;
		}
		public float RawAxis {
			get { return raw; }
		}
		public float SmoothAxis {
			get { return smooth; }
		}
		public bool RawPressed {
			get { return raw > 0.0f; }
		}
		public bool SmoothPressed {
			get { return smooth > 0.0f; }
		}
	}
}
