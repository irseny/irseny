﻿using System;
using System.Diagnostics;

namespace Irseny.Viol.Main.Image.Tracking {
	public class TrackingBaseFactory : InterfaceFactory {
		public TrackingBaseFactory() : base() {
		}
		protected override bool CreateInternal() {
			var factory = Mycena.InterfaceFactory.CreateFromFile(Content.ContentMaster.Instance.Resources.InterfaceDefinitions.GetEntry("TrackingImageBase"));
			Container = factory.CreateWidget("box_Root");
			return true;
		}
		protected override bool ConnectInternal() {
			var boxRoot = Hall.Container.GetWidget<Gtk.Box>("box_Tracking");
			var boxMain = Container.GetWidget("box_Root");
			boxRoot.PackStart(boxMain, true, true, 0);
			Listing.EquipmentMaster.Instance.HeadTracker.Updated += TrackerChanged;
			return true;
		}
		protected override bool DisconnectInternal() {
			Listing.EquipmentMaster.Instance.HeadTracker.Updated -= TrackerChanged;
			while (RemoveTracker()) {
			}
			var boxRoot = Hall.Container.GetWidget<Gtk.Box>("box_Tracking");
			var boxMain = Container.GetWidget("box_Root");
			boxRoot.Remove(boxMain);
			return true;
		}
		protected override bool DestroyInternal() {
			Container.Dispose();
			return true;
		}
		private void TrackerChanged(object sender, Listing.EquipmentUpdateArgs<int> args) {
			if (!Initialized) {
				return;
			}
			var ntbTrackers = Container.GetWidget<Gtk.Notebook>("ntb_Tracker");
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
			var ntbTracker = Container.GetWidget<Gtk.Notebook>("ntb_Tracker");
			int page = ntbTracker.NPages;
			var factory = new TrackingFactory(page);
			ConstructFloor("Track" + page, factory);
			var boxTracker = factory.Container.GetWidget("box_Root");
			var label = new Gtk.Label("Track" + page);
			Container.AddWidget(label);
			ntbTracker.AppendPage(boxTracker, label);
			ntbTracker.ShowAll();
			return true;
		}
		private bool RemoveTracker() {
			var ntbTracker = Container.GetWidget<Gtk.Notebook>("ntb_Tracker");
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
