using System;
namespace Irseny.Inco.Device {
	public interface IVirtualDevice {
		VirtualDeviceType DeviceType { get; }
		VirtualDeviceSendPolicy SendPolicy { get; set; }
		int SendInterval { get; set; }
		bool SendRequired { get; }
		int DeviceIndex { get; }
		VirtualDeviceCapability[] GetSupportedCapabilities();
		string[] GetKeyDescriptions(VirtualDeviceCapability capability);
		object[] GetKeyHandles(VirtualDeviceCapability capability);
		int GetKeyNo(VirtualDeviceCapability capability);
		void BeginUpdate();
		bool SetKeyState(VirtualDeviceCapability capability, object keyHandle, float state);
		void EndUpdate();
	}
}
