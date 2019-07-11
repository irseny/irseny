using System;
using System.Collections.Generic;
using Irseny.Content;
using Irseny.Content.Profile;
using Irseny.Log;
using Irseny.Capture.Video;
using Irseny.Tracap;
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

			ContentMaster.Instance.Profiles.SafeActiveProfile(profile);
			LogManager.Instance.LogSignal(this, "Default profile saved");
		}
		private void LoadActiveProfile() {
			var profile = ContentMaster.Instance.Profiles.LoadDefaultProfile();
			if (profile == null) {
				LogManager.Instance.LogError(this, "Failed to read profile");
			}
			// clear existing configuration
			var cameraFactory = register.GetFactory<Iface.Main.Config.Camera.CameraFactory>("CameraConfig");
			cameraFactory.Clear();

			// apply the new config
			foreach (int iCapture in profile.VideoCaptureIndexes) {
				if (!cameraFactory.AddWebcam(iCapture, profile.GetVideoCapture(iCapture))) {
					LogManager.Instance.LogError(this, "Failed to restore camera config");
					return;
				}
			}
			LogManager.Instance.LogSignal(this, "Default profile loaded");
		}
	}
}
