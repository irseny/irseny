﻿using System;
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
			var ntbRoot = (Gtk.Notebook)sender;
			int page = ntbRoot.CurrentPage;
			lock (pageLock) {
				EquipmentMaster.Instance.Surface.Update(SurfaceProperty.ActivePage, EquipmentState.Active, page);
			}
		}
		private void ActivePageUpdated(object sender, EquipmentUpdateArgs<int> args) {
			if (Monitor.IsEntered(pageLock)) {
				// do not change if the update message is sent from this instance
				return;
			}
			if (args.Index != SurfaceProperty.ActivePage) {
				return;
			}
			if (!args.Active) {
				return;
			}
			int page = args.Equipment;
			Invoke(delegate {
				if (!Initialized) {
					return;
				}
				var ntbRoot = Container.GetWidget<Gtk.Notebook>("ntb_Root");
				if (ntbRoot.CurrentPage != page) {
					ntbRoot.CurrentPage = page;
				}
			});
		}
	}
}

