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

			VirtualDeviceManager.Instance.Invoke(delegate {
				int deviceId = VirtualDeviceManager.Instance.MountDevice(new VirtualKeyboard());
				if (deviceId < 0) {
					LogManager.Instance.Log(LogMessage.CreateError(this, "Failed to mount keyboard: " + keyboardIndex));
				}
				// TODO: translate from keyboard to device index
				EquipmentMaster.Instance.OutputDevice.Update(deviceIndex, EquipmentState.Active, deviceId);
			});



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
			return true;
		}
		protected override bool DisconnectInternal() {
			return true;
		}

		protected override bool DestroyInternal() {
			VirtualDeviceManager.Instance.Invoke(delegate {
				int deviceId = EquipmentMaster.Instance.OutputDevice.GetEquipment(deviceIndex, -1);
				if (deviceId < 0) {
					LogManager.Instance.Log(LogMessage.CreateError(this, "Failed to unmount unregistered keyboard: " + keyboardIndex));
					return;
				}
				EquipmentMaster.Instance.OutputDevice.Update(deviceIndex, EquipmentState.Missing, -1);
				if (VirtualDeviceManager.Instance.UnmountDevice(deviceId) == null) {
					LogManager.Instance.Log(LogMessage.CreateError(this, "Failed to unmount keyboard: " + keyboardIndex));
					return;
				}
			});

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

