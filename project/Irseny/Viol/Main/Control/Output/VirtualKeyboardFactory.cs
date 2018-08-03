using System;
using Irseny.Content;

namespace Irseny.Viol.Main.Control.Output {
	public class VirtualKeyboardFactory : InterfaceFactory {
		public VirtualKeyboardFactory() : base() {
		}
		protected override bool CreateInternal() {
			var factory = ContentMaster.Instance.Resources.InterfaceFactory.GetEntry("VirtualKeyboardControl");
			Container = factory.CreateWidget("box_Root");
			return true;
		}
		protected override bool ConnectInternal() {			
			return true;
		}
		protected override bool DisconnectInternal() {
			return true;
		}
		protected override bool DestroyInternal() {
			Container.Dispose();
			return true;
		}
	}
}

