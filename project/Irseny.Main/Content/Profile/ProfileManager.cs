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
using System.IO;
using System.Xml;

namespace Irseny.Main.Content.Profile {
	public class ProfileManager : ContentManager {
		ContentManagerSettings settings = null;

		public ProfileManager() {
		}

		public override void Load(ContentManagerSettings settings) {
			this.settings = settings;
			Loaded = true;
		}
		public override void Reload() {

		}
		public override void Unload() {

			Loaded = false;
		}
		public void SafeActiveProfile(SetupProfile profile) {
			if (profile == null) throw new ArgumentNullException("profile");
			if (!Loaded) throw new InvalidOperationException("not loaded");
			string filePath = Path.Combine(settings.ResourceDirectory, "profiles", profile.Name + ".xml");

			XmlDocument document = new XmlDocument();
			XmlDeclaration decl = document.CreateXmlDeclaration("1.0", "UTF-8", null);
			document.InsertBefore(decl, document.DocumentElement);
			XmlElement root = document.CreateElement("Profile");
			document.AppendChild(root);

			XmlElement capture = document.CreateElement("VideoCapture");
			root.AppendChild(capture);
			new CaptureProfileWriter().Write(profile, capture, document);
			XmlElement tracking = document.CreateElement("Tracking");
			root.AppendChild(tracking);
			new TrackingProfileWriter().Write(profile, tracking, document);
			XmlElement devices = document.CreateElement("VirtualDevices");
			root.AppendChild(devices);
			new DeviceProfileWriter().Write(profile, devices, document);
			XmlElement bindings = document.CreateElement("TrackerBindings");
			root.AppendChild(bindings);
			new BindingsProfileWriter().Write(profile, bindings, document);
			using (FileStream stream = File.Open(filePath, FileMode.Create, FileAccess.Write)) {
				document.Save(stream);
			}

		}
		public SetupProfile LoadDefaultProfile() {
			if (!Loaded) throw new InvalidOperationException("not loaded");
			string filePath = Path.Combine(settings.ResourceDirectory, "profiles", "Default.xml");
			var document = new XmlDocument();
			try {
				using (FileStream stream = File.Open(filePath, FileMode.Open, FileAccess.Read)) {
					document.Load(stream);
				}
			} catch (FileNotFoundException) {
				return null;
			} catch (System.Reflection.TargetInvocationException) {
				return null;
			}
			var result = new SetupProfile("Default");
			foreach (XmlNode root in document.DocumentElement.ChildNodes) {
				if (root.Name.Equals("VideoCapture")) {
					if (!new CaptureProfileReader().Read(result, root)) {
						return null;
					}
				} else if (root.Name.Equals("VirtualDevices")) {
					if (!new DeviceProfileReader().Read(result, root)) {
						return null;
					}
				} else if (root.Name.Equals("Tracking")) {
					if (!new TrackingProfileReader().Read(result, root)) {
						return null;
					}
				} else if (root.Name.Equals("TrackerBindings")) {
					if (!new BindingsProfileReader().Read(result, root)) {
						return null;
					}
				}
			}
			return result;
		}
	}
}
