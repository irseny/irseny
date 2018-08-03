using System;
using System.Diagnostics;
using Irseny.Content;
using Irseny.Listing;

namespace Irseny.Viol.Main.Display.Tracking {
	public class TrackingFactory : InterfaceFactory {
		public TrackingFactory() : base() {
		}
		protected override bool CreateInternal() {
			var factory = ContentMaster.Instance.Resources.InterfaceFactory.GetEntry("TrackingDisplay");
			Container = factory.CreateWidget("ntb_Root");
			return true;
		}
		protected override bool ConnectInternal() {
			var boxRoot = Hall.Container.GetWidget<Gtk.Box>("box_Tracking");
			var ntbMain = Container.GetWidget("ntb_Root");
			boxRoot.PackStart(ntbMain, true, true, 0);
			EquipmentMaster.Instance.HeadTracker.Updated += TrackerChanged;
			return true;
		}
		protected override bool DisconnectInternal() {
			EquipmentMaster.Instance.HeadTracker.Updated -= TrackerChanged;
			while (RemoveTracker()) {
			}
			var boxRoot = Hall.Container.GetWidget<Gtk.Box>("box_Tracking");
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
	}
}
