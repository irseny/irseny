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
				"Display",
				"CameraDisplay",
				"WebcamDisplay",
				"Control",
				"CameraControl",
				"WebcamControl",
				"TrackingControl",
				"CapTrackingControl",
				"TrackingDisplay",
				"CapTrackingDisplay",
				"BindingsDisplay",
				"CapBindingsDisplay",
				"OutputControl",
				"OutputDeviceConfig",
				"VirtualKeyboardConfig",
				"OutputDeviceBindings",
				"VirtualKeyboardBindings",
				"OutputDeviceSignalBinding"


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

