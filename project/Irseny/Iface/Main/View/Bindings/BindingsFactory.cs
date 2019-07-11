using System;
using System.Collections.Generic;
using System.Diagnostics;
using Irseny.Content;
using Irseny.Listing;
using Irseny.Tracap;
using Irseny.Inco.Device;

namespace Irseny.Iface.Main.View.Bindings {
	public class BindingsFactory : InterfaceFactory {
		public BindingsFactory() : base() {
		}
		public IReadOnlyDictionary<int, CapInputRelay> GetConfig() {
			var result = new Dictionary<int, CapInputRelay>();
			foreach (var floor in Floors) {
				var factory = floor.GetFloor<BindingTabFactory>("Binding");
				result.Add(factory.TrackerIndex, factory.GetConfig());
			}
			return result;
		}
		public bool ApplyConfig(IReadOnlyDictionary<int, CapInputRelay> config) {
			if (config == null) throw new ArgumentNullException("config");
			bool result = true;
			foreach (var floor in Floors) {
				var viewFactory = (CapBindingsFactory)floor;
				viewFactory.HideBindings();
				var bindFactory = floor.GetFloor<BindingTabFactory>("Binding");
				CapInputRelay conf;
				if (config.TryGetValue(bindFactory.TrackerIndex, out conf)) {
					bindFactory.ApplyConfig(conf);
				} else {
					result = false;
				}
			}
			return result;
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
			while (RemoveTracker()) {
			}
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
			var factory = new CapBindingsFactory(page);
			ConstructFloor("Track" + page, factory);
			var bindingFactory = new BindingTabFactory(page);
			factory.ConstructFloor("Binding", bindingFactory);
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
