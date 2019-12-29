using System;
using System.Collections.Generic;
using Irseny.Inco.Device;

namespace Irseny.Iface.Main.Config.Devices {
	interface IClassifiedDeviceConfigFactory : IInterfaceFactory {
		int CommonDeviceIndex { get; }
		int ClassifiedDeviceIndex { get; }
		VirtualDeviceType DeviceType { get; }
		VirtualDeviceSettings GetSettings();
	}
}
