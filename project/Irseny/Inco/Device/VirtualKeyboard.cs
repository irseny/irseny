using System;

namespace Irseny.Inco.Device {
	public class VirtualKeyboard : IVirtualDevice {
		IntPtr deviceHandle = IntPtr.Zero;
		int deviceIndex;

		public VirtualKeyboard(int index) {
			this.deviceIndex = index;
		}
		public VirtualDeviceType DeviceType {
			get { return VirtualDeviceType.Keyboard; }
		}
		public int DeviceIndex {
			get { return deviceIndex; }
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
				return 6;
			default:
				return 0;
			}
		}
		public string[] GetKeyDescriptions(VirtualDeviceCapability capability) {
			switch (capability) {
			case VirtualDeviceCapability.Key:
				return new string[] {
				"Q", "W", "E", "R", "T", "Z"
			};
			default:
				return new string[0];
			}
		}
		public object[] GetKeyHandles(VirtualDeviceCapability capability) {
			switch (capability) {
			case VirtualDeviceCapability.Key:
				return new object[] {
					"Q", "W", "E", "R", "T", "Z"
				};
			default:
				return new object[0];
			}
		}
	}
}
