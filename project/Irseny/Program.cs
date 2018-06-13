using System;
using System.IO;

namespace Irseny {
	class MainClass {
		public static void Main(string[] args) {
			string[] resourceLocations = {
				"resources",
				@"..\resources",
				@"..\..\resources",
				@"..\..\..\resources"
			};
			string resourceFolder = null;
			foreach (string path in resourceLocations) {
				string filePath = Path.Combine(Path.GetFullPath(path), "Resources.root");
				if (File.Exists(filePath)) {
					resourceFolder = Path.GetFullPath(path);
					Console.WriteLine("found resource folder at: " + resourceFolder);
				}
			}
			if (resourceFolder == null) {
				throw new FileNotFoundException("resource folder not available");
			}
			/*string mainFile = Path.Combine(resourceFolder, "gtk/Main.glade");
			string logFile = Path.Combine(resourceFolder, "gtk/Log.glade");
			var logFactory = Mycena.InterfaceFactory.CreateFromFile(logFile);
			var mainFactory = Mycena.InterfaceFactory.CreateFromFile(mainFile);

			Gtk.Application.Init();
			var mainContainer = mainFactory.CreateWidget("wn_Main");
			var logContainer = logFactory.CreateWidget("pnl_Root");
			var imageLogSplitter = mainContainer.GetWidget<Gtk.Paned>("sp_ImageLog");
			var logPanel = logContainer.GetWidget<Gtk.Widget>("pnl_Root");
			imageLogSplitter.Pack2(logPanel, true, true);

			var window = mainContainer.GetWidget<Gtk.Window>("wn_Main");*/
			string ccFile = Path.Combine(resourceFolder, "gtk/CameraControl.glade");
			var ccFactory = Mycena.InterfaceFactory.CreateFromFile(ccFile);
			Gtk.Application.Init();
			var ccContainer = ccFactory.CreateWidget("win_Main");
			var window = ccContainer.GetWidget<Gtk.Window>("win_Main");

			/*string logFile = Path.Combine(resourceFolder, "gtk/Log.glade");
			var logFactory = Mycena.InterfaceFactory.CreateFromFile(logFile);
			Gtk.Application.Init();
			var logContainer = logFactory.CreateWidget("win_Root");
			var window = logContainer.GetWidget<Gtk.Window>("win_Root");*/
			window.Resize(800, 600);
			window.ShowAll();
			window.DeleteEvent += delegate {
				Gtk.Application.Quit();
			};
			Gtk.Application.Run();
		}
	}
}
