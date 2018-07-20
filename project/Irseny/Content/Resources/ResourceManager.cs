using System;
using System.IO;

namespace Irseny.Content.Resources {
	public class ResourceManager : ContentManager {

		public ResourceManager() : base() {
			InterfaceFactory = new InterfaceFactoryManager();
			InterfaceStock = new InterfaceStockManager();
		}
		public InterfaceFactoryManager InterfaceFactory { get; private set; }
		public InterfaceStockManager InterfaceStock { get; private set; }

		public override void Load(ContentManagerSettings settings) {
			Settings = settings;
			{
				var stockSettings = new ContentManagerSettings(settings);
				stockSettings.SetResourcePaths(null, Path.Combine(settings.ResourceDirectory, "icons"), "(no-file)");
				InterfaceStock.Load(stockSettings);
			}
			{ // depends on stock
				var factorySettings = new ContentManagerSettings(settings);
				factorySettings.SetResourcePaths(null, Path.Combine(settings.ResourceDirectory, "gtk"), "(no-file)");
				InterfaceFactory.Load(factorySettings);
			}
			Loaded = true;
		}
		public override void Reload() {
			InterfaceStock.Reload();
			InterfaceFactory.Reload();
		}
		public override void Save() {
			// nothing to do
		}
		public override void Unload() {
			InterfaceFactory.Unload();
			InterfaceStock.Unload();
		}
	}
}

