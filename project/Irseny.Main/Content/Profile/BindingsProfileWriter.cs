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
using Irseny.Core.Tracking;

namespace Irseny.Main.Content.Profile {
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
