using System;
using System.Xml;
using System.Collections.Generic;
using Irseny.Inco.Device;
using Irseny.Tracking;
namespace Irseny.Content.Profile {
	public class BindingsProfileWriter {
		public BindingsProfileWriter() {
		}
		public bool Write(SetupProfile profile, XmlNode root, XmlDocument document) {
			if (profile == null) throw new ArgumentNullException("profile");
			if (root == null) throw new ArgumentNullException("root");
			if (document == null) throw new ArgumentNullException("document");
			foreach (int index in profile.BindingIndexes) {
				CapInputRelay settings = profile.GetBindings(index);
				XmlNode node = WriteRelay(index, settings, document);
				if (node != null) {
					root.AppendChild(node);
				}
			}
			return true;
		}
		private XmlNode WriteRelay(int index, CapInputRelay settings, XmlDocument document) {
			var result = document.CreateElement("Bindings");
			result.SetAttribute("Index", index.ToString());
			foreach (CapAxis axis in (CapAxis[])Enum.GetValues(typeof(CapAxis))) {
				int device = settings.GetDeviceIndex(axis);
				if (device < 0) {
					continue;
				}
				XmlElement node = document.CreateElement("Bind");
				result.AppendChild(node);
				node.SetAttribute("Axis", axis.ToString());
				node.SetAttribute("Index", device.ToString());
				VirtualDeviceCapability capability = settings.GetDeviceCapability(axis);
				node.SetAttribute("Capability", capability.ToString());
				Tuple<object, object> keys = settings.GetDeviceKeys(axis);
				node.SetAttribute("Key1", keys.Item1.ToString());
				node.SetAttribute("Key2", keys.Item2.ToString());

			}
			return result;
		}
	}
}
