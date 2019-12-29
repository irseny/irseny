using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Threading;
using Irseny.Content;
using Irseny.Listing;
using Irseny.Util;

namespace Irseny.Iface.Main.View.Camera {
	public class CameraFactory : InterfaceFactory {
		const string TitlePrefix = "Cam";
		const string FloorPrefix = "Camera";

		readonly object webcamLock = new object();

		public CameraFactory() : base() {
		}
		protected override bool CreateInternal() {
			var factory = ContentMaster.Instance.Resources.InterfaceFactory.GetEntry("CameraView");
			Container = factory.CreateWidget("ntb_Root");
			return true;
		}
		protected override bool ConnectInternal() {
			var boxRoot = Hall.Container.GetWidget<Gtk.Box>("box_Camera");
			var ntbRoot = Container.GetWidget<Gtk.Notebook>("ntb_Root");
			ntbRoot.SwitchPage += ActiveCameraSelected;
			boxRoot.PackStart(ntbRoot, true, true, 0);
			EquipmentMaster.Instance.VideoCaptureStream.Updated += CameraChanged;
			EquipmentMaster.Instance.Surface.Updated += ActiveCameraUpdated;
			return true;
		}
		protected override bool DisconnectInternal() {
			EquipmentMaster.Instance.VideoCaptureStream.Updated -= CameraChanged;
			EquipmentMaster.Instance.Surface.Updated -= ActiveCameraUpdated;
			var boxRoot = Hall.Container.GetWidget<Gtk.Box>("box_Camera");
			var ntbRoot = Container.GetWidget<Gtk.Notebook>("ntb_Root");
			ntbRoot.SwitchPage -= ActiveCameraSelected;
			boxRoot.Remove(ntbRoot);
			RemoveCameras();
			return true;
		}
		protected override bool DestroyInternal() {
			Container.Dispose();
			return true;
		}
		private void CameraChanged(object sender, Listing.EquipmentUpdateArgs<int> args) {
			Invoke(delegate {
				if (!Initialized) {
					return;
				}
				var ntbCamera = Container.GetWidget<Gtk.Notebook>("ntb_Root");
				int pages = ntbCamera.NPages;
				if (args.Missing) {
					if (args.Index == pages - 1) {
						RemoveCamera();
					} else {
						Debug.WriteLine(this.GetType().Name + ": Camera modified out of order: " + args.Index);
					}

				} else { // can occur multiple times (passive/active) -> debug message
					if (args.Index == pages) {
						AddCamera();
					} else {
						Debug.WriteLine(this.GetType().Name + ": Camera modified out of order: " + args.Index);
					}
				}
			});
		}
		public bool AddCamera() {
			var ntbCamera = Container.GetWidget<Gtk.Notebook>("ntb_Root");
			int page = ntbCamera.NPages;
			var factory = new Irseny.Iface.Main.View.Camera.WebcamFactory(page);
			ConstructFloor(FloorPrefix + page, factory);
			var boxInner = factory.Container.GetWidget("box_Root");
			var label = new Gtk.Label(TitlePrefix + page);
			factory.Container.AddWidget(label);
			ntbCamera.AppendPage(boxInner, label);
			ntbCamera.ShowAll();
			return true;
		}
		public bool RemoveCamera() {
			var ntbCamera = Container.GetWidget<Gtk.Notebook>("ntb_Root");
			int page = ntbCamera.NPages - 1;
			if (page > -1) {
				ntbCamera.RemovePage(page);
				IInterfaceFactory floor = DestructFloor(string.Format("Camera{0}", page));
				floor.Dispose();
				return true;
			}
			return false;
		}
		public void RemoveCameras() {
			var ntbCamera = Container.GetWidget<Gtk.Notebook>("ntb_Root");
			while (ntbCamera.NPages > 0) {
				Gtk.Widget page = ntbCamera.GetNthPage(0);
				Gtk.Widget label = ntbCamera.GetTabLabel(page);
				ntbCamera.RemovePage(0);
				label.Dispose();
			}
			var floorNames = new List<string>(FloorNames);
			foreach (string name in floorNames) {
				IInterfaceFactory floor = DestructFloor(name);
				floor.Dispose();
			}
		}
		private void ActiveCameraSelected(object sender, EventArgs args) {
			if (!Initialized) {
				return;
			}
			if (Monitor.IsEntered(webcamLock)) {
				return;
			}
			var ntbRoot = (Gtk.Notebook)sender;
			Gtk.Widget page = ntbRoot.CurrentPageWidget;
			string title = ntbRoot.GetTabLabelText(page);
			int iCamera = TextParseTools.ParseInt(title.Substring(TitlePrefix.Length), -1);
			if (iCamera < 0) {
				return;
			}
			lock (webcamLock) {
				EquipmentMaster.Instance.Surface.Update(SurfacePage.ActiveCamera, EquipmentState.Active, iCamera);
			}
		}
		private void ActiveCameraUpdated(object sender, EquipmentUpdateArgs<int> args) {
			if (args.Index != SurfacePage.ActiveCamera) {
				return;
			}
			if (!args.Active) {
				return;
			}
			int iCamera = args.Equipment;
			if (iCamera < 0) {
				return;
			}
			if (Monitor.IsEntered(webcamLock)) {
				return;
			}
			Invoke(delegate {
				if (!Initialized) {
					return;
				}
				string targetTitle = TitlePrefix + iCamera;
				var ntbRoot = Container.GetWidget<Gtk.Notebook>("ntb_Root");
				int pageNo = ntbRoot.NPages;
				for (int p = 0; p < pageNo; p++) {
					Gtk.Widget page = ntbRoot.GetNthPage(p);
					string title = ntbRoot.GetTabLabelText(page);
					if (title.Equals(targetTitle)) {
						if (ntbRoot.CurrentPage != p) {
							lock (webcamLock) {
								ntbRoot.CurrentPage = p;
							}
						}
					}
				}
			});
		}
	}
}

