using System;
using System.Collections.Generic;
using Irseny.Content;
using Irseny.Tracap;
using Irseny.Util;

namespace Irseny.Iface.Main.Config.Tracking {
	public class TrackingFactory : InterfaceFactory {
		readonly string FloorPrefix = "Tracker";
		readonly string TitlePrefix = "Track";

		public TrackingFactory() : base() {
		}
		protected override bool CreateInternal() {
			var factory = ContentMaster.Instance.Resources.InterfaceFactory.GetEntry("TrackingConfig");
			Container = factory.CreateWidget("box_Root");
			return true;
		}
		protected override bool ConnectInternal() {
			var boxRoot = Hall.Container.GetWidget<Gtk.Box>("box_Tracking");
			var boxMain = Container.GetWidget("box_Root");
			boxRoot.PackStart(boxMain, true, true, 0);
			var btnAdd = Container.GetWidget<Gtk.Button>("btn_Add");
			btnAdd.Clicked += delegate {
				AddFreeTracker();
			};
			var btnRemove = Container.GetWidget<Gtk.Button>("btn_Remove");
			btnRemove.Clicked += delegate {
				RemoveSelectedTracker();
			};
			return true;
		}
		protected override bool DisconnectInternal() {

			var boxRoot = Hall.Container.GetWidget<Gtk.Box>("box_Tracking");
			var boxMain = Container.GetWidget("box_Root");
			boxRoot.Remove(boxMain);
			return true;
		}
		protected override bool DestroyInternal() {
			Container.Dispose();
			return true;
		}
		public bool AddTracker(int index, TrackerSettings settings) {
			if (index < 0) throw new ArgumentOutOfRangeException("index");
			if (settings == null) throw new ArgumentNullException("settings");
			if (FloorNames.Contains(FloorPrefix + index)) {
				return false;
			}
			var ntbTracker = Container.GetWidget<Gtk.Notebook>("ntb_Root");
			// construct floor
			var factory = new CapTrackingFactory(index, settings);
			ConstructFloor(FloorPrefix + index, factory);
			var page = factory.Container.GetWidget("box_Root");
			var label = new Gtk.Label(TitlePrefix + index);
			Container.AddWidget(label);
			ntbTracker.AppendPage(page, label);
			ntbTracker.ShowAll();
			return true;
		}
		private bool AddFreeTracker() {
			var ntbTracker = Container.GetWidget<Gtk.Notebook>("ntb_Root");
			// find occupied indexes
			var occupied = new List<int>();
			foreach (IInterfaceFactory floor in Floors) {
				var factory = (CapTrackingFactory)floor;
				occupied.Add(factory.TrackerIndex);
			}
			// find free index
			int iFree;
			for (iFree = 0; iFree < 16; iFree++) {
				bool free = true;
				foreach (int iOccupied in occupied) {
					if (iOccupied == iFree) {
						free = false;
						break;
					}
				}
				if (free) {
					break;
				}
			}
			// create tracker
			if (iFree >= 16) {
				return false;
			}
			// TODO: initialize settings
			var settings = new TrackerSettings();
			return AddTracker(iFree, settings);
		}
		private bool RemoveSelectedTracker() {
			var ntbTracker = Container.GetWidget<Gtk.Notebook>("ntb_Root");
			int pageNo = ntbTracker.NPages;
			if (pageNo < 1) {
				return false;
			}
			int iPage = ntbTracker.CurrentPage;
			if (iPage < 0 || iPage >= pageNo) {
				return false;
			}
			Gtk.Widget page = ntbTracker.GetNthPage(iPage);
			string title = ntbTracker.GetTabLabelText(page);
			int iTracker = TextParseTools.ParseInt(title.Substring(TitlePrefix.Length), -1);
			if (iTracker < 0) {
				return false;
			}
			string name = FloorPrefix + iTracker;
			if (!FloorNames.Contains(name)) {
				return false;
			}
			Gtk.Widget label = ntbTracker.GetTabLabel(page);
			ntbTracker.RemovePage(iPage);
			label.Dispose();
			IInterfaceFactory floor = DestructFloor(name);
			floor.Dispose();
			return true;
		}
		public void RemoveTrackers() {
			// remove all tracker pages
			var ntbTracker = Container.GetWidget<Gtk.Notebook>("ntb_Root");
			while (ntbTracker.NPages > 0) {
				Gtk.Widget page = ntbTracker.GetNthPage(0);
				Gtk.Widget label = ntbTracker.GetTabLabel(page);
				ntbTracker.RemovePage(0);
				label.Dispose();
			}
			// deconstruct all trackers
			var names = new List<string>(FloorNames);
			foreach (string name in names) {
				IInterfaceFactory floor = DestructFloor(name);
				floor.Dispose();
			}

		}
		public TrackerSettings GetTrackerSettings(int index) {
			if (!Initialized) {
				return null;
			}
			foreach (IInterfaceFactory floor in Floors) {
				var factory = (CapTrackingFactory)floor;
				if (factory.TrackerIndex == index) {
					return factory.GetSettings();
				}
			}
			return null;
		}
	}
}
