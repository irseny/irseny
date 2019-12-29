using System;
using Irseny.Content;

namespace Irseny.Iface {
	public class MainFactory : InterfaceFactory {
		public MainFactory() : base() {
		}
		protected override bool CreateInternal() {
			Gtk.Application.Init();
			var factory = ContentMaster.Instance.Resources.InterfaceFactory.GetEntry("Main");
			Container = factory.CreateWidget("win_Main");
			return true;
		}
		protected override bool ConnectInternal() {
			var window = Container.GetWidget<Gtk.Window>("win_Main");
			window.DeleteEvent += delegate {
				Init(InterfaceFactoryState.Initial);
			};
			return true;
		}
		protected override bool DisconnectInternal() {
			// nothing to do
			return true;
		}
		protected override bool DestroyInternal() {
			Container.Dispose();
			return true;
		}

	}
}

