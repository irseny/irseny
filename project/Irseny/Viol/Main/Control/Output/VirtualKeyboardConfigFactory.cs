using System;
using Irseny.Log;
using Irseny.Content;
using Irseny.Inco.Device;
using Irseny.Listing;

namespace Irseny.Viol.Main.Control.Output {
	public class VirtualKeyboardConfigFactory : InterfaceFactory {
		readonly int index;

		public VirtualKeyboardConfigFactory(int index) : base() {
			this.index = index;
		}
		protected override bool CreateInternal() {

			// TODO: create device and add to equipment
			var factory = ContentMaster.Instance.Resources.InterfaceFactory.GetEntry("VirtualKeyboardConfig");
			Container = factory.CreateWidget("box_Root");

			VirtualDeviceManager.Instance.Invoke(delegate {
				int deviceId = VirtualDeviceManager.Instance.MountDevice(new VirtualKeyboard());
				if (deviceId < 0) {
					LogManager.Instance.Log(LogMessage.CreateError(this, "Failed to mount keyboard: " + index));
				}
				EquipmentMaster.Instance.OutputDevice.Update(index, EquipmentState.Active, deviceId);
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
				int deviceId = EquipmentMaster.Instance.OutputDevice.GetEquipment(index, -1);
				if (deviceId < 0) {
					LogManager.Instance.Log(LogMessage.CreateError(this, "Failed to unmount unregistered device: " + index));
					return;
				}
				EquipmentMaster.Instance.OutputDevice.Update(index, EquipmentState.Missing, -1);
				if (VirtualDeviceManager.Instance.UnmountDevice(deviceId) == null) {
					LogManager.Instance.Log(LogMessage.CreateError(this, "Failed to unmount device: " + index));
					return;
				}
			});

			Container.Dispose();
			return true;
		}
	}
}

