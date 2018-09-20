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
			return new VirtualDeviceCapability[] { VirtualDeviceCapability.Key };
		}
		public int GetKeyNo(VirtualDeviceCapability capability) {
			switch (capability) {
			case VirtualDeviceCapability.Key:
				return 255;
			default:
				return 0;
			}
		}
		public string[] GetKeyDescriptions(VirtualDeviceCapability capability) {
			switch (capability) {
			case VirtualDeviceCapability.Key:
				return new string[] {
				"Q", "W", "E", "R", "Z"
			};
			default:
				return new string[0];
			}
		}
		public object[] GetKeyHandles(VirtualDeviceCapability capability) {
			switch (capability) {
			case VirtualDeviceCapability.Key:
				return new object[] {
					new object(), new object(), new object(), new object(), new object()
				};
			default:
				return new object[0];
			}
		}
	}
}
