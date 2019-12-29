using System;
namespace Irseny.Core.Inco.Device {
	public enum VirtualDeviceAccess {
		None = 0b00,
		Read = 0b01,
		Write = 0b10,
		ReadWrite = 0b11
	}
}
