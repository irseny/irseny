using System;
using System.IO;

namespace Irseny.Content.Resources {
	public class ResourceManager : ContentManager {
		
		public ResourceManager() : base() {
			InterfaceDefinitions = new InterfaceDefinitionManager();
		}
		public InterfaceDefinitionManager InterfaceDefinitions { get; private set; }
		public override void Load(ContentManagerSettings settings) {
			Settings = settings;
			var iDefSettings = new ContentManagerSettings(settings);
			iDefSettings.SetResourcePaths(null, Path.Combine(settings.ResourceDirectory, "gtk"), "(no-file)");
			InterfaceDefinitions.Load(iDefSettings);
			Loaded = true;
		}
		public override void Reload() {
			
		}
		public override void Save() {
			
		}
		public override void Unload() {
			
		}
	}
}

