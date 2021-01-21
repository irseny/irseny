// This file is part of Irseny.
//
// Copyright (C) 2021  Thilo Gabel
//
// Irseny is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
//
// Irseny is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
//
// You should have received a copy of the GNU General Public License
// along with this program.  If not, see <https://www.gnu.org/licenses/>.

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

