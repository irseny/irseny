using System;
using Irseny.Content;
namespace Irseny.Viol.Main.Control.Output {
	public class VirtualKeyboardBindingsFactory : InterfaceFactory {
		readonly int index;
		public VirtualKeyboardBindingsFactory(int index) : base() {
			this.index = index;
		}
		protected override bool CreateInternal() {
			var factory = ContentMaster.Instance.Resources.InterfaceFactory.GetEntry("VirtualKeyboardBindings");
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
