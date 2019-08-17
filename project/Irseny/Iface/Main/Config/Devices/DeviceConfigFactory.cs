using System;
using System.Collections.Generic;
using Irseny.Content;
using Irseny.Log;
using Irseny.Inco.Device;

namespace Irseny.Iface.Main.Config.Devices {
	public class DeviceConfigFactory : InterfaceFactory {
		const string KeyboardTitlePrefix = "Key";
		const string MouseTitlePrefix = "Mouse";
		const string JoystickTitlePrefix = "Joy";
		const string TrackingTitlePrefix = "Track";

		public DeviceConfigFactory() : base() {

		}
		protected override bool CreateInternal() {
			var factory = ContentMaster.Instance.Resources.InterfaceFactory.GetEntry("DeviceConfig");
			Container = factory.CreateWidget("box_Root");
			return true;
		}
		protected override bool ConnectInternal() {
			var boxRoot = Hall.Container.GetWidget<Gtk.Box>("box_Devices");
			var boxMain = Container.GetWidget("box_Root");
			boxRoot.PackStart(boxMain, true, true, 0);
			{
				var btnAdd = Container.GetWidget<Gtk.Button>("btn_Add");
				btnAdd.Clicked += delegate {
					AddSelectedDevice();
				};
			}
			{
				var btnRemove = Container.GetWidget<Gtk.Button>("btn_Remove");
				btnRemove.Clicked += delegate {
					RemoveSelectedDevice();
				};
			}

			return true;
		}
		protected override bool DisconnectInternal() {
			var boxRoot = Hall.Container.GetWidget<Gtk.Box>("box_Devices");
			var boxMain = Container.GetWidget("box_Root");
			boxRoot.Remove(boxMain);
			RemoveDevices();
			return true;
		}
		protected override bool DestroyInternal() {
			Container.Dispose();
			return true;
		}
		public VirtualDeviceSettings GetDeviceSettings(int index) {
			foreach (IInterfaceFactory floor in Floors) {
				if (floor is KeyboardConfigFactory) {
					var factory = (KeyboardConfigFactory)floor;
					if (factory.GetDeviceIndex() == index) {
						return factory.GetSettings();
					}
				} else {
					throw new NotImplementedException();
				}
			}
			return null;
		}
		private bool AddSelectedDevice() {
			if (!Initialized) {
				return false;
			}
			var settings = new VirtualDeviceSettings();
			// determine type to add
			var cbbType = Container.GetWidget<Gtk.ComboBoxText>("cbb_Type");
			string typeName = cbbType.ActiveText;
			if (typeName == null) {
				return false;
			} else if (typeName.Equals("Keyboard")) {
				settings.DeviceType = VirtualDeviceType.Keyboard;
			} else if (typeName.Equals("Mouse")) {
				settings.DeviceType = VirtualDeviceType.Mouse;
			} else if (typeName.Equals("Joystick")) {
				settings.DeviceType = VirtualDeviceType.Joystick;
			} else if (typeName.Equals("Freetrack")) {
				settings.DeviceType = VirtualDeviceType.TrackingInterface;
			}
			// build assigned device lists
			var takenDevices = new List<int>(16);
			var takenSubdevices = new List<int>(16);
			foreach (IInterfaceFactory floor in Floors) {
				if (floor is KeyboardConfigFactory) {
					var factory = (KeyboardConfigFactory)floor;
					takenDevices.Add(factory.GetDeviceIndex());
					if (settings.DeviceType == VirtualDeviceType.Keyboard) {
						takenSubdevices.Add(factory.GetKeyboardIndex());
					}
				}
			}
			// find a free device
			int iDevice;
			for (iDevice = 0; iDevice < 16; iDevice++) {
				bool deviceTaken = false;
				foreach (int d in takenDevices) {
					if (d == iDevice) {
						deviceTaken = true;
					}
				}
				if (!deviceTaken) {
					break;
				}
			}
			if (iDevice >= 16) {
				return false;
			}
			// find a free subdevice
			int iSubdevice;
			for (iSubdevice = 0; iSubdevice < 16; iSubdevice++) {
				bool deviceTaken = false;
				foreach (int d in takenSubdevices) {
					if (d == iSubdevice) {
						deviceTaken = true;
					}
				}
				if (!deviceTaken) {
					break;
				}
			}
			if (iSubdevice >= 16) {
				return false;
			}
			settings.SubdeviceIndex = iSubdevice;
			return AddDevice(iDevice, settings);
		}
		public bool AddDevice(int deviceIndex, VirtualDeviceSettings settings) {
			if (settings == null) throw new ArgumentNullException("settings");
			if (deviceIndex < 0) throw new ArgumentOutOfRangeException("deviceIndex");
			if (!Initialized) {
				return false;
			}
			// select the title and factory
			var ntbDevice = Container.GetWidget<Gtk.Notebook>("ntb_Device");
			IInterfaceFactory floor;
			string title;
			switch (settings.DeviceType) {
			case VirtualDeviceType.Keyboard:
				floor = new KeyboardConfigFactory(settings, deviceIndex);
				title = KeyboardTitlePrefix + settings.SubdeviceIndex;
				break;
			default:
				throw new NotImplementedException();
			}
			// create the floor
			// note that the floor name is the same as the title
			if (!ConstructFloor(title, floor)) {
				return false;
			}
			var boxRoot = floor.Container.GetWidget("box_Root");
			var label = new Gtk.Label(title);
			Container.AddWidget(label);
			ntbDevice.AppendPage(boxRoot, label);
			return true;
		}
		private bool RemoveSelectedDevice() {
			if (!Initialized) {
				return false;
			}
			// get selected page
			var ntbDevice = Container.GetWidget<Gtk.Notebook>("ntb_Device");
			int iPage = ntbDevice.CurrentPage;
			if (iPage < 0 || iPage >= ntbDevice.NPages) {
				return false;
			}
			// query tab
			Gtk.Widget page = ntbDevice.GetNthPage(iPage);
			Gtk.Widget label = ntbDevice.GetTabLabel(page);
			string title = ntbDevice.GetTabLabelText(page);
			// destruct tab and floor
			ntbDevice.RemovePage(iPage);
			label.Dispose();
			ntbDevice.RemovePage(iPage);
			IInterfaceFactory floor = DestructFloor(title);
			if (floor == null) {
				return false;
			}
			floor.Dispose();
			return true;
		}
		public bool RemoveDevices() {
			if (!Initialized) {
				return false;
			}
			// remove all tabs
			var ntbDevice = Container.GetWidget<Gtk.Notebook>("ntb_Device");
			while (ntbDevice.NPages > 0) {
				Gtk.Widget page = ntbDevice.GetNthPage(0);
				Gtk.Widget label = ntbDevice.GetTabLabel(page);
				ntbDevice.RemovePage(0);
				label.Dispose();
			}
			// destruct all floors
			var floorNames = new List<string>(FloorNames);
			foreach (string name in floorNames) {
				IInterfaceFactory floor = DestructFloor(name);
				floor.Dispose();
			}
			return true;
		}
		/*private void Add() {
			var cbbType = Container.GetWidget<Gtk.ComboBoxText>("cbb_Type");
			string typeName = cbbType.ActiveText;
			if (typeName == null) {
				return;
			} else if (typeName.Equals("Keyboard")) {
				AddDevice(VirtualDeviceType.Keyboard);
			} else if (typeName.Equals("Mouse")) {
				AddDevice(VirtualDeviceType.Mouse);
			} else if (typeName.Equals("Joystick")) {
				AddDevice(VirtualDeviceType.Joystick);
			} else if (typeName.Equals("Freetrack")) {
				AddDevice(VirtualDeviceType.TrackingInterface);
			}

		}
		private bool AddDevice(VirtualDeviceType deviceType) {
			string name;
			int innerIndex;
			string family;
			switch (deviceType) {
			case VirtualDeviceType.Keyboard:
				family = "Key";
				break;
			case VirtualDeviceType.Joystick:
				family = "Joy";
				break;
			case VirtualDeviceType.Mouse:
				family = "Mou";
				break;
			case VirtualDeviceType.TrackingInterface:
				family = "Tif";
				break;
			default:
				throw new ArgumentException("deviceType: Unknown type: " + deviceType);
			}
			if (!RegisterAvailableName(family, 0, 16, out name, out innerIndex)) {
				return false;
			}
			var ntbDevice = Container.GetWidget<Gtk.Notebook>("ntb_Device");
			int deviceIndex = ClaimAvailableDevice();
			if (deviceIndex < 0) {
				return false;
			}

			IInterfaceFactory floor;
			switch (deviceType) {
			case VirtualDeviceType.Keyboard:
				floor = new KeyboardConfigFactory(innerIndex, deviceIndex);
				break;
			case VirtualDeviceType.Joystick:
			case VirtualDeviceType.Mouse:
			case VirtualDeviceType.TrackingInterface:
				LogManager.Instance.Log(LogMessage.CreateError(this, "Configuration not implemented"));
				return false;
			default:
				throw new ArgumentException("deviceType: Unknown type: " + deviceType);
			}

			ConstructFloor(name, floor);
			usedNames.Add(name);
			var label = new Gtk.Label(name);
			Container.AddGadget(label);
			ntbDevice.AppendPage(floor.Container.GetWidget("box_Root"), label);
			ntbDevice.ShowAll();
			return true;
		}
		private bool RegisterAvailableName(string family, int start, int maxNames, out string name, out int index) {
			for (int i = start; i < start + maxNames; i++) {
				string candidate = string.Format("{0}{1}", family, i);
				if (!usedNames.Contains(candidate)) {
					usedNames.Add(candidate);
					name = candidate;
					index = i;
					return true;
				}
			}
			name = string.Empty;
			index = start - 1;
			return false;
		}
		private int ClaimAvailableDevice() {
			for (int i = 0; i < deviceClaimed.Length; i++) {
				if (!deviceClaimed[i]) {
					deviceClaimed[i] = true;
					return i;
				}
			}
			return -1;
		}
		private bool Remove() {
			var ntbDevice = Container.GetWidget<Gtk.Notebook>("ntb_Device");
			Gtk.Widget pageWidget = ntbDevice.CurrentPageWidget;
			if (pageWidget == null) {
				return false;
			}
			string floorName = ntbDevice.GetTabLabelText(pageWidget);
			ntbDevice.Remove(pageWidget);
			IInterfaceFactory floor = DestructFloor(floorName);
			floor.Dispose();
			usedNames.Remove(floorName);
			ntbDevice.QueueDraw();
			return true;
		}*/
	}
}

