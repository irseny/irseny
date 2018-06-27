using System;
using System.IO;

namespace Irseny.Content {
	public class ContentMaster : ContentManager {
		public ContentMaster() : base() {
			Resources = new Resources.ResourceManager();
		}
		public static ContentMaster Instance { get; private set; }

		public Resources.ResourceManager Resources { get; protected set; }

		public override void Load(ContentManagerSettings settings) {
			Settings = settings;
			Resources.Load(new ContentManagerSettings(settings));

		}
		public override void Reload() {
			Resources.Reload();
		}
		public override void Unload() {
			Resources.Unload();
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

