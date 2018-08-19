using System;

namespace Irseny.Inco.Device {
	public class VirtualKeyboard : IVirtualDevice {
		IntPtr deviceHandle = IntPtr.Zero;
		public VirtualKeyboard() {
		}
		public VirtualDeviceType DeviceType {
			get { return VirtualDeviceType.Keyboard; }
		}
		public int ButtonCount {
			get { return 0; }
		}
		public int AxisCount {
			get { return 0; }
		}
		public int KeyCount {
			get { throw new NotImplementedException(); }
		}
		public string GetButtonDescription(int button) {
			throw new NotSupportedException();
		}
		public string GetAxisDescription(int axis) {
			throw new NotSupportedException();
		}
		public string GetKeyDescription(int key) {
			return key.ToString();
		}
	}
}

