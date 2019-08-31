using System;
using System.Threading;
using System.Collections.Generic;
using Irseny.Content;
using Irseny.Tracking;
using Irseny.Util;
using Irseny.Listing;

namespace Irseny.Iface.Main.Config.Tracking {
	public class TrackingFactory : InterfaceFactory {
		readonly string FloorPrefix = "Tracker";
		readonly string TitlePrefix = "Track";

		readonly object trackerLock = new object();

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
			var ntbRoot = Container.GetWidget<Gtk.Notebook>("ntb_Root");
			ntbRoot.SwitchPage += ActiveTrackerSelected;
			EquipmentMaster.Instance.Surface.Updated += ActiveTrackerUpdated;
			return true;
		}
		protected override bool DisconnectInternal() {
			EquipmentMaster.Instance.Surface.Updated -= ActiveTrackerUpdated;
			var boxRoot = Hall.Container.GetWidget<Gtk.Box>("box_Tracking");
			var boxMain = Container.GetWidget("box_Root");
			boxRoot.Remove(boxMain);
			RemoveTrackers();
			var ntbRoot = Container.GetWidget<Gtk.Notebook>("ntb_Root");
			ntbRoot.SwitchPage -= ActiveTrackerSelected;
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
			settings.SetInteger(TrackerProperty.MinBrightness, 32);
			settings.SetInteger(TrackerProperty.Stream0, 0);
			settings.SetInteger(TrackerProperty.Smoothing, 4);
			settings.SetDecimal(TrackerProperty.SmoothingDropoff, 0.8);
			settings.SetInteger(TrackerProperty.MinClusterRadius, 2);
			settings.SetInteger(TrackerProperty.MaxClusterRadius, 32);
			settings.SetInteger(TrackerProperty.MinLayerEnergy, 6);
			settings.SetInteger(TrackerProperty.LabelNo, 3);
			settings.SetInteger(TrackerProperty.FastApproxThreshold, 200);
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
