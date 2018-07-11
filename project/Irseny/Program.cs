using System;

using System.IO;
using System.Threading;
using System.Diagnostics;

namespace Irseny {
	class MainClass {
		public static void Main(string[] args) {
			Gtk.Application.Init();
#if WINDOWS
			Gtk.Settings.Default.SetLongProperty("gtk-button-images", 1, "");
#endif
			{
				Listing.EquipmentMaster.MakeInstance(new Listing.EquipmentMaster());
				Log.LogManager.MakeInstance(new Log.LogManager());
				Capture.Video.CaptureSystem.MakeInstance(new Capture.Video.CaptureSystem());
				Tracap.DetectionSystem.MakeInstance(new Tracap.DetectionSystem());
			}
			{
				Content.ContentMaster.MakeInstance(new Content.ContentMaster());
				var contentSettings = new Content.ContentManagerSettings();
				string resourceRoot = Content.ContentMaster.FindResourceRoot();
				contentSettings.SetResourcePaths(resourceRoot, resourceRoot, "(no-file)");
				string userRoot = Content.ContentMaster.FindConfigRoot();
				contentSettings.SetConfigPaths(userRoot, userRoot, "(no-file)");
				Content.ContentMaster.Instance.Load(contentSettings);
			}
			bool stopped = false;
			{
				var mainFactory = new Viol.MainFactory();
				var logFactory = new Viol.Main.Log.MainFactory();
				var controlFactory = new Viol.Main.Control.MainFactory();
				var imageFactory = new Viol.Main.Image.MainFactory();
				mainFactory.ConstructFloor("log", logFactory);
				mainFactory.ConstructFloor("control", controlFactory);
				mainFactory.ConstructFloor("image", imageFactory);
				var cameraControlFactory = new Viol.Main.Control.Camera.CameraBaseFactory();
				controlFactory.ConstructFloor("camera", cameraControlFactory);
				var trackingControlFactory = new Viol.Main.Control.Tracking.TrackingBaseFactory();
				controlFactory.ConstructFloor("tracking", trackingControlFactory);
				var cameraImageFactory = new Viol.Main.Image.Camera.CameraBaseFactory();
				imageFactory.ConstructFloor("camera", cameraImageFactory);
				if (!mainFactory.Init(Irseny.Viol.InterfaceFactoryState.Connected)) {
					Debug.WriteLine("main factory initialization failed");
					return;
				}
				var window = mainFactory.Container.GetWidget<Gtk.Window>("win_Main");

				window.Resize(800, 600);
				window.ShowAll();
				window.DeleteEvent += delegate {
					stopped = true;
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
				Gtk.Application.RunIteration();
			}
			/*watch.Stop();
			Extrack.Artf.FreePacket(packet);
			Extrack.Artf.CloseDevice(context, device);
			Extrack.Artf.DestroyContext(context);*/
			//Gtk.Application.Run();
			{
				Tracap.DetectionSystem.MakeInstance(null);
				Capture.Video.CaptureSystem.MakeInstance(null);
				Log.LogManager.MakeInstance(null);
				Listing.EquipmentMaster.MakeInstance(null);
			}
		}
	}
}
