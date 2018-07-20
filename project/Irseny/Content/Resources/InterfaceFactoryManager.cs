using System;
using System.IO;

namespace Irseny.Content.Resources {
	public class InterfaceFactoryManager : ContentManager<Mycena.InterfaceFactory> {
		public InterfaceFactoryManager() : base() {
		}
		public override void Load(ContentManagerSettings settings) {
			string[] entryNames = new string[] {
				"Main",
				"Log",
				"Output",
				"CameraOutputBase",
				"CameraOutput",
				"Control",
				"CameraControlBase",
				"CameraControl",
				"TrackingControlBase",
				"TrackingControl",
				"TrackingOutputBase",
				"TrackingOutput"
			};
			for (int i = 0; i < entryNames.Length; i++) {
				var stock = ContentMaster.Instance.Resources.InterfaceStock.GetEntry("Main"); // load this first
				string filePath = Path.Combine(settings.ResourceDirectory, entryNames[i] + ".glade");
				var factory = Mycena.InterfaceFactory.CreateFromFile(filePath, stock);
				SetEntry(entryNames[i], factory);
			}
			Loaded = true;
		}
		public override void Reload() {

		}
		public override void Unload() {
			ClearEntries();
			Loaded = false;
		}
	}
}

