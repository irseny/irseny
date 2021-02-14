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
using Irseny.Core.Inco.Device;

namespace Irseny.Core.Tracking.HeadTracking {
	public struct HeadPostition {


		public HeadPostition(HeadPostition source) {
			this.Yaw = source.Yaw;
			this.Pitch = source.Pitch;
			this.Roll = source.Roll;
			this.PosX = source.PosX;
			this.PosY = source.PosY;
			this.PosZ = source.PosZ;
		}
		public KeyState Yaw { get; set; }
		public KeyState Pitch { get; set; }
		public KeyState Roll { get; set; }
		public KeyState PosX { get; set; }
		public KeyState PosY { get; set; }
		public KeyState PosZ { get; set; }

		public KeyState GetAxis(HeadAxis axis) {
			switch (axis) {
			case HeadAxis.X:
				return PosX;
			case HeadAxis.Y:
				return PosY;
			case HeadAxis.Z:
				return PosZ;
			case HeadAxis.Yaw:
				return Yaw;
			case HeadAxis.Pitch:
				return Pitch;
			case HeadAxis.Roll:
				return Roll;
			default:
				return new KeyState(0.0f, 0.0f);
			}
		}
		public void SetAxis(HeadAxis axis, KeyState state) {
			switch (axis) {
			case HeadAxis.X:
				PosX = state;
				break;
			case HeadAxis.Y:
				PosY = state;
				break;
			case HeadAxis.Z:
				PosZ = state;
				break;
			case HeadAxis.Yaw:
				Yaw = state;
				break;
			case HeadAxis.Pitch:
				Pitch = state;
				break;
			case HeadAxis.Roll:
				Roll = state;
				break;
			default:
				break;
			}
		}

	}
}
