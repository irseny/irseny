using System;
using System.Threading;
using Irseny.Content;
using Irseny.Listing;
namespace Irseny.Iface.Main.View {
	public class ViewFactory : InterfaceFactory {
		readonly object pageLock = new object();

		public ViewFactory() : base() {
		}
		protected override bool CreateInternal() {
			var factory = ContentMaster.Instance.Resources.InterfaceFactory.GetEntry("View");
			Container = factory.CreateWidget("ntb_Root");
			return true;
		}
		protected override bool ConnectInternal() {
			var boxRoot = Hall.Container.GetWidget<Gtk.Box>("box_Display");
			var ntbRoot = Container.GetWidget<Gtk.Notebook>("ntb_Root");
			ntbRoot.SwitchPage += ActivePageSelected;
			boxRoot.PackStart(ntbRoot, true, true, 0);
			EquipmentMaster.Instance.Surface.Updated += ActivePageUpdated;
			return true;
		}
		protected override bool DisconnectInternal() {
			EquipmentMaster.Instance.Surface.Updated -= ActivePageUpdated;
			var boxRoot = Hall.Container.GetWidget<Gtk.Box>("box_Display");
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
			if (Monitor.IsEntered(pageLock)) {
				// do not change if the update message is sent from this instance
				return;
			}
			if (args.Index != SurfacePage.ActivePage) {
				return;
			}
			if (!args.Active) {
				return;
			}
			int pageId = args.Equipment;
			string targetTitle;
			switch (pageId) {
			case SurfacePage.CameraPage:
				targetTitle = "Camera";
				break;
			case SurfacePage.TrackingPage:
				targetTitle = "Tracking";
				break;
			case SurfacePage.BindingsPage:
			case SurfacePage.DevicePage:
				targetTitle = "Bindings";
				break;
			default:
				return;
			}
			Invoke(delegate {
				if (!Initialized) {
					return;
				}
				var ntbRoot = Container.GetWidget<Gtk.Notebook>("ntb_Root");
				int pageNo = ntbRoot.NPages;
				for (int i = 0; i < pageNo; i++) {
					Gtk.Widget page = ntbRoot.GetNthPage(i);
					string title = ntbRoot.GetTabLabelText(page);
					if (title.StartsWith(targetTitle) && ntbRoot.CurrentPage != i) {
						lock (pageLock) {
							ntbRoot.CurrentPage = i;
						}
						break;
					}
				}
			});
		}
	}
}

