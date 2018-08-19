using System;
namespace Irseny.Viol.Main.Control.Output {
	public class VirtualKeyboardBindingsFactory : InterfaceFactory {
		public VirtualKeyboardBindingsFactory() {
		}
		protected override bool CreateInternal() {
			return false;
		}
		protected override bool ConnectInternal() {
			throw new NotImplementedException();
		}
		protected override bool DisconnectInternal() {
			throw new NotImplementedException();
		}
		protected override bool DestroyInternal() {
			throw new NotImplementedException();
		}
	}
}
