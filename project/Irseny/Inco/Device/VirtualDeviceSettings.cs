using System;
namespace Irseny.Inco.Device {
	public class VirtualDeviceSettings {
		public VirtualDeviceType DeviceType { get; set; }
		public int DeviceId { get; set; }
		public int SubdeviceIndex { get; set; }
		public VirtualDeviceSendPolicy SendPolicy { get; set; }
		public int SendRate { get; set; }
		public int ButtonNo { get; set; }
		public int AxisNo { get; set; }
		public string Name {
			get {
				switch (DeviceType) {
				case VirtualDeviceType.Keyboard:
					return "Key" + SubdeviceIndex;
				case VirtualDeviceType.Mouse:
					return "Mouse" + SubdeviceIndex;
				case VirtualDeviceType.Joystick:
					return "Joy" + SubdeviceIndex;
				case VirtualDeviceType.TrackingInterface:
					return "Track" + SubdeviceIndex;
				default:
					return "HID" + SubdeviceIndex;
				}
			}
		}
		public VirtualDeviceSettings() {
			DeviceId = 0;
			SubdeviceIndex = 0;
			DeviceType = VirtualDeviceType.Keyboard;
			SendPolicy = VirtualDeviceSendPolicy.AfterModification;
			SendRate = 60;
			ButtonNo = 12;
			AxisNo = 4;
		}
		public VirtualDeviceSettings(VirtualDeviceSettings settings) {
			if (settings == null) throw new ArgumentNullException("settings");
			this.DeviceId = settings.DeviceId;
			this.SubdeviceIndex = settings.SubdeviceIndex;
			this.DeviceType = settings.DeviceType;
			this.SendPolicy = settings.SendPolicy;
			this.SendRate = settings.SendRate;
			this.ButtonNo = settings.ButtonNo;
			this.AxisNo = settings.AxisNo;
		}
	}
}
