using System;
using System.IO;

namespace Irseny.Content {
	public class InterfaceDefinitionManager : Manager<string> {
		public InterfaceDefinitionManager() : base() {
		}
		public override void Load(ContentManagerSettings settings) {
			SetEntry("Main", Path.Combine(settings.ResourceDirectory, "Main.glade"));
			SetEntry("Log", Path.Combine(settings.ResourceDirectory, "Log.glade"));
			SetEntry("Image", Path.Combine(settings.ResourceDirectory, "Image.glade"));
			SetEntry("Control", Path.Combine(settings.ResourceDirectory, "Control.glade"));
			SetEntry("CameraControl", Path.Combine(settings.ResourceDirectory, "CameraControl.glade"));
			SetEntry("InnerCameraControl", Path.Combine(settings.ResourceDirectory, "InnerCameraControl.glade"));
		}
		public override void Reload() {
			
		}
		public override void Unload() {
			ClearEntries();
		}
	}
}

