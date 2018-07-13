using System;
namespace Irseny.Tracap {
	public class PositionDetectedEventArgs : EventArgs {

		public PositionDetectedEventArgs(CapPosition position) : base() {
			Position = position;
		}
		public CapPosition Position { get; private set; }
	}
}
