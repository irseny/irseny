using System;
using System.Collections.Generic;
using Irseny.Content;
using Irseny.Content.Profile;
using Irseny.Log;
using Irseny.Capture.Video;
using Irseny.Tracking;
using Irseny.Inco.Device;
using Irseny.Listing;

namespace Irseny.Iface.Main.Config.Profile {
	public class ProfileFactory : InterfaceFactory {
		IInterfaceRegister register;

		public ProfileFactory(IInterfaceRegister register) : base() {
			if (register == null) throw new ArgumentNullException("register");
			this.register = register;
		}

		protected override bool CreateInternal() {
			var factory = ContentMaster.Instance.Resources.InterfaceFactory.GetEntry("ProfileConfig");
			Container = factory.CreateWidget("box_Root");
			return true;
		}
		protected override bool ConnectInternal() {
			var boxRoot = Hall.Container.GetWidget<Gtk.Box>("box_Profile");
			var boxMain = Container.GetWidget("box_Root");
			boxRoot.PackStart(boxMain, true, true, 0);
			{
				var btnSave = Container.GetWidget<Gtk.Button>("btn_Save");
				btnSave.Clicked += delegate {
					SafeActiveProfile();
				};
			}
			{
				var btnLoad = Container.GetWidget<Gtk.Button>("btn_Load");
				btnLoad.Clicked += delegate {
					LoadActiveProfile();
				};
			}
			Invoke(delegate {
				LoadActiveProfile();
			});
			return true;
		}
		protected override bool DisconnectInternal() {
			var boxRoot = Hall.Container.GetWidget<Gtk.Box>("box_Devices");
			var boxMain = Container.GetWidget("box_Root");
			boxRoot.Remove(boxMain);
			return true;
		}
		protected override bool DestroyInternal() {
			Container.Dispose();
			return true;
		}
		private void SafeActiveProfile() {
			var profile = new SetupProfile("Default");
			/*for (int i = 0; i < 16; i++) {
				int streamId = EquipmentMaster.Instance.VideoCaptureStream.GetEquipment(i, -1);
				if (streamId < 0) {
					continue;
				}
				CaptureStream stream = CaptureSystem.Instance.GetStream(streamId);
				if (stream == null) {
					continue;
				}
				CaptureSettings settings = stream.GetSettings();
				profile.AddVideoCapture(i, settings);
			}*/
			// video capture settings
			var cameraFactory = register.GetFactory<Iface.Main.Config.Camera.CameraFactory>("CameraConfig");
			for (int i = 0; i < 16; i++) {
				CaptureSettings settings = cameraFactory.GetWebcamSettings(i);
				if (settings != null) {
					profile.AddVideoCapture(i, settings);
				}
			}
			// virtual device config
			var deviceFactory = register.GetFactory<Iface.Main.Config.Devices.DeviceConfigFactory>("DeviceConfig");
			for (int i = 0; i < 16; i++) {
				VirtualDeviceSettings settings = deviceFactory.GetDeviceSettings(i);
				if (settings != null) {
					profile.AddDevice(i, settings);
				}
			}
			// tracker settings
			var trackerFactory = register.GetFactory<Iface.Main.Config.Tracking.TrackingFactory>("TrackingConfig");
			for (int i = 0; i < 16; i++) {
				TrackerSettings settings = trackerFactory.GetTrackerSettings(i);
				if (settings != null) {
					profile.AddTracker(i, settings);
				}
			}
			// tracker bindings
			var bindingsFactory = register.GetFactory<Iface.Main.View.Bindings.BindingsFactory>("BindingsView");
			for (int i = 0; i < 16; i++) {
				CapInputRelay settings = bindingsFactory.GetBindings(i);
				if (settings != null) {
					profile.AddBindings(i, settings);
				}
			}
			// tracker bindings
			ContentMaster.Instance.Profiles.SafeActiveProfile(profile);
			LogManager.Instance.LogMessage(this, "Default profile saved");
		}
		private void LoadActiveProfile() {
			var profile = ContentMaster.Instance.Profiles.LoadDefaultProfile();
			if (profile == null) {
				LogManager.Instance.LogError(this, "Failed to read profile");
				return;
			}
			// clear existing configuration
			var cameraFactory = register.GetFactory<Iface.Main.Config.Camera.CameraFactory>("CameraConfig");
			cameraFactory.RemoveWebcams();
			var trackerFactory = register.GetFactory<Iface.Main.Config.Tracking.TrackingFactory>("TrackingConfig");
			trackerFactory.RemoveTrackers();
			var deviceFactory = register.GetFactory<Iface.Main.Config.Devices.DeviceConfigFactory>("DeviceConfig");
			deviceFactory.RemoveDevices();
			var bindingsFactory = register.GetFactory<Iface.Main.View.Bindings.BindingsFactory>("BindingsView");

			// apply the new config
			foreach (int iCapture in profile.VideoCaptureIndexes) {
				if (!cameraFactory.AddWebcam(iCapture, profile.GetVideoCapture(iCapture))) {
					LogManager.Instance.LogError(this, "Failed to restore camera config");
					return;
				}
			}
			foreach (int iDevice in profile.DeviceIndexes) {
				if (!deviceFactory.AddDevice(iDevice, profile.GetDevice(iDevice))) {
					LogManager.Instance.LogError(this, "Failed to restore device config");
					return;
				}
			}
			foreach (int iTracker in profile.TrackerIndexes) {
				if (!trackerFactory.AddTracker(iTracker, profile.GetTracker(iTracker))) { // TODO: pass arguments
					LogManager.Instance.LogError(this, "Failed to restore tracker config");
					return;
				}
			}
			// we need to wait for the detection system to update tracker equipment
			DetectionSystem.Instance.Invoke(delegate {
				Invoke(delegate {
					foreach (int iTracker in profile.BindingIndexes) {
						if (!bindingsFactory.ApplyBindings(iTracker, profile.GetBindings(iTracker))) {
							LogManager.Instance.LogError(this, "Failed to restore tracker bindings");
							return;
						}
					}
				});
			});
			LogManager.Instance.LogMessage(this, "Default profile loaded");
		}
	}
}
