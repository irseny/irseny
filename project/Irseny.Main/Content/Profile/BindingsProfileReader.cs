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
using Irseny.Core.Tracking;
using Irseny.Core.Tracking.HeadTracking;
using Irseny.Core.Inco.Device;
using Irseny.Core.Util;

namespace Irseny.Main.Content.Profile {
	public class BindingsProfileReader {
		public BindingsProfileReader() {
		}
		public bool Read(SetupProfile profile, XmlNode root) {
			if (profile == null) throw new ArgumentNullException("profile");
			if (root == null) throw new ArgumentNullException("root");
			foreach (XmlNode node in root.ChildNodes) {
				if (node.Name.Equals("Bindings")) {
					if (node.Attributes["Index"] == null) {
						return false;
					}
					int index = JsonString.ParseInt(node.Attributes["Index"].InnerText, -1);
					if (index < 0) {
						return false;
					}
					CapInputRelay settings = ReadRelay(node);
					if (settings == null) {
						return false;
					}
					profile.AddBindings(index, settings);
				}
			}
			return true;
		}
		private CapInputRelay ReadRelay(XmlNode root) {
			var result = new CapInputRelay();
			foreach (XmlNode node in root.ChildNodes) {
				if (node.Name.Equals("Bind")) {
					// read information for a single axis
					if (node.Attributes["Axis"] == null || node.Attributes["Index"] == null || node.Attributes["Capability"] == null) {
						return null;
					}
					HeadAxis axis;
					if (!Enum.TryParse(node.Attributes["Axis"].InnerText, out axis)) {
						return null;
					}
					int device = JsonString.ParseInt(node.Attributes["Index"].InnerText, -1);
					if (device < 0) {
						return null;
					}
					VirtualDeviceCapability capability;
					if (!Enum.TryParse(node.Attributes["Capability"].InnerText, out capability)) {
						return null;
					}
					object key1 = null;
					if (node.Attributes["Key1"] != null) {
						key1 = node.Attributes["Key1"].InnerText;
					}
					object key2 = null;
					if (node.Attributes["Key2"] != null) {
						key2 = node.Attributes["Key2"].InnerText;
					}
					object mapping = null;
					result.AddBinding(axis, device, capability, key1, key2, mapping);
				}
			}
			return result;
		}
	}
}
