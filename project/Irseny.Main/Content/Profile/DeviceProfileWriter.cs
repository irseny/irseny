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
using System.Xml;
using System.Collections.Generic;
using Irseny.Core.Inco.Device;

namespace Irseny.Main.Content.Profile {
	public class DeviceProfileWriter {
		public DeviceProfileWriter() {
		}
		public bool Write(SetupProfile profile, XmlNode root, XmlDocument target) {
			if (profile == null) throw new ArgumentNullException("profile");
			if (target == null) throw new ArgumentNullException("target");
			if (root == null) throw new ArgumentNullException("root");
			var indexes = new List<int>(profile.VideoCaptureIndexes);
			foreach (int i in profile.DeviceIndexes) {
				VirtualDeviceSettings settings = profile.GetDevice(i);
				XmlNode node = WriteDevice(i, settings, target);
				if (node != null) {
					root.AppendChild(node);
				}
			}
			return true;
		}
		private XmlNode WriteDevice(int index, VirtualDeviceSettings settings, XmlDocument target) {
			XmlElement result = target.CreateElement(settings.DeviceType.ToString());
			result.SetAttribute("CommonIndex", index.ToString());
			result.SetAttribute("ClassifiedIndex", settings.ClassifiedDeviceIndex.ToString());
			{ // device id
				XmlElement deviceId = target.CreateElement("DeviceId");
				result.AppendChild(deviceId);
				deviceId.InnerText = settings.DeviceId.ToString();
			}
			{ // send policy
				XmlElement sendPolicy = target.CreateElement("SendPolicy");
				result.AppendChild(sendPolicy);
				sendPolicy.InnerText = settings.SendPolicy.ToString();
			}
			// send rate
			if (settings.SendPolicy != VirtualDeviceSendPolicy.AfterModification) {
				XmlElement sendRate = target.CreateElement("SendRate");
				result.AppendChild(sendRate);
				sendRate.InnerText = settings.SendRate.ToString();
			}
			// axis and button count
			if (settings.DeviceType == VirtualDeviceType.Joystick) {
				XmlElement buttonNo = target.CreateElement("ButtonNo");
				result.AppendChild(buttonNo);
				buttonNo.InnerText = settings.ButtonNo.ToString();
				XmlElement axisNo = target.CreateElement("AxisNo");
				result.AppendChild(axisNo);
				axisNo.InnerText = settings.AxisNo.ToString();
			}
			return result;
		}
	}
}
