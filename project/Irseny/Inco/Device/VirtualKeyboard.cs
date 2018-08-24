using System;

namespace Irseny.Inco.Device {
	public class VirtualKeyboard : IVirtualDevice {
		IntPtr deviceHandle = IntPtr.Zero;
		public VirtualKeyboard() {
		}
		public VirtualDeviceType DeviceType {
			get { return VirtualDeviceType.Keyboard; }
		}
		public bool SupportsAccess(VirtualDeviceAccess access) {
			switch (access) {
			case VirtualDeviceAccess.None:
				return true;
			case VirtualDeviceAccess.Read:
				return true;
			case VirtualDeviceAccess.Write:
				return false;
			default:
				return false;
			}

		}
		public VirtualDeviceCapability[] GetSupportedCapabilities() {
			return new VirtualDeviceCapability[] { VirtualDeviceCapability.Button };
		}
		public int GetKeyNo(VirtualDeviceCapability capability) {
			switch (capability) {
			case VirtualDeviceCapability.Button:
				return 255;
			default:
				return 0;
			}
		}
		public string GetKeyDescription(VirtualDeviceCapability capability, int index) {
			return string.Empty;
		}


	}
}

