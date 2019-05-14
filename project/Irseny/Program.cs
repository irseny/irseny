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
				Inco.Device.VirtualDeviceManager.MakeInstance(new Inco.Device.VirtualDeviceManager());
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
				var mainFactory = new Iface.MainFactory();
				var logFactory = new Iface.Main.Log.MainFactory();
				var controlFactory = new Iface.Main.Config.ConfigFactory();
				var displayFactory = new Irseny.Iface.Main.View.ViewFactory();
				mainFactory.ConstructFloor("Log", logFactory);
				mainFactory.ConstructFloor("Control", controlFactory);
				mainFactory.ConstructFloor("Display", displayFactory);
				{ // control
					var cameraControlFactory = new Iface.Main.Config.Camera.CameraFactory();
					controlFactory.ConstructFloor("Camera", cameraControlFactory);
					var trackingControlFactory = new Iface.Main.Config.Tracking.TrackingFactory();
					controlFactory.ConstructFloor("Tracking", trackingControlFactory);
					var outputControlFactory = new Iface.Main.Config.Devices.DeviceConfigFactory();
					controlFactory.ConstructFloor("Output", outputControlFactory);
					/*{
						var deviceControlFactory = new Iface.Main.Control.Output.OutputDeviceConfigFactory();
						outputControlFactory.ConstructFloor("Device", deviceControlFactory);
						var assignmentControlFactory = new Iface.Main.Control.Output.OutputDeviceBindingsFactory();
						outputControlFactory.ConstructFloor("Assignment", assignmentControlFactory);
					}*/
				}
				{ // display
					var cameraDisplayFactory = new Irseny.Iface.Main.View.Camera.CameraFactory();
					displayFactory.ConstructFloor("Camera", cameraDisplayFactory);
					var trackingDisplayFactory = new Irseny.Iface.Main.View.Tracking.TrackingFactory();
					displayFactory.ConstructFloor("Tracking", trackingDisplayFactory);
					var bindingsDisplayFactory = new Irseny.Iface.Main.View.Bindings.BindingsFactory();
					displayFactory.ConstructFloor("Bindings", bindingsDisplayFactory);
				}

				if (!mainFactory.Init(Iface.InterfaceFactoryState.Connected)) {
					Debug.WriteLine("main factory initialization failed");
					return;
				}
				var window = mainFactory.Container.GetWidget<Gtk.Window>("win_Main");

				window.ShowAll();
				window.DeleteEvent += delegate {
					stopped = true;
					// TODO: remove all new display elements to prevent invinite marshaling
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
				Inco.Device.VirtualDeviceManager.MakeInstance(null);
				Tracap.DetectionSystem.MakeInstance(null);
				Capture.Video.CaptureSystem.MakeInstance(null);
				LogManager.MakeInstance(null);
				EquipmentMaster.MakeInstance(null);
			}
		}
	}
}
