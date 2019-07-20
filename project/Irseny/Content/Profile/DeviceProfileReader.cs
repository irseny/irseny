using System;
using System.Xml;
using Irseny.Inco.Device;
using Irseny.Util;
namespace Irseny.Content.Profile {
	public class DeviceProfileReader {
		public DeviceProfileReader() {
		}
		public bool Read(SetupProfile result, XmlNode root) {
			foreach (XmlNode node in root.ChildNodes) {
				VirtualDeviceType deviceType;
				if (!Enum.TryParse(node.Name, out deviceType)) {
					continue;
				}

				if (node.Attributes["Index"] == null || node.Attributes["Subindex"] == null) {
					return false;
				}
				int index = TextParseTools.ParseInt(node.Attributes["Index"].InnerText, -1);
				int subindex = TextParseTools.ParseInt(node.Attributes["Subindex"].InnerText, -1);
				if (index < 0 || subindex < 0) {
					return false;
				}
				var settings = new VirtualDeviceSettings() {
					DeviceType = deviceType,
					SubdeviceIndex = subindex
				};
				settings = ReadDevice(settings, node);
				if (settings == null) {
					return false;
				}
				result.AddDevice(index, settings);
			}
			return true;
		}
		private VirtualDeviceSettings ReadDevice(VirtualDeviceSettings settings, XmlNode root) {

			foreach (XmlNode node in root.ChildNodes) {
				if (node.Name.Equals("DeviceId")) {
					int deviceId = TextParseTools.ParseInt(node.InnerText, -1);
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
					int sendRate = TextParseTools.ParseInt(node.InnerText, -1);
					if (sendRate < 0) {
						return null;
					}
					settings.SendRate = sendRate;
				} else if (node.Name.Equals("ButtonNo")) {
					int buttonNo = TextParseTools.ParseInt(node.InnerText, -1);
					if (buttonNo < 0) {
						return null;
					}
					settings.ButtonNo = buttonNo;
				} else if (node.Name.Equals("AxisNo")) {
					int axisNo = TextParseTools.ParseInt(node.InnerText, -1);
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
