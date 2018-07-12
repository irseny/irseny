using System;
namespace Irseny.Tracap {
	public struct CapPosition {


		public CapPosition(CapPosition source) {
			this.Yaw = source.Yaw;
			this.Pitch = source.Pitch;
		}
		public float Yaw { get; set; }
		public float Pitch { get; set; }

	}
}
