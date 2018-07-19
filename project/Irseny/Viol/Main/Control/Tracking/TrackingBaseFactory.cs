﻿using System;

namespace Irseny.Viol.Main.Control.Tracking {
	public class TrackingBaseFactory : InterfaceFactory {
		public TrackingBaseFactory() : base() {
		}
		protected override bool CreateInternal() {
			var factory = Mycena.InterfaceFactory.CreateFromFile(Content.ContentMaster.Instance.Resources.InterfaceDefinitions.GetEntry("TrackingControlBase"));
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
				RemoveTracker();
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
		public bool AddTracker() {
			var ntbTracker = Container.GetWidget<Gtk.Notebook>("ntb_Tracker");
			int page = ntbTracker.NPages;
			// create and append
			if (page < 10) {
				var factory = new TrackingFactory(page);
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
		public bool RemoveTracker() {
			var ntbTracker = Container.GetWidget<Gtk.Notebook>("ntb_Tracker");
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
	}
}
