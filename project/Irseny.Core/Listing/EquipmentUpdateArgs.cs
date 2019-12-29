using System;

namespace Irseny.Core.Listing {
	public class EquipmentUpdateArgs<T> : EventArgs {
		public EquipmentUpdateArgs(int index, EquipmentState currentState, EquipmentState previousState, T value) {
			Index = index;
			CurrentState = currentState;
			PreviousState = previousState;
			Equipment = value;
		}
		public int Index { get; private set; }
		public EquipmentState CurrentState { get; private set; }
		public EquipmentState PreviousState { get; private set; }
		public T Equipment { get; private set; }
		public bool StateChanged {
			get { return CurrentState != PreviousState; }
		}
		public bool Missing {
			get { return CurrentState == EquipmentState.Missing; }
		}
		public bool Active {
			get { return CurrentState == EquipmentState.Active; }
		}
		public bool Passive {
			get { return CurrentState == EquipmentState.Passive; }
		}

	}
}

