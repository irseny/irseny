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
using Irseny.Main.Content.Profile;

namespace Irseny.Main.Content {
	public class ContentMaster : ContentManager {
		public ContentMaster() : base() {

			Profiles = new ProfileManager();
		}
		public static ContentMaster Instance { get; private set; }


		public ProfileManager Profiles { get; protected set; }

		public override void Load(ContentManagerSettings settings) {
			Settings = settings;

			Profiles.Load(new ContentManagerSettings(settings));
		}
		public override void Reload() {

			Profiles.Reload();
		}
		public override void Unload() {

			Profiles.Unload();
		}



		public static void MakeInstance(ContentMaster instance) {
			if (instance == null) throw new ArgumentNullException("instance");
			Instance = instance;
		}
		public static string FindResourceRoot() {
			string[] resourceLocations = {
				"resources",
				"../resources",
				"../../resources",
				"../../../resources"
			};
			foreach (string path in resourceLocations) {
				string filePath = Path.Combine(path, ".resroot");
				if (File.Exists(filePath)) {
					return Path.GetFullPath(path);
				}
			}
			return null;
		}
		public static string FindConfigRoot() {
			return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Irseny");
		}
	}
}

