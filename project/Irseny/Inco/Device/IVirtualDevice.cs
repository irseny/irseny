using System;
namespace Irseny.Inco.Device {
	public interface IVirtualDevice {
		VirtualDeviceType DeviceType { get; }
		int DeviceIndex { get; }
		VirtualDeviceCapability[] GetSupportedCapabilities();
		string[] GetKeyDescriptions(VirtualDeviceCapability capability);
		object[] GetKeyHandles(VirtualDeviceCapability capability);
		int GetKeyNo(VirtualDeviceCapability capability);
		bool SupportsAccess(VirtualDeviceAccess mode);
	}
}
