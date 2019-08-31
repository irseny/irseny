using System;
using System.Threading;
using Irseny.Content;
using Irseny.Listing;

namespace Irseny.Iface.Main.Config {
	public class ConfigFactory : InterfaceFactory {
		readonly object pageLock = new object();

		public ConfigFactory() : base() {
		}
		protected override bool CreateInternal() {
			var factory = ContentMaster.Instance.Resources.InterfaceFactory.GetEntry("Config");
			Container = factory.CreateWidget("ntb_Root");
			return true;
		}
		protected override bool ConnectInternal() {
			var boxRoot = Hall.Container.GetWidget<Gtk.Box>("box_Control");
			var ntbRoot = Container.GetWidget<Gtk.Notebook>("ntb_Root");
			boxRoot.PackStart(ntbRoot, true, true, 0);
			ntbRoot.SwitchPage += ActivePageSelected;
			EquipmentMaster.Instance.Surface.Updated += ActivePageUpdated;
			return true;
		}
		protected override bool DisconnectInternal() {
			EquipmentMaster.Instance.Surface.Updated -= ActivePageUpdated;
			var boxRoot = Hall.Container.GetWidget<Gtk.Box>("box_Control");
			var ntbRoot = Container.GetWidget<Gtk.Notebook>("ntb_Root");
			ntbRoot.SwitchPage -= ActivePageSelected;
			boxRoot.Remove(ntbRoot);
			return true;
		}
		protected override bool DestroyInternal() {
			Container.Dispose();
			return true;
		}
		private void ActivePageSelected(object sender, EventArgs args) {
			if (!Initialized) {
				return;
			}
			if (Monitor.IsEntered(pageLock)) {
				return;
			}
			var ntbRoot = (Gtk.Notebook)sender;
			Gtk.Widget page = ntbRoot.CurrentPageWidget;
			string title = ntbRoot.GetTabLabelText(page);
			int pageId;
			if (title.StartsWith("Camera")) {
				pageId = SurfacePage.CameraPage;
			} else if (title.StartsWith("Tracking")) {
				pageId =  SurfacePage.TrackingPage;
			} else if (title.StartsWith("Device")) {
				pageId =  SurfacePage.DevicePage;
			} else if (title.StartsWith("Profile")) {
				pageId =  SurfacePage.ProfilePage;
			} else if (title.StartsWith("Settings")) {
				pageId =  SurfacePage.SettingsPage;
			} else if (title.StartsWith("Model")) {
				pageId =  SurfacePage.ModelPage;
			} else if (title.StartsWith("Binding")) {
				pageId =  SurfacePage.BindingsPage;
			} else {
				return;
			}
			lock (pageLock) {
				EquipmentMaster.Instance.Surface.Update(SurfacePage.ActivePage, EquipmentState.Active, pageId);
			}
		}
		private void ActivePageUpdated(object sender, EquipmentUpdateArgs<int> args) {
			// do not change if the update is sent from this instance
			if (Monitor.IsEntered(pageLock)) {
				return;
			}
			if (args.Index != SurfacePage.ActivePage) {
				return;
			}
			if (!args.Active) {
				return;
			}
			// determine a page title
			int pageId = args.Equipment;
			string targetTitle;
			switch (pageId) {
			case SurfacePage.CameraPage:
				targetTitle = "Camera";
				break;
			case SurfacePage.TrackingPage:
				targetTitle = "Tracking";
				break;
			case SurfacePage.DevicePage:
			case SurfacePage.BindingsPage:
				targetTitle = "Device";
				break;
			case SurfacePage.ProfilePage:
				targetTitle = "Profile";
				break;
			case SurfacePage.SettingsPage:
				targetTitle = "Settings";
				break;
			default:
				return;
			}
			// activate the page
			Invoke(delegate {
				if (!Initialized) {
					return;
				}
				var ntbRoot = Container.GetWidget<Gtk.Notebook>("ntb_Root");
				int pageNo = ntbRoot.NPages;
				for (int p = 0; p < pageNo; p++) {
					Gtk.Widget page = ntbRoot.GetNthPage(p);
					string title = ntbRoot.GetTabLabelText(page);
					if (title.StartsWith(targetTitle) && ntbRoot.CurrentPage != p) {
						lock (pageLock) {
							ntbRoot.CurrentPage = p;
						}
					}
				}
			});
		}
	}
}

