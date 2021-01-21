// This file is part of Irseny.
//
// Copyright (C) 2021  Thilo Gabel
//
// Irseny is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
//
// Irseny is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
//
// You should have received a copy of the GNU General Public License
// along with this program.  If not, see <https://www.gnu.org/licenses/>.

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
