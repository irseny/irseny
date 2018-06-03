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
			string exampleFile = Path.Combine(resourceFolder, "gtk/example2.glade");
			var factory = Mycena.InterfaceFactory.CreateFromFile(exampleFile);
			Gtk.Application.Init();
			var container = factory.CreateWidget("window1");
			var window = container.GetWidget<Gtk.Window>("window1");
			window.Resize(800, 600);

			var l = new Gtk.Label("text");
			window.Add(l);
			window.ShowAll();
			window.DeleteEvent += delegate {
				Gtk.Application.Quit();
			};
			Gtk.Application.Run();
		}
	}
}
