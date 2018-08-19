using System;
using Irseny.Log;
using Irseny.Listing;
using Irseny.Inco.Device;
using Irseny.Content;

namespace Irseny.Viol.Main.Control.Output {
	public class OutputDeviceBindingsFactory : InterfaceFactory {
		public OutputDeviceBindingsFactory() : base() {
		}

		protected override bool CreateInternal() {
			var factory = ContentMaster.Instance.Resources.InterfaceFactory.GetEntry("OutputDeviceBindings");
			Container = factory.CreateWidget("ntb_Root");
			return true;
		}
		protected override bool ConnectInternal() {
			// TODO: listen to equipment master
			var boxRoot = Hall.Container.GetWidget<Gtk.Box>("box_Assignment");
			var ntbMain = Container.GetWidget("ntb_Root");
			boxRoot.PackStart(ntbMain, true, true, 0);
			EquipmentMaster.Instance.OutputDevice.Updated += DeviceUpdated;
			return true;
		}
		protected override bool DisconnectInternal() {
			EquipmentMaster.Instance.OutputDevice.Updated -= DeviceUpdated;
			var boxRoot = Hall.Container.GetWidget<Gtk.Box>("box_Assignment");
			var ntbMain = Container.GetWidget("ntb_Root");
			boxRoot.Remove(ntbMain);
			return true;
		}
		protected override bool DestroyInternal() {
			Container.Dispose();
			return true;
		}
		private void DeviceUpdated(object sender, EquipmentUpdateArgs<int> args) {
			// the state is assumed should switch between active and missing
			if (args.Active) {
				VirtualDeviceManager.Instance.Invoke(delegate {
					IVirtualDevice device = VirtualDeviceManager.Instance.GetDevice(args.Equipment);
					if (device != null) {
						Invoke(delegate {
							// ignore changes out of order
							var ntbRoot = Container.GetWidget<Gtk.Notebook>("ntb_Root");
							int nextIndex = ntbRoot.NPages;
							if (nextIndex != args.Index) {
								AddDevice(device);
							} else {
								LogManager.Instance.Log(LogMessage.CreateError(this, string.Format("Device {0} modified out of order", args.Index)));
							}
						});
					}
				});
			} else {
				Invoke(delegate {
					var ntbRoot = Container.GetWidget<Gtk.Notebook>("ntb_Root");
					int lastIndex = ntbRoot.NPages - 1;
					// ignore all changes out of order
					if (lastIndex == args.Index) {
						RemoveDevice();
					} else {
						LogManager.Instance.Log(LogMessage.CreateError(this, string.Format("Device {0} modified out of order", args.Index)));
					}
				});
			}
		}
		private void AddDevice(IVirtualDevice device) {
			var ntbRoot = Container.GetWidget<Gtk.Notebook>("ntb_Root");
			int nextIndex = ntbRoot.NPages;

			// TODO: add factory
		}
		private void RemoveDevice() {
			var ntbRoot = Container.GetWidget<Gtk.Notebook>("ntb_Root");

			int lastIndex = ntbRoot.NPages - 1;

			// TODO: remove factory
		}
	}
}

