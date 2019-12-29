using System;

namespace Irseny.Content {
	public class ContentManagerSettings {
		public ContentManagerSettings() {
			ResourceRoot = string.Empty;
			ResourceDirectory = string.Empty;
			ResourceFile = string.Empty;
			ConfigRoot = string.Empty;
			ConfigFile = string.Empty;
			ConfigDirectory = string.Empty;
		}
		public ContentManagerSettings(ContentManagerSettings source) {
			if (source == null) throw new ArgumentNullException("source");
			this.ResourceRoot = source.ResourceRoot;
			this.ResourceFile = source.ResourceFile;
			this.ResourceDirectory = source.ResourceDirectory;
			this.ConfigRoot = source.ConfigRoot;
			this.ConfigFile = source.ConfigFile;
			this.ConfigDirectory = source.ConfigDirectory;
		}

		public string ResourceRoot { get; set; }
		public string ResourceDirectory { get; set; }
		public string ResourceFile { get; set; }
		public string ConfigRoot { get; private set; }
		public string ConfigFile { get; set; }
		public string ConfigDirectory { get; set; }

		public void SetResourcePaths(string root, string directory, string file) {
			if (root != null) {
				ResourceRoot = root;
			}
			if (directory != null) {
				ResourceDirectory = directory;
			}
			if (file != null) {
				ResourceFile = file;
			}

		}
		public void SetConfigPaths(string root, string directory, string file) {
			if (root != null) {
				ConfigRoot = root;
			}
			if (directory != null) {
				ConfigDirectory = directory;
			}
			if (file != null) {
				ConfigFile = file;
			}
		}

	}
}

