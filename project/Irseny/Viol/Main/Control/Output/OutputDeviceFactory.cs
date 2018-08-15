using System;
using System.Collections.Generic;
using Irseny.Content;

namespace Irseny.Viol.Main.Control.Output {
	public class OutputDeviceFactory : InterfaceFactory {
		ISet<string> usedNames = new HashSet<string>();

		public OutputDeviceFactory() : base() {

		}
		protected override bool CreateInternal() {
			var factory = ContentMaster.Instance.Resources.InterfaceFactory.GetEntry("OutputDeviceControl");
			Container = factory.CreateWidget("box_Root");
			return true;
		}
		protected override bool ConnectInternal() {
			var boxRoot = Hall.Container.GetWidget<Gtk.Box>("box_Device");
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
			var boxRoot = Hall.Container.GetWidget<Gtk.Box>("box_Device");
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
				AddKeyboard();
			} else if (typeName.Equals("Joystick")) {
				AddJoystick();
			} else if (typeName.Equals("TrackIR")) {
				AddTrackInterface();
			}

		}
		private void AddKeyboard() {
			string name = RegisterAvailableName("Key");
			if (name.Length <= 0) {
				return;
			}
			var ntbDevice = Container.GetWidget<Gtk.Notebook>("ntb_Device");
			var floor = new VirtualKeyboardFactory();
			ConstructFloor(name, floor);
			usedNames.Add(name);
			var label = new Gtk.Label(name);
			Container.AddGadget(label);
			ntbDevice.AppendPage(floor.Container.GetWidget("box_Root"), label);
			ntbDevice.ShowAll();

		}
		private void AddJoystick() {
			string name = RegisterAvailableName("Joy");
			if (name.Length <= 0) {
				return;
			}
			var ntbDevice = Container.GetWidget<Gtk.Notebook>("ntb_Device");
			var floor = new VirtualKeyboardFactory();
			ConstructFloor(name, floor);
			var label = new Gtk.Label(name);
			Container.AddGadget(label);
			ntbDevice.AppendPage(floor.Container.GetWidget("box_Root"), label);
			ntbDevice.ShowAll();
		}
		private void AddTrackInterface() {
			string name = RegisterAvailableName("Tif");
			if (name.Length <= 0) {
				return;
			}
			var ntbDevice = Container.GetWidget<Gtk.Notebook>("ntb_Device");
			var floor = new VirtualKeyboardFactory();
			ConstructFloor(name, floor);
			var label = new Gtk.Label(name);
			Container.AddGadget(label);
			ntbDevice.AppendPage(floor.Container.GetWidget("box_Root"), label);
			ntbDevice.ShowAll();
		}
		private string RegisterAvailableName(string prefix, int startAt = 0, int attempts = 8) {
			for (int i = startAt; i < attempts + startAt; i++) {
				string candidate = string.Format("{0}{1}", prefix, i);
				if (!usedNames.Contains(candidate)) {
					usedNames.Add(candidate);
					return candidate;
				}
			}
			return string.Empty;
		}
		private void Remove() {
			var ntbDevice = Container.GetWidget<Gtk.Notebook>("ntb_Device");
			Gtk.Widget pageWidget = ntbDevice.CurrentPageWidget;
			if (pageWidget == null) {
				return;
			}
			string floorName = ntbDevice.GetTabLabelText(pageWidget);
			ntbDevice.Remove(pageWidget);
			IInterfaceFactory floor = DestructFloor(floorName);
			floor.Dispose();
			usedNames.Remove(floorName);
			ntbDevice.QueueDraw();
		}
	}
}

