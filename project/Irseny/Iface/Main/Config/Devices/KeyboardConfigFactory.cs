using System;
using Irseny.Log;
using Irseny.Content;
using Irseny.Inco.Device;
using Irseny.Listing;

namespace Irseny.Iface.Main.Config.Devices {
	public class KeyboardConfigFactory : InterfaceFactory {
		readonly int keyboardIndex;
		readonly int deviceIndex;
		VirtualDeviceSettings settings;

		public KeyboardConfigFactory(VirtualDeviceSettings settings, int deviceIndex) : base() {
			if (settings == null) throw new ArgumentNullException("settings");
			if (deviceIndex < 0) throw new ArgumentOutOfRangeException("deviceIndex");
			if (keyboardIndex < 0) throw new ArgumentOutOfRangeException("keyboardIndex");
			this.keyboardIndex = settings.SubdeviceIndex;
			this.deviceIndex = deviceIndex;
			this.settings = settings;
		}
		protected override bool CreateInternal() {

			// TODO: create device and add to equipment
			var factory = ContentMaster.Instance.Resources.InterfaceFactory.GetEntry("KeyboardConfig");
			Container = factory.CreateWidget("box_Root");


			return true;
		}
		protected override bool ConnectInternal() {
			{
				/*var rdbTimed = Container.GetWidget<Gtk.RadioButton>("rdb_PolicyTimed");
				var rdbChange = Container.GetWidget<Gtk.RadioButton>("rdb_PolicyChange");
				var rgbComplete = Container.GetWidget<Gtk.RadioButton>("rdb_PolicyComplete");
				var txtRate = Container.GetWidget<Gtk.SpinButton>("txt_UpdateRate");
				rdbTimed.Clicked += PolicyUpdated;*/
			}
			VirtualDeviceManager.Instance.Invoke(delegate {
				var device = VirtualDevice.CreateFromSettings(settings);
				int deviceId = VirtualDeviceManager.Instance.ConnectDevice(device);
				if (deviceId < 0) {
					LogManager.Instance.Log(LogMessage.CreateError(this, "Failed to connect keyboard " + keyboardIndex));
					return;
				}
				EquipmentMaster.Instance.VirtualDevice.Update(deviceIndex, EquipmentState.Active, deviceId);
			});
			return true;
		}
		protected override bool DisconnectInternal() {
			VirtualDeviceManager.Instance.Invoke(delegate {
				int deviceId = EquipmentMaster.Instance.VirtualDevice.GetEquipment(deviceIndex, -1);
				if (deviceId < 0) {
					LogManager.Instance.Log(LogMessage.CreateWarning(this, "Failed to disconnect keyboard " + keyboardIndex));
					return;
				}
				EquipmentMaster.Instance.VirtualDevice.Update(deviceIndex, EquipmentState.Missing, -1);
				if (!VirtualDeviceManager.Instance.DisconnectDevice(deviceId)) {
					LogManager.Instance.Log(LogMessage.CreateWarning(this, "Failed to disconnect keyboard " + keyboardIndex));
					return;
				}
			});
			return true;
		}
		protected override bool DestroyInternal() {


			Container.Dispose();
			return true;
		}
		public int GetDeviceIndex() {
			return deviceIndex;
		}
		public int GetKeyboardIndex() {
			return keyboardIndex;
		}
		public VirtualDeviceSettings GetSettings() {
			return new VirtualDeviceSettings(settings);
		}
		public bool ApplySettings(VirtualDeviceSettings settings) {
			if (settings == null) throw new ArgumentNullException("settings");
			if (settings.DeviceType != VirtualDeviceType.Keyboard) throw new ArgumentException("settings.DeviceType");
			if (settings.SubdeviceIndex != keyboardIndex) throw new ArgumentException("settings.SubdeviceIndex");
			this.settings = new VirtualDeviceSettings(settings);
			// TODO: apply to user interface elements
			VirtualDeviceManager.Instance.Invoke(delegate {
				int deviceId = EquipmentMaster.Instance.VirtualDevice.GetEquipment(deviceIndex, -1);
				if (deviceId < 0) {
					LogManager.Instance.LogError(this, "Missing keyboard " + keyboardIndex);
					return;
				}
				IVirtualDevice device = VirtualDeviceManager.Instance.GetDevice(deviceId);
				if (device == null) {
					LogManager.Instance.LogError(this, "Missing keyboard " + keyboardIndex);
					return;
				}
				bool reconnect = false;
				if (device.DeviceIndex != settings.DeviceId) {
					reconnect = true;
				}
				if (reconnect) {
					if (!VirtualDeviceManager.Instance.DisconnectDevice(deviceId)) {
						LogManager.Instance.LogError(this, "Failed to disconnnect keyboard " + keyboardIndex);
						return;
					}
					device = VirtualDevice.CreateFromSettings(settings);
					deviceId = VirtualDeviceManager.Instance.ConnectDevice(device);
					if (deviceId < 0) {
						LogManager.Instance.LogError(this, "Failed to connect keyboard " + keyboardIndex);
						// TODO: try to avoid equipment updates, apply in device manager
						EquipmentMaster.Instance.VirtualDevice.Update(deviceIndex, EquipmentState.Missing, -1);
						return;
					}
					EquipmentMaster.Instance.VirtualDevice.Update(deviceIndex, EquipmentState.Active, deviceId);
				} else {
					if (device.SendRate != settings.SendRate) {
						device.SendRate = settings.SendRate;
					}
					if (device.SendPolicy != settings.SendPolicy) {
						device.SendPolicy = settings.SendPolicy;
					}
				}


			});
			return true;
		}
		private void PolicyUpdated(object sender, EventArgs args) {
			/*var rdbTimed = Container.GetWidget<Gtk.RadioButton>("rdb_PolicyTimed");
			var rdbChange = Container.GetWidget<Gtk.RadioButton>("rdb_PolicyChange");
			var rgbComplete = Container.GetWidget<Gtk.RadioButton>("rdb_PolicyComplete");
			int deviceId = EquipmentMaster.Instance.OutputDevice.GetEquipment(deviceIndex, -1);
			if (deviceId > -1) {
				EquipmentMaster.Instance.OutputDevice.Update(deviceIndex, EquipmentState.Active, deviceId);
			}*/
		}

	}
}

