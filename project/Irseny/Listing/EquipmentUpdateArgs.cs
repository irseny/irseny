using System;

namespace Irseny.Listing {
	public class EquipmentUpdateArgs<T> : EventArgs {
		public EquipmentUpdateArgs(int index, EquipmentState state, T value) {
			Index = index;
			State = state;
			Equipment = value;
		}
		public int Index { get; private set; }
		public EquipmentState State { get; private set; }
		public T Equipment { get; private set; }
		public bool Available {
			get { return State != EquipmentState.Missing; }
		}
		public bool Missing {
			get { return State == EquipmentState.Missing; }
		}
		public bool Active {
			get { return State == EquipmentState.Active; }
		}
		public bool Passive {
			get { return State == EquipmentState.Passive; }
		}

	}
}

