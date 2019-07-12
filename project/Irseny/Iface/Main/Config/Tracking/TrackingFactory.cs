using System;
using System.Collections.Generic;
using Irseny.Content;
using Irseny.Tracap;

namespace Irseny.Iface.Main.Config.Tracking {
	public class TrackingFactory : InterfaceFactory {
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
				AddTracker();
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
		public bool AddTracker(int index, ICapTrackerOptions options) {
			if (index < 0) throw new ArgumentOutOfRangeException("index");
			if (options == null) throw new ArgumentNullException("options");
			//if (!(options is Cap3PointOptions)) throw new ArgumentException("options");
			// TODO: add the tracker
			return true;
		}
		private bool AddTracker() {
			var ntbTracker = Container.GetWidget<Gtk.Notebook>("ntb_Root");
			int page = ntbTracker.NPages;
			// create and append
			if (page < 10) {
				var factory = new CapTrackingFactory(page, new CapTrackerOptions());
				ConstructFloor(string.Format("Track{0}", page), factory);
				var boxInner = factory.Container.GetWidget("box_Root");
				var label = new Gtk.Label(string.Format("Track{0}", page));
				factory.Container.AddWidget(label);
				ntbTracker.AppendPage(boxInner, label);
				ntbTracker.ShowAll();

				return true;
			} else {
				return false;
			}
		}
		private bool RemoveSelectedTracker() {
			var ntbTracker = Container.GetWidget<Gtk.Notebook>("ntb_Root");
			int page = ntbTracker.NPages - 1;
			// remove last
			if (page > -1) {
				ntbTracker.RemovePage(page);
				IInterfaceFactory floor = DestructFloor(string.Format("Track{0}", page));
				floor.Dispose();
				return true;
			} else {
				return false;
			}
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
		public ICapTrackerOptions GetTrackerOptions(int index) {
			if (!Initialized) {
				return null;
			}
			foreach (IInterfaceFactory floor in Floors) {
				var factory = (CapTrackingFactory)floor;
				if (factory.TrackerIndex == index) {
					return factory.GetOptions();
				}
			}
			return null;
		}
	}
}
