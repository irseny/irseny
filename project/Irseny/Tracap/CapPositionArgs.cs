using System;
namespace Irseny.Tracap {
	public class CapPositionArgs : EventArgs {
		public CapPositionArgs(CapPosition position) : base() {
		}
		public CapPosition Position { get; private set; }
	}
}
