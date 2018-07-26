using System;

namespace Irseny.Tracap {
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

	}
}
