using System;
namespace Irseny.Inco.Device {
	public interface IVirtualDevice {
		VirtualDeviceType DeviceType { get; }
		VirtualDeviceCapability[] GetSupportedCapabilities();
		int GetKeyNo(VirtualDeviceCapability capability);
		string GetKeyDescription(VirtualDeviceCapability capability, int index);
		bool SupportsAccess(VirtualDeviceAccess mode);
	}
}
