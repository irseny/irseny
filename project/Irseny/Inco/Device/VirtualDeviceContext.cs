using System;

namespace Irseny.Inco.Device {
	public class VirtualDeviceContext {
		IntPtr contextHandle = IntPtr.Zero;


		public VirtualDeviceContext() {
		}
		public bool Created {
			get { return contextHandle != IntPtr.Zero; }
		}
		public bool Create() {

			return true;
		}
		public bool Destroy() {

			return true;
		}
		// TODO: create context, add/remove devices
	}
}

