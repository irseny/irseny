using System;
namespace Irseny.Inco.Device {
	public interface IVirtualDevice {
		VirtualDeviceType DeviceType { get; }
		VirtualDeviceSendPolicy SendPolicy { get; set; }
		int SendRate { get; set; }
		bool SendRequired { get; }
		int DeviceIndex { get; }
		VirtualDeviceCapability[] GetSupportedCapabilities();
		object[] GetKeyHandles(VirtualDeviceCapability capability);
		int GetKeyNo(VirtualDeviceCapability capability);
		void BeginUpdate();
		bool SetKeyState(VirtualDeviceCapability capability, object keyHandle, KeyState state);
		KeyState GetKeyState(VirtualDeviceCapability capability, object keyHandle);
		object[] GetModifiedKeys(VirtualDeviceCapability capability);
		void EndUpdate();
		void Send();
	}
}
