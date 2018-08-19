using System;
namespace Irseny.Inco.Device {
	public interface IVirtualDevice {
		VirtualDeviceType DeviceType { get; }
		int ButtonCount { get; }
		int KeyCount { get; }
		int AxisCount { get; }
		string GetButtonDescription(int button);
		string GetKeyDescription(int key);
		string GetAxisDescription(int axis);
	}
}
