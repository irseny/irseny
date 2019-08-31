using System;
using System.Threading;
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

		readonly object trackerLock = new object();

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
			var ntbRoot = Container.GetWidget<Gtk.Notebook>("ntb_Root");
			boxRoot.PackStart(ntbRoot, true, true, 0);
			ntbRoot.SwitchPage += ActiveTrackerSelected;
			EquipmentMaster.Instance.HeadTracker.Updated += TrackerChanged;
			EquipmentMaster.Instance.Surface.Updated -= ActiveTrackerUpdated;
			return true;
		}
		protected override bool DisconnectInternal() {
			EquipmentMaster.Instance.Surface.Updated -= ActiveTrackerUpdated;
			EquipmentMaster.Instance.HeadTracker.Updated -= TrackerChanged;
			var boxRoot = Hall.Container.GetWidget<Gtk.Box>("box_Bindings");
			var ntbRoot = Container.GetWidget<Gtk.Notebook>("ntb_Root");
			boxRoot.Remove(ntbRoot);
			ntbRoot.SwitchPage -= ActiveTrackerSelected;
			RemoveTrackers();
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
		private void RemoveTrackers() {
			if (!Initialized) {
				return;
			}
			var ntbTracker = Container.GetWidget<Gtk.Notebook>("ntb_Root");
			while (ntbTracker.NPages > 0) {
				Gtk.Widget page = ntbTracker.GetNthPage(0);
				Gtk.Widget label = ntbTracker.GetTabLabel(page);
				ntbTracker.RemovePage(0);
				label.Dispose();
			}

			var floorNames = new List<string>(FloorNames);
			foreach (string name in floorNames) {
				IInterfaceFactory floor = DestructFloor(name);
				floor.Dispose();
			}
		}
		private void ActiveTrackerSelected(object sender, EventArgs args) {
			if (!Initialized) {
				return;
			}
			if (Monitor.IsEntered(trackerLock)) {
				return;
			}
			var ntbRoot = (Gtk.Notebook)sender;
			Gtk.Widget page = ntbRoot.CurrentPageWidget;
			string title = ntbRoot.GetTabLabelText(page);
			int iTracker = TextParseTools.ParseInt(title.Substring(TitlePrefix.Length), -1);
			if (iTracker < 0) {
				return;
			}
			lock (trackerLock) {
				EquipmentMaster.Instance.Surface.Update(SurfacePage.ActiveTracker, EquipmentState.Active, iTracker);
			}
		}
		private void ActiveTrackerUpdated(object sender, EquipmentUpdateArgs<int> args) {
			if (args.Index != SurfacePage.ActiveTracker) {
				return;
			}
			if (!args.Active) {
				return;
			}
			int iTracker = args.Equipment;
			if (iTracker < 0) {
				return;
			}
			if (Monitor.IsEntered(trackerLock)) {
				return;
			}
			Invoke(delegate {
				if (!Initialized) {
					return;
				}
				string targetTitle = TitlePrefix + iTracker;
				var ntbRoot = Container.GetWidget<Gtk.Notebook>("ntb_Root");
				int pageNo = ntbRoot.NPages;
				for (int p = 0; p < pageNo; p++) {
					Gtk.Widget page = ntbRoot.GetNthPage(p);
					string title = ntbRoot.GetTabLabelText(page);
					if (title.Equals(targetTitle)) {
						if (ntbRoot.CurrentPage != p) {
							lock (trackerLock) {
								ntbRoot.CurrentPage = p;
							}
						}
					}
				}
			});
		}
	}
}
