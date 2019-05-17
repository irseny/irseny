using System;
using System.Collections.Generic;
using Irseny.Content;
using Irseny.Tracap;
using Irseny.Inco.Device;

namespace Irseny.Iface.Main.View.Bindings {
	public class BindingTabFactory : InterfaceFactory {
		int trackerIndex;

		CapAxis activeTrackerAxis = CapAxis.Yaw;
		int activeDeviceIndex = -1;
		VirtualDeviceCapability activeDeviceCapability = VirtualDeviceCapability.Axis;
		object activeDeviceKeyHandle = null;
		string activeDeviceKeyDescription = string.Empty;
		object activeAxisTranslation = null;

		List<CapAxis> trackerAxes = new List<CapAxis>();
		List<int> deviceIndexes = new List<int>();
		List<VirtualDeviceCapability> deviceCapabilities = new List<VirtualDeviceCapability>();
		List<object> deviceKeyHandles = new List<object>();
		List<object> axisTranslations = new List<object>();


		public BindingTabFactory(int trackerIndex) : base() {
			this.trackerIndex = trackerIndex;
		}
		protected override bool CreateInternal() {
			var factory = ContentMaster.Instance.Resources.InterfaceFactory.GetEntry("BindingTab");
			Container = factory.CreateWidget("box_Root");
			return true;
		}
		protected override bool ConnectInternal() {
			var boxParent = Hall.Container.GetWidget<Gtk.Box>("box_Binding");
			var boxRoot = Container.GetWidget("box_Root");
			boxParent.PackStart(boxRoot, true, true, 0);
			var drwGraph = Container.GetWidget<Gtk.DrawingArea>("drw_Graph");

			// TODO: listen to device creation and removal
			return true;
		}
		protected override bool DisconnectInternal() {
			var boxParent = Hall.Container.GetWidget<Gtk.Box>("box_Binding");
			var boxRoot = Container.GetWidget("box_Root");
			boxParent.Remove(boxRoot);
			return true;
		}
		protected override bool DestroyInternal() {
			Container.Dispose();
			return true;
		}
		public void RestoreBinding(CapAxis axis) {
			var expRoot = Container.GetWidget<Gtk.Expander>("exp_Binding");
			expRoot.Expanded = true;
			expRoot.Sensitive = true;
		}
		public void Hide() {
			var expRoot = Container.GetWidget<Gtk.Expander>("exp_Binding");
			expRoot.Expanded = false;
			expRoot.Sensitive = false;
		}
		public void ApplyBinding() {

		}
	}
}
