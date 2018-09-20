using System;
using Irseny.Content;
namespace Irseny.Viol.Main.Control.Output {
	public class VirtualKeyboardBindingsFactory : InterfaceFactory {
		readonly int deviceIndex;
		int freeNameId = 0;

		public VirtualKeyboardBindingsFactory(int deviceIndex) : base() {
			this.deviceIndex = deviceIndex;
		}
		protected override bool CreateInternal() {
			var factory = ContentMaster.Instance.Resources.InterfaceFactory.GetEntry("VirtualKeyboardBindings");
			Container = factory.CreateWidget("box_Root");
			return true;
		}
		protected override bool ConnectInternal() {
			var btnAdd = Container.GetWidget<Gtk.Button>("btn_Add");
			btnAdd.Clicked += delegate {
				Add();
			};
			var btnRemove = Container.GetWidget<Gtk.Button>("btn_Remove");
			btnRemove.Clicked += delegate {
				Remove();
			};
			return true;
		}
		protected override bool DisconnectInternal() {

			return true;
		}
		protected override bool DestroyInternal() {

			Container.Dispose();
			return true;
		}
		private bool Add() {
			var cbbType = Container.GetWidget<Gtk.ComboBoxText>("cbb_AddMode");
			var active = cbbType.ActiveText;
			if (active == null) {
				return false;
			} else if (active.Equals("Key")) {
				// determine floor id
				string floorName = string.Format("Key{0}", freeNameId);
				freeNameId += 1;
				// create floor
				var floor = new OuputDeviceSignalBindingFactory(deviceIndex, floorName);
				ConstructFloor(floorName, floor);
				var expMain = floor.Container.GetWidget("exp_Root");
				var boxBindings = Container.GetWidget<Gtk.Box>("box_Bindings");
				boxBindings.PackStart(expMain, false, true, 0);
				boxBindings.ShowAll();
				return true;
			} else {
				return false;
			}
		}
		private bool Remove() {
			// TODO: remove marked entries
			return true;
		}
	}
}
