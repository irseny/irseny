using System;
using System.Collections.Generic;
using System.Threading;

namespace Irseny.Listing {
	public class EquipmentManager<T> {
		object equipmentSync = new object();
		List<Tuple<bool, T>> equipment = new List<Tuple<bool,T>>(32);
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
		public bool Available(int index) {
			lock (equipmentSync) {
				if (index < 0 || index >= equipment.Count) {
					return false;
				} else {
					return equipment[index].Item1;
				}
			}
		}
		public TE GetEquipment<TE>(int index, TE defaultValue) where TE : T {
			lock (equipmentSync) {
				if (index < 0 || index >= equipment.Count || !equipment[index].Item1) {
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
				if (index >= 0 && index < equipment.Count && equipment[index].Item1 && equipment[index].Item2 is TE) {
					result = (TE)equipment[index].Item2;
					return true;
				} 
			}
			result = default(TE);
			return false;
		}
		public void Update(int index, bool available, T equipment) {
			if (index < 0) throw new ArgumentOutOfRangeException("index");
			lock (equipmentSync) {
				if (index >= this.equipment.Count) {
					if (index >= this.equipment.Capacity) {
						this.equipment.Capacity = index + 1;
					}
					for (int i = this.equipment.Count; i <= index; i++) {
						this.equipment.Add(new Tuple<bool, T>(false, default(T)));
					}
				}
				this.equipment[index] = new Tuple<bool, T>(available, equipment);
			}
			OnUpdated(new EquipmentUpdateArgs<T>(index, available, equipment));
		}
	}
}

