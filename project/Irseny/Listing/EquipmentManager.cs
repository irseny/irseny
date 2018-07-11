using System;
using System.Collections.Generic;
using System.Threading;

namespace Irseny.Listing {
	public class EquipmentManager<T> {
		object equipmentSync = new object();
		List<Tuple<EquipmentState, T>> equipment = new List<Tuple<EquipmentState,T>>(32);
		object updateEventSync = new object();
		event EventHandler<EquipmentUpdateArgs<T>> updated;
		public EquipmentManager() {
		}
		public event EventHandler<EquipmentUpdateArgs<T>> Updated {
			add {
				lock (updateEventSync) {
					updated += value;
				}
			}
			remove {
				lock (updateEventSync) {
					updated -= value;
				}
			}
		}

		protected void OnUpdated(EquipmentUpdateArgs<T> args) {
			EventHandler<EquipmentUpdateArgs<T>> handler;
			lock (updateEventSync) {
				handler = updated;
			}
			if (handler != null) {
				handler(this, args);
			}
		}
		public EquipmentState GetState(int index) {
			lock (equipmentSync) {
				if (index < 0 || index >= equipment.Count) {
					return EquipmentState.Missing;
				} else {
					return equipment[index].Item1;
				}
			}
		}
		public TE GetEquipment<TE>(int index, TE defaultValue) where TE : T {
			lock (equipmentSync) {
				if (index < 0 || index >= equipment.Count || equipment[index].Item1 == EquipmentState.Missing) {
					return defaultValue;
				} else {
					if (equipment[index].Item2 is TE) {
						return (TE)equipment[index].Item2;
					} else {
						return defaultValue;
					}
				}
			}
		}
		public T GetEquipment(int index, T defaultValue) {
			return GetEquipment<T>(index, defaultValue);
		}
		public bool TryGetEquipment<TE>(int index, out TE result) where TE : T {
			lock (equipmentSync) {
				if (index >= 0 && index < equipment.Count && equipment[index].Item1 != EquipmentState.Missing && equipment[index].Item2 is TE) {
					result = (TE)equipment[index].Item2;
					return true;
				} 
			}
			result = default(TE);
			return false;
		}
		public void Update(int index, EquipmentState state, T equipment) {
			if (index < 0) throw new ArgumentOutOfRangeException("index");
			lock (equipmentSync) {
				if (index >= this.equipment.Count) {
					if (index >= this.equipment.Capacity) {
						this.equipment.Capacity = index + 1;
					}
					for (int i = this.equipment.Count; i <= index; i++) {
						this.equipment.Add(Tuple.Create(EquipmentState.Missing, default(T)));
					}
				}
				this.equipment[index] = Tuple.Create(state, equipment);
			}
			OnUpdated(new EquipmentUpdateArgs<T>(index, state, equipment));
		}
	}
}

