using System;
using System.Xml;
using System.Collections.Generic;
using Irseny.Inco.Device;

namespace Irseny.Content.Profile {
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
