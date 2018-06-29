using System;

namespace Irseny.Listing {
	public class EquipmentUpdateArgs<T> : EventArgs {
		public EquipmentUpdateArgs(int index, bool available, T value) {
			IndexChanged = index;
			Available = available;
			Equipment = value;
		}
		public int IndexChanged { get; private set; }
		public bool Available { get; private set; }
		public T Equipment { get; private set; }
	}
}

