using System;

using System.IO;
using System.Threading;
using System.Diagnostics;
using Irseny.Listing;
using Irseny.Log;
using Irseny.Content;

namespace Irseny {
	class MainClass {
		public static void Main(string[] args) {
			Gtk.Application.Init();
#if WINDOWS
			Gtk.Settings.Default.SetLongProperty("gtk-button-images", 1, string.Empty);
#endif
			{
				EquipmentMaster.MakeInstance(new EquipmentMaster());
				LogManager.MakeInstance(new LogManager());
				Capture.Video.CaptureSystem.MakeInstance(new Capture.Video.CaptureSystem());
				Tracap.DetectionSystem.MakeInstance(new Tracap.DetectionSystem());
			}
			{
				ContentMaster.MakeInstance(new Content.ContentMaster());
				var contentSettings = new ContentManagerSettings();
				string resourceRoot = ContentMaster.FindResourceRoot();
				contentSettings.SetResourcePaths(resourceRoot, resourceRoot, "(no-file)");
				string userRoot = ContentMaster.FindConfigRoot();
				contentSettings.SetConfigPaths(userRoot, userRoot, "(no-file)");
				ContentMaster.Instance.Load(contentSettings);
			}
			bool stopped = false;
			{
				var mainFactory = new Viol.MainFactory();
				var logFactory = new Viol.Main.Log.MainFactory();
				var controlFactory = new Viol.Main.Control.ControlFactory();
				var imageFactory = new Irseny.Viol.Main.Display.DisplayFactory();
				mainFactory.ConstructFloor("Log", logFactory);
				mainFactory.ConstructFloor("Control", controlFactory);
				mainFactory.ConstructFloor("Output", imageFactory);
				var cameraControlFactory = new Viol.Main.Control.Camera.CameraFactory();
				controlFactory.ConstructFloor("Camera", cameraControlFactory);
				var trackingControlFactory = new Viol.Main.Control.Tracking.TrackingFactory();
				controlFactory.ConstructFloor("Tracking", trackingControlFactory);
				var cameraImageFactory = new Irseny.Viol.Main.Display.Camera.CameraFactory();
				imageFactory.ConstructFloor("Camera", cameraImageFactory);
				var trackingImageFactory = new Irseny.Viol.Main.Display.Tracking.TrackingFactory();
				imageFactory.ConstructFloor("Tracking", trackingImageFactory);

				if (!mainFactory.Init(Viol.InterfaceFactoryState.Connected)) {
					Debug.WriteLine("main factory initialization failed");
					return;
				}
				var window = mainFactory.Container.GetWidget<Gtk.Window>("win_Main");

				window.Resize(800, 600);
				window.ShowAll();
				window.DeleteEvent += delegate {
					stopped = true;
				};
				GLib.ExceptionManager.UnhandledException += (GLib.UnhandledExceptionArgs e) => {
					LogManager.Instance.Log(LogMessage.CreateError(e.ExceptionObject, e.ExceptionObject.ToString()));
					e.ExitApplication = false;
				};

			}
			/*IntPtr context = Extrack.Artf.CreateContext();
			IntPtr device = Extrack.Artf.OpenDevice(context, 0);
			IntPtr packet = Extrack.Artf.AllocatePacket();
			Extrack.Artf.SetPacketCameraSize(packet, 320, 240);
			Stopwatch watch = new Stopwatch();
			watch.Start();*/
			Gtk.Application.RunIteration();
			while (!stopped) {
				/*Extrack.Artf.IncreasePacketId(packet);
				float yaw = (float)Math.Sin(watch.ElapsedMilliseconds * 0.0002f);
				if (!Extrack.Artf.SetPacketProperty(packet, Extrack.Artf.PacketProperty.Yaw, yaw)) {
					Debug.WriteLine("property not set successfully");
				}
				Extrack.Artf.SetPacketProperty(packet, Extrack.Artf.PacketProperty.RawYaw, yaw);
				if (!Extrack.Artf.SubmitPacket(context, device, packet)) {
					Debug.WriteLine("packet not submitted successfully");
				}*/
				// TODO: fix attempted to read or write protected memory through GLib.ToggleRef.Free();
				Gtk.Application.RunIteration();

				long memory = GC.GetTotalMemory(true);
				//Console.WriteLine("total memory used {0:#,##0}k", memory / 1000);
				// occuring exceptions:
				// invalid access to memory when removing a camera page
			}
			/*watch.Stop();
			Extrack.Artf.FreePacket(packet);
			Extrack.Artf.CloseDevice(context, device);
			Extrack.Artf.DestroyContext(context);*/
			//Gtk.Application.Run();
			{
				Tracap.DetectionSystem.MakeInstance(null);
				Capture.Video.CaptureSystem.MakeInstance(null);
				LogManager.MakeInstance(null);
				EquipmentMaster.MakeInstance(null);
			}
		}
	}
}
