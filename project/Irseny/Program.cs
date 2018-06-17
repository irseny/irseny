using System;

using System.IO;
using System.Diagnostics;

namespace Irseny {
	class MainClass {
		public static void Main(string[] args) {
			/*string[] resourceLocations = {
				"resources",
				"../resources",
				"../../resources",
				"../../../resources"
			};
			string resourceFolder = null;
			foreach (string path in resourceLocations) {
				string filePath = Path.Combine(path, "Resources.root");
				if (File.Exists(filePath)) {
					resourceFolder = Path.GetFullPath(path);
					Console.WriteLine("found resource folder at: " + resourceFolder);
					break;
				}
			}
			if (resourceFolder == null) {
				throw new FileNotFoundException("resource folder not available");
			}
			string mainFile = Path.Combine(resourceFolder, "gtk/Main.glade");
			string logFile = Path.Combine(resourceFolder, "gtk/Log.glade");
			string controlFile = Path.Combine(resourceFolder, "gtk/Control.glade");
			string cameraControlFile = Path.Combine(resourceFolder, "gtk/CameraControl.glade");
			var logFactory = Mycena.InterfaceFactory.CreateFromFile(logFile);
			var mainFactory = Mycena.InterfaceFactory.CreateFromFile(mainFile);
			var cameraControlFactory = Mycena.InterfaceFactory.CreateFromFile(cameraControlFile);
			var controlFactory = Mycena.InterfaceFactory.CreateFromFile(controlFile);

			Gtk.Application.Init();
			var mainContainer = mainFactory.CreateWidget("win_Main");
			var logContainer = logFactory.CreateWidget("pnl_Root");
			var controlContainer = controlFactory.CreateWidget("ntb_Control");
			var cameraContontrolContainer = cameraControlFactory.CreateWidget("pnl_Main");

			var pnlCameraControl = cameraContontrolContainer.GetWidget<Gtk.Box>("pnl_Main");
			var ntbControl = controlContainer.GetWidget<Gtk.Notebook>("ntb_Control");
			var pnlLog = logContainer.GetWidget<Gtk.Widget>("pnl_Root");
			var splImageLog = mainContainer.GetWidget<Gtk.Paned>("spl_ImageLog");
			var pnlControlStatus = mainContainer.GetWidget<Gtk.Box>("pnl_ControlStatus");

			pnlControlStatus.PackStart(ntbControl);
			ntbControl.AppendPage(pnlCameraControl, new Gtk.Label("Camera"));
			splImageLog.Pack2(pnlLog, true, true);

			var window = mainContainer.GetWidget<Gtk.Window>("win_Main");
			window.Resize(800, 600);
			window.ShowAll();
			window.DeleteEvent += delegate {
				Gtk.Application.Quit();
			};
			Gtk.Application.Run();*/
			Gtk.Application.Init();
			var mainFactory = new Viol.MainFactory();
			var logFactory = new Viol.Main.LogFactory();
			mainFactory.ConstructFloor("log", logFactory);
			if (!mainFactory.Init(Irseny.Viol.InterfaceFactoryState.Connected)) {
				Debug.WriteLine("main factory initialization failed");
				return;
			}
			var window = mainFactory.Container.GetWidget<Gtk.Window>("win_Main");
			window.Resize(800, 600);
			window.ShowAll();
			Gtk.Application.Run();
		}
	}
}
