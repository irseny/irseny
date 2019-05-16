using System;
using Irseny.Content;

namespace Irseny.Iface.Main.View.Bindings {
	public class BindingTabFactory : InterfaceFactory {
		public BindingTabFactory() : base() {
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
	}
}
