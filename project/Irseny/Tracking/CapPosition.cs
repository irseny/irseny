using System;

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
		public float Yaw { get; set; }
		public float Pitch { get; set; }
		public float Roll { get; set; }
		public float PosX { get; set; }
		public float PosY { get; set; }
		public float PosZ { get; set; }

		public float GetAxis(CapAxis axis) {
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
				return 0f;
			}
		}
		public void SetAxis(CapAxis axis, float value) {
			switch (axis) {
			case CapAxis.X:
				PosX = value;
				break;
			case CapAxis.Y:
				PosY = value;
				break;
			case CapAxis.Z:
				PosZ = value;
				break;
			case CapAxis.Yaw:
				Yaw = value;
				break;
			case CapAxis.Pitch:
				Pitch = value;
				break;
			case CapAxis.Roll:
				Roll = value;
				break;
			default:
				break;
			}
		}

	}
}
