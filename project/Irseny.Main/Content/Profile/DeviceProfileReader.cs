using System;
using System.Xml;
using Irseny.Core.Inco.Device;
using Irseny.Core.Util;

namespace Irseny.Main.Content.Profile {
	public class DeviceProfileReader {
		public DeviceProfileReader() {
		}
		public bool Read(SetupProfile result, XmlNode root) {
			foreach (XmlNode node in root.ChildNodes) {
				VirtualDeviceType deviceType;
				if (!Enum.TryParse(node.Name, out deviceType)) {
					continue;
				}

				if (node.Attributes["CommonIndex"] == null || node.Attributes["ClassifiedIndex"] == null) {
					return false;
				}
				int commonIndex = JsonString.ParseInt(node.Attributes["CommonIndex"].InnerText, -1);
				int classifiedIndex = JsonString.ParseInt(node.Attributes["ClassifiedIndex"].InnerText, -1);
				if (commonIndex < 0 || classifiedIndex < 0) {
					return false;
				}
				var settings = new VirtualDeviceSettings() {
					DeviceType = deviceType,
					ClassifiedDeviceIndex = classifiedIndex
				};
				settings = ReadDevice(settings, node);
				if (settings == null) {
					return false;
				}
				result.AddDevice(commonIndex, settings);
			}
			return true;
		}
		private VirtualDeviceSettings ReadDevice(VirtualDeviceSettings settings, XmlNode root) {

			foreach (XmlNode node in root.ChildNodes) {
				if (node.Name.Equals("DeviceId")) {
					int deviceId = JsonString.ParseInt(node.InnerText, -1);
					if (deviceId < 0) {
						return null;
					}
					settings.DeviceId = deviceId;
				} else if (node.Name.Equals("SendPolicy")) {
					VirtualDeviceSendPolicy policy;
					if (!Enum.TryParse(node.InnerText, out policy)) {
						return null;
					}
					settings.SendPolicy = policy;
				} else if (node.Name.Equals("SendRate")) {
					int sendRate = JsonString.ParseInt(node.InnerText, -1);
					if (sendRate < 0) {
						return null;
					}
					settings.SendRate = sendRate;
				} else if (node.Name.Equals("ButtonNo")) {
					int buttonNo = JsonString.ParseInt(node.InnerText, -1);
					if (buttonNo < 0) {
						return null;
					}
					settings.ButtonNo = buttonNo;
				} else if (node.Name.Equals("AxisNo")) {
					int axisNo = JsonString.ParseInt(node.InnerText, -1);
					if (axisNo < 0) {
						return null;
					}
					settings.AxisNo = axisNo;
				}
			}
			return settings;
		}
	}
}
