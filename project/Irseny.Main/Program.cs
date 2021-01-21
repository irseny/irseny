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
using System.Net;
using System.Text;
using Irseny.Core.Util;
using Irseny.Core.Log;
using Irseny.Core.Sensors;
using Irseny.Core.Tracking;
using Irseny.Core.Inco.Device;
using Irseny.Core.Listing;

using Irseny.Main.Content;
using Irseny.Main.Webface;

namespace Irseny.Main {
	public static class Program {
		public static void Main(string[] args) {
			{ // start main systems
				LogManager.MakeInstance(new LogManager());
				CaptureSystem.MakeInstance(new CaptureSystem());
				TrackingSystem.MakeInstance(new TrackingSystem());
				VirtualDeviceManager.MakeInstance(new VirtualDeviceManager());
			}
			{ // prepare content managers
				ContentMaster.MakeInstance(new ContentMaster());
				var contentSettings = new ContentManagerSettings();
				string resourceRoot = ContentMaster.FindResourceRoot();
				contentSettings.SetResourcePaths(resourceRoot, resourceRoot, "(no-file)");
				string userRoot = ContentMaster.FindConfigRoot();
				contentSettings.SetConfigPaths(userRoot, userRoot, "(no-file)");
				ContentMaster.Instance.Load(contentSettings);
			}
			{ // load last configuration
				new Emgu.CV.Mat();
				var profile = ContentMaster.Instance.Profiles.LoadDefaultProfile();
				new ProfileActivator().ActivateProfile(profile).Wait();
			}
			{ // start webserver
				var server = new Webface.WebfaceServer();
				server.Start();
				Console.ReadLine();
				server.Stop();
			}
			{ // cleanup main systems
				VirtualDeviceManager.MakeInstance(null);
				TrackingSystem.MakeInstance(null);
				CaptureSystem.MakeInstance(null);
				LogManager.MakeInstance(null);
				EquipmentMaster.MakeInstance(null);
			}
		}
	}
}
