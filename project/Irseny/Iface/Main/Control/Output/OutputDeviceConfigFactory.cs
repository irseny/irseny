using System;
using System.Collections.Generic;
using Irseny.Content;

namespace Irseny.Iface.Main.Control.Output {
	public class OutputDeviceConfigFactory : InterfaceFactory {
		ISet<string> usedNames = new HashSet<string>();
		bool[] deviceClaimed = new bool[32];
		public OutputDeviceConfigFactory() : base() {

		}
		protected override bool CreateInternal() {
			var factory = ContentMaster.Instance.Resources.InterfaceFactory.GetEntry("OutputDeviceConfig");
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
					Add();
				};
			}
			{
				var btnRemove = Container.GetWidget<Gtk.Button>("btn_Remove");
				btnRemove.Clicked += delegate {
					Remove();
				};
			}

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
		private void Add() {
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
			} else if (typeName.Equals("TrackIR")) {
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
				floor = new VirtualKeyboardConfigFactory(innerIndex, deviceIndex);
				break;
			case VirtualDeviceType.Joystick:
			case VirtualDeviceType.Mouse:
			case VirtualDeviceType.TrackingInterface:
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
		}
	}
}

