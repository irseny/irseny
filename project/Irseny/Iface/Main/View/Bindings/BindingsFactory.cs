using System;
using System.Collections.Generic;
using System.Diagnostics;
using Irseny.Content;
using Irseny.Listing;
using Irseny.Tracking;
using Irseny.Inco.Device;
using Irseny.Util;

namespace Irseny.Iface.Main.View.Bindings {
	public class BindingsFactory : InterfaceFactory {
		const string TitlePrefix = "Track";
		const string FloorPrefix = "Track";
		const string SubfloorName = "Binding";

		public BindingsFactory() : base() {
		}
		public CapInputRelay GetBindings(int index) {
			string name = FloorPrefix + index;
			try {
				var floor = GetFloor(name);
				var factory = floor.GetFloor<BindingTabFactory>(SubfloorName);
				return factory.GetSettings();
			} catch (KeyNotFoundException) {
			} catch (ArgumentException) {
			}
			return null;
		}
		public bool ApplyBindings(int index, CapInputRelay settings) {
			if (settings == null) throw new ArgumentNullException("settings");
			string name = FloorPrefix + index;
			try {
				var floor = GetFloor(name);
				var factory = floor.GetFloor<BindingTabFactory>(SubfloorName);
				return factory.ApplySettings(settings);
			} catch (KeyNotFoundException) {
			} catch (ArgumentException) {
			}
			return false;
		}
		protected override bool CreateInternal() {
			var factory = ContentMaster.Instance.Resources.InterfaceFactory.GetEntry("TrackingView");
			Container = factory.CreateWidget("ntb_Root");
			return true;
		}
		protected override bool ConnectInternal() {
			var boxRoot = Hall.Container.GetWidget<Gtk.Box>("box_Bindings");
			var ntbMain = Container.GetWidget("ntb_Root");
			boxRoot.PackStart(ntbMain, true, true, 0);
			EquipmentMaster.Instance.HeadTracker.Updated += TrackerChanged;
			return true;
		}
		protected override bool DisconnectInternal() {
			EquipmentMaster.Instance.HeadTracker.Updated -= TrackerChanged;
			var boxRoot = Hall.Container.GetWidget<Gtk.Box>("box_Bindings");
			var ntbMain = Container.GetWidget("ntb_Root");
			boxRoot.Remove(ntbMain);
			return true;
		}
		protected override bool DestroyInternal() {
			Container.Dispose();
			return true;
		}
		private void TrackerChanged(object sender, EquipmentUpdateArgs<int> args) {
			if (!Initialized) {
				return;
			}
			var ntbTrackers = Container.GetWidget<Gtk.Notebook>("ntb_Root");
			if (args.Missing) {
				if (args.Index == ntbTrackers.NPages - 1) {
					RemoveTracker(args.Index);
				} else {
					Debug.WriteLine(this.GetType().Name + ": Trackers modified out of order");
				}
			} else {
				if (args.Index == ntbTrackers.NPages) {
					AddTracker(args.Index);
				} else {
					Debug.WriteLine(this.GetType().Name + ": Trackers modified out of order");
				}
			}
		}
		private bool AddTracker(int index) {
			if (!Initialized) {
				return false;
			}
			var ntbTracker = Container.GetWidget<Gtk.Notebook>("ntb_Root");
			var factory = new CapBindingsFactory(index);
			ConstructFloor(FloorPrefix + index, factory);
			var bindingFactory = new BindingTabFactory(index);
			factory.ConstructFloor(SubfloorName, bindingFactory);
			var boxRoot = factory.Container.GetWidget("box_Root");
			var label = new Gtk.Label(TitlePrefix + index);
			Container.AddWidget(label);
			ntbTracker.AppendPage(boxRoot, label);
			ntbTracker.ShowAll();
			return true;
		}
		private bool RemoveTracker(int index) {
			if (!Initialized) {
				return false;
			}
			var ntbTracker = Container.GetWidget<Gtk.Notebook>("ntb_Root");
			// remove the tracker page
			int pageNo = ntbTracker.NPages;
			for (int iPage = 0; iPage < pageNo; iPage++) {
				Gtk.Widget page = ntbTracker.GetNthPage(iPage);
				string title = ntbTracker.GetTabLabelText(page);
				int iTracker = TextParseTools.ParseInt(title.Substring(TitlePrefix.Length), -1);
				if (iTracker == index) {
					Gtk.Widget label = ntbTracker.GetTabLabel(page);
					ntbTracker.RemovePage(iPage);
					label.Dispose();
					break;
				}
			}
			// destruct the floor
			IInterfaceFactory floor = DestructFloor(FloorPrefix + index);
			if (floor == null) {
				return false;
			}
			floor.Dispose();
			return true;
		}
	}
}
