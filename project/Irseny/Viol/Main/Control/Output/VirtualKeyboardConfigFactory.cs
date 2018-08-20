using System;
using Irseny.Log;
using Irseny.Content;
using Irseny.Inco.Device;
using Irseny.Listing;

namespace Irseny.Viol.Main.Control.Output {
	public class VirtualKeyboardConfigFactory : InterfaceFactory {
		readonly int keyboardIndex;
		readonly int deviceIndex;

		public VirtualKeyboardConfigFactory(int keyboardIndex, int deviceIndex) : base() {
			this.keyboardIndex = keyboardIndex;
			this.deviceIndex = deviceIndex;
		}
		protected override bool CreateInternal() {

			// TODO: create device and add to equipment
			var factory = ContentMaster.Instance.Resources.InterfaceFactory.GetEntry("VirtualKeyboardConfig");
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
			return true;
		}
		protected override bool DisconnectInternal() {
			return true;
		}
		protected override bool DestroyInternal() {
			VirtualDeviceManager.Instance.Invoke(delegate {
				int deviceId = EquipmentMaster.Instance.OutputDevice.GetEquipment(keyboardIndex, -1);
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
	}
}

