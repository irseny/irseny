using System;
using System.IO;

namespace Irseny {
	class MainClass {
		public static void Main(string[] args) {
			string[] resourceLocations = {
				"resources",
				"../resources",
				"../../resources",
				"../../../resources"
			};
			string resourceFolder = null;
			foreach (string path in resourceLocations) {
				string filePath = System.IO.Path.Combine(path, "Resources.root");
				if (System.IO.File.Exists(filePath)) {
					resourceFolder = System.IO.Path.GetFullPath(path);
					System.Console.WriteLine("found resource folder at: " + resourceFolder);
				}
			}
			if (resourceFolder == null) {
				throw new FileNotFoundException("resource folder not available");
			}
			string exampleFile = Path.Combine(resourceFolder, "gtk/Main.glade");
			var factory = Mycena.InterfaceFactory.CreateFromFile(exampleFile);
			Gtk.Application.Init();
			var container = factory.CreateWidget("wn_Main");
			var window = container.GetWidget<Gtk.Window>("wn_Main");
			var imageLogSplitter = container.GetWidget<Gtk.Paned>("sp_ImageLog");
			//imageLogSplitter.Add(new Gtk.Label("text"));
			window.Resize(800, 600);
			window.ShowAll();
			window.DeleteEvent += delegate {
				Gtk.Application.Quit();
			};
			Gtk.Application.Run();
		}
	}
}
