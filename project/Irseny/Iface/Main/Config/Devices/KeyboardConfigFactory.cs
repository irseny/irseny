using System;
using Irseny.Log;
using Irseny.Content;
using Irseny.Inco.Device;
using Irseny.Listing;

namespace Irseny.Iface.Main.Config.Devices {
	public class KeyboardConfigFactory : InterfaceFactory {
		readonly int keyboardIndex;
		readonly int deviceIndex;

		public KeyboardConfigFactory(int keyboardIndex, int deviceIndex) : base() {
			this.keyboardIndex = keyboardIndex;
			this.deviceIndex = deviceIndex;
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

				int deviceId = VirtualDeviceManager.Instance.ConnectDevice(new VirtualKeyboard(keyboardIndex));
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

