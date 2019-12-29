using System;

namespace Irseny.Inco.Device {
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
