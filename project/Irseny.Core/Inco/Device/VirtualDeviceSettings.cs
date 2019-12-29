using System;
namespace Irseny.Core.Inco.Device {
	public class VirtualDeviceSettings {
		public VirtualDeviceType DeviceType { get; set; }
		public int DeviceId { get; set; }
		public int ClassifiedDeviceIndex { get; set; }
		public VirtualDeviceSendPolicy SendPolicy { get; set; }
		public int SendRate { get; set; }
		public int ButtonNo { get; set; }
		public int AxisNo { get; set; }
		public string Name {
			get {
				switch (DeviceType) {
				case VirtualDeviceType.Keyboard:
					return "Key" + ClassifiedDeviceIndex;
				case VirtualDeviceType.Mouse:
					return "Mouse" + ClassifiedDeviceIndex;
				case VirtualDeviceType.Joystick:
					return "Joy" + ClassifiedDeviceIndex;
				case VirtualDeviceType.TrackingInterface:
					return "Track" + ClassifiedDeviceIndex;
				default:
					return "HID" + ClassifiedDeviceIndex;
				}
			}
		}
		public VirtualDeviceSettings() {
			DeviceId = 0;
			ClassifiedDeviceIndex = 0;
			DeviceType = VirtualDeviceType.Keyboard;
			SendPolicy = VirtualDeviceSendPolicy.AfterModification;
			SendRate = 60;
			ButtonNo = 12;
			AxisNo = 4;
		}
		public VirtualDeviceSettings(VirtualDeviceSettings settings) {
			if (settings == null) throw new ArgumentNullException("settings");
			this.DeviceId = settings.DeviceId;
			this.ClassifiedDeviceIndex = settings.ClassifiedDeviceIndex;
			this.DeviceType = settings.DeviceType;
			this.SendPolicy = settings.SendPolicy;
			this.SendRate = settings.SendRate;
			this.ButtonNo = settings.ButtonNo;
			this.AxisNo = settings.AxisNo;
		}
	}
}
