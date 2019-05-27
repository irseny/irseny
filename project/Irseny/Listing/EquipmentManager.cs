using System;
using System.Collections.Generic;
using System.Threading;

namespace Irseny.Listing {
	public class EquipmentManager<T> {
		readonly object equipmentSync = new object();
		readonly object updateSync = new object();
		readonly object updateEventSync = new object();
		Queue<EquipmentUpdateArgs<T>> updateQueue = new Queue<EquipmentUpdateArgs<T>>();
		List<Tuple<EquipmentState, T>> equipment = new List<Tuple<EquipmentState, T>>(32);

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
		private void ShareUpdates() {
			lock (updateSync) {
				while (updateQueue.Count > 0) {
					// to avoid order conflicts we need to stay in the locked region
					OnUpdated(updateQueue.Dequeue());
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
		/// <summary>
		/// Updates the equipment view and sends updated information to all subscribers on the calling thread.
		/// To avoid conflicts of order this method should only be called by a single thread.
		/// </summary>
		/// <param name="index">Index.</param>
		/// <param name="state">State.</param>
		/// <param name="equipment">Equipment.</param>
		public void Update(int index, EquipmentState state, T equipment) {
			if (index < 0) throw new ArgumentOutOfRangeException("index");
			lock (equipmentSync) {
				EquipmentState lastState;
				if (index >= this.equipment.Count) {
					lastState = EquipmentState.Missing;
					if (index >= this.equipment.Capacity) {
						this.equipment.Capacity = index + 1;
					}
					for (int i = this.equipment.Count; i <= index; i++) {
						this.equipment.Add(Tuple.Create(EquipmentState.Missing, default(T)));
					}

				} else {
					if (this.equipment[index] != null) {
						lastState = this.equipment[index].Item1;
					} else {
						lastState = EquipmentState.Missing;
					}
				}
				this.equipment[index] = Tuple.Create(state, equipment);
				// update queue lock
				lock (updateSync) {
					// to avoid order conflicts we can not leave the outer lock
					updateQueue.Enqueue(new EquipmentUpdateArgs<T>(index, state, lastState, equipment));
				}
			}
			// we can now leave the locked regions because updates have been enqueued
			// and can therefore no longer change order
			// keep in mind that this only works reliably if updates com from only one thread
			ShareUpdates();
		}
		/// <summary>
		/// Sends the current equipment view through the given event handler on the calling thread.
		/// To receive all updates objects should subscribe to equipment updates before calling this method.
		/// Subscribing methods may have an inconsistent view on equipment before this method terminates.
		/// After termination objects should have an equipment view that is consistent with the current view.
		/// Note that updates can still be sent through other threads which may result in double updates.
		/// </summary>
		/// <param name="handler">Event handler that the subscribing object receives updates through.</param>
		public void SendEquipment(EventHandler<EquipmentUpdateArgs<T>> handler) {
			if (handler == null) throw new ArgumentNullException("handler");
			var argsQueue = new Queue<EquipmentUpdateArgs<T>>(equipment.Count);
			lock (equipmentSync) {
				// collect the current view of the equipment
				for (int i = 0; i < equipment.Count; i++) {
					if (equipment[i] != null) {
						argsQueue.Enqueue(new EquipmentUpdateArgs<T>(i, equipment[i].Item1, equipment[i].Item1, equipment[i].Item2));
					}
				}
				// share the information before its state changes
				// there is no benefit from locking the update sync object
				// to avoid order conflicts we can not leave the lock
				while (argsQueue.Count > 0) {
					handler(this, argsQueue.Dequeue());
				}
			}
		}
	}
}

