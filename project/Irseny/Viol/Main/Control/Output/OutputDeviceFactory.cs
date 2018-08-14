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
				var cbbAdd = Container.GetWidget<Gtk.ComboBoxText>("cbb_Type");
				cbbAdd.EditingDone += delegate {
					Console.WriteLine("editing done");
				};
				cbbAdd.Changed += delegate {
					Console.WriteLine("changed");
				};
				cbbAdd.MoveActive += delegate {
					Console.WriteLine("move active");
				};
				cbbAdd.PoppedDown += delegate {
					Console.WriteLine("popped down");
				};
				cbbAdd.PoppedUp += delegate {
					Console.WriteLine("popped up");
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
			string name = string.Empty;
			for (int i = 0; i < 8; i++) {
				string candidate = string.Format("Key{0}", i);
				if (!usedNames.Contains(candidate)) {
					name = candidate;
					break;
				}
			}
			if (name.Length > 0) {
				var ntbDevice = Container.GetWidget<Gtk.Notebook>("ntb_Device");
				var floor = new VirtualKeyboardFactory();
				ConstructFloor(name, floor);
				usedNames.Add(name);
				var label = new Gtk.Label(name);
				ntbDevice.AppendPage(label, floor.Container.GetWidget("box_Root"));
				ntbDevice.QueueDraw();
			}

		}
		private void AddJoystick() {
			
		}
		private void AddTrackInterface() {
			
		}
		private void Remove() {
			var ntbDevice = Container.GetWidget<Gtk.Notebook>("ntb_Device");
			Gtk.Widget pageWidget = ntbDevice.CurrentPageWidget;
			if (pageWidget == null) {
				return;
			}
			string floorName = ntbDevice.GetTabLabelText(pageWidget);
			IInterfaceFactory floor = DestructFloor(floorName);
			floor.Dispose();
			usedNames.Remove(floorName);
		}
	}
}

