using System;
using System.Threading;
using System.Diagnostics;
using System.Collections.Generic;
using Irseny.Content;
using Irseny.Listing;
using Irseny.Util;

namespace Irseny.Iface.Main.View.Tracking {
	public class TrackingFactory : InterfaceFactory {
		const string TitlePrefix = "Track";
		const string FloorPrefix = "Tracker";

		readonly object trackerLock = new object();
		public TrackingFactory() : base() {
		}
		protected override bool CreateInternal() {
			var factory = ContentMaster.Instance.Resources.InterfaceFactory.GetEntry("TrackingView");
			Container = factory.CreateWidget("ntb_Root");
			return true;
		}
		protected override bool ConnectInternal() {
			var boxRoot = Hall.Container.GetWidget<Gtk.Box>("box_Tracking");
			var ntbRoot = Container.GetWidget<Gtk.Notebook>("ntb_Root");
			boxRoot.PackStart(ntbRoot, true, true, 0);
			ntbRoot.SwitchPage += ActiveTrackerSelected;
			EquipmentMaster.Instance.HeadTracker.Updated += TrackerChanged;
			EquipmentMaster.Instance.Surface.Updated += ActiveTrackerUpdated;
			return true;
		}
		protected override bool DisconnectInternal() {
			EquipmentMaster.Instance.Surface.Updated -= ActiveTrackerUpdated;
			EquipmentMaster.Instance.HeadTracker.Updated -= TrackerChanged;
			var boxRoot = Hall.Container.GetWidget<Gtk.Box>("box_Tracking");
			var ntbMain = Container.GetWidget("ntb_Root");
			boxRoot.Remove(ntbMain);
			RemoveTrackers();
			var ntbRoot = Container.GetWidget<Gtk.Notebook>("ntb_Root");
			ntbRoot.SwitchPage -= ActiveTrackerSelected;
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
					RemoveTracker();
				} else {
					Debug.WriteLine(this.GetType().Name + ": Trackers modified out of order");
				}
			} else {
				if (args.Index == ntbTrackers.NPages) {
					AddTracker();
				} else {
					Debug.WriteLine(this.GetType().Name + ": Trackers modified out of order");
				}
			}
		}
		private bool AddTracker() {
			var ntbTracker = Container.GetWidget<Gtk.Notebook>("ntb_Root");
			int page = ntbTracker.NPages;
			var factory = new CapTrackingFactory(page);
			ConstructFloor("Track" + page, factory);
			var boxTracker = factory.Container.GetWidget("box_Root");
			var label = new Gtk.Label("Track" + page);
			Container.AddWidget(label);
			ntbTracker.AppendPage(boxTracker, label);
			ntbTracker.ShowAll();
			return true;
		}
		private bool RemoveTracker() {
			var ntbTracker = Container.GetWidget<Gtk.Notebook>("ntb_Root");
			int page = ntbTracker.NPages - 1;
			if (page > -1) {
				ntbTracker.RemovePage(page);
				var floor = DestructFloor("Track" + page);
				floor.Dispose();
				return true;
			}
			return false;
		}
		private void RemoveTrackers() {
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
