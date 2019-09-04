using System;
using Irseny.Inco.Device;

namespace Irseny.Tracking {
	public struct CapPosition {


		public CapPosition(CapPosition source) {
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

		public KeyState GetAxis(CapAxis axis) {
			switch (axis) {
			case CapAxis.X:
				return PosX;
			case CapAxis.Y:
				return PosY;
			case CapAxis.Z:
				return PosZ;
			case CapAxis.Yaw:
				return Yaw;
			case CapAxis.Pitch:
				return Pitch;
			case CapAxis.Roll:
				return Roll;
			default:
				return new KeyState(0.0f, 0.0f);
			}
		}
		public void SetAxis(CapAxis axis, KeyState state) {
			switch (axis) {
			case CapAxis.X:
				PosX = state;
				break;
			case CapAxis.Y:
				PosY = state;
				break;
			case CapAxis.Z:
				PosZ = state;
				break;
			case CapAxis.Yaw:
				Yaw = state;
				break;
			case CapAxis.Pitch:
				Pitch = state;
				break;
			case CapAxis.Roll:
				Roll = state;
				break;
			default:
				break;
			}
		}

	}
}
