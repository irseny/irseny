﻿using System;
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

