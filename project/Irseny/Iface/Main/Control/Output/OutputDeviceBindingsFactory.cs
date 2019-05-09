using System;
using System.Diagnostics;
using System.Collections.Generic;
using Irseny.Log;
using Irseny.Listing;
using Irseny.Inco.Device;
using Irseny.Content;

namespace Irseny.Iface.Main.Control.Output {
	public class OutputDeviceBindingsFactory : InterfaceFactory {
		ISet<string> usedNames = new HashSet<string>();
		IDictionary<int, string> deviceFloorMap = new Dictionary<int, string>(32);
		public OutputDeviceBindingsFactory() : base() {
		}

		protected override bool CreateInternal() {
			var factory = ContentMaster.Instance.Resources.InterfaceFactory.GetEntry("OutputDeviceBindings");
			Container = factory.CreateWidget("ntb_Root");
			return true;
		}
		protected override bool ConnectInternal() {
			// TODO: listen to equipment master
			var boxRoot = Hall.Container.GetWidget<Gtk.Box>("box_Bindings");
			var ntbMain = Container.GetWidget("ntb_Root");
			boxRoot.PackStart(ntbMain, true, true, 0);
			EquipmentMaster.Instance.OutputDevice.Updated += DeviceUpdated;
			return true;
		}
		protected override bool DisconnectInternal() {
			EquipmentMaster.Instance.OutputDevice.Updated -= DeviceUpdated;
			var boxRoot = Hall.Container.GetWidget<Gtk.Box>("box_Bindings");
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
							AddDevice(device.DeviceType, args.Index, args.Equipment);
						});
					}
				});
			} else {
				Invoke(delegate {
					RemoveDevice(args.Index);
				});
			}
		}
		private bool AddDevice(VirtualDeviceType deviceType, int deviceIndex, int deviceId) {
			var ntbRoot = Container.GetWidget<Gtk.Notebook>("ntb_Root");
			int nextIndex = ntbRoot.NPages;
			int deviceFamilyIndex;
			string name;
			string family;
			IInterfaceFactory floor;
			switch (deviceType) {
			case VirtualDeviceType.Keyboard:
				floor = new VirtualKeyboardBindingsFactory(nextIndex);
				family = "Key";
				break;
			case VirtualDeviceType.Mouse:
				floor = null;
				family = "Mouse";
				break;
			case VirtualDeviceType.Joystick:
				floor = null;
				family = "Joy";
				break;
			case VirtualDeviceType.TrackingInterface:
				floor = null;
				family = "Tif";
				break;
			default:
				throw new ArgumentException("deviceType: Unknown type: " + deviceType);
			}
			if (!RegisterFloorName(family, deviceIndex, 0, 16, out name, out deviceFamilyIndex)) {
				return false;
			}
			ConstructFloor(name, floor);
			var label = new Gtk.Label(name);
			floor.Container.AddGadget(label);
			var boxMain = floor.Container.GetWidget("box_Root");
			ntbRoot.AppendPage(boxMain, label);
			ntbRoot.ShowAll();
			return true;
		}
		private bool RemoveDevice(int deviceIndex) {
			var ntbRoot = Container.GetWidget<Gtk.Notebook>("ntb_Root");
			string floorName = GetDeviceFloor(deviceIndex);
			if (floorName.Length < 1) {
				return false;
			}

			IInterfaceFactory floor = GetFloor(floorName);
			var boxMain = floor.Container.GetWidget("box_Root");
			ntbRoot.Remove(boxMain);
			DestructFloor(floorName);
			UnregisterFloorName(floorName, deviceIndex);
			return true;
		}
		private bool RegisterFloorName(string family, int deviceIndex, int start, int maxNames, out string name, out int index) {
			// the deviceindex could be known at this point
			// ignore such entries
			if (deviceFloorMap.ContainsKey(deviceIndex)) {
				name = string.Empty;
				index = start - 1;
				return false;
			}
			// find an unused name consisting of <family><NUMBER>
			for (int i = start; i < start + maxNames; i++) {
				string candidate = string.Format("{0}{1}", family, i);
				if (!usedNames.Contains(candidate)) {
					usedNames.Add(candidate);
					name = candidate;
					index = i;
					// register the floor name that corresponds to the given device index
					deviceFloorMap.Add(deviceIndex, name);
					return true;
				}
			}
			// all names reserved
			name = string.Empty;
			index = start - 1;
			return false;
		}
		private bool UnregisterFloorName(string floorName, int deviceIndex) {
			deviceFloorMap.Remove(deviceIndex);
			usedNames.Remove(floorName);
			return true;
		}
		private string GetDeviceFloor(int deviceIndex) {
			string result;
			if (deviceFloorMap.TryGetValue(deviceIndex, out result)) {
				return result;
			} else {
				return string.Empty;
			}
		}
	}
}

