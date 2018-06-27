using System;
using System.Collections.Generic;

namespace Irseny.Viol {
	public abstract class InterfaceFactory : IInterfaceFactory {
		Dictionary<string, IInterfaceFactory> innerFactories;

		public InterfaceFactory() {
			State = InterfaceFactoryState.Initial;
			innerFactories = new Dictionary<string, IInterfaceFactory>();
			Container = null;
			Hall = null;
		}
		public InterfaceFactoryState State { get; private set; }
		public IInterfaceFactory Hall { get; set; }
		public Mycena.IInterfaceNode Container { get; protected set; }
		public ICollection<IInterfaceFactory> Floors {
			get { return innerFactories.Values; }
		}

		public bool Create() {
			if (State == InterfaceFactoryState.Initial) {
				if (CreateInternal()) {
					State = InterfaceFactoryState.Created;
					foreach (InterfaceFactory inner in innerFactories.Values) {
						if (!inner.Create()) {
							return false;
						}
					}
					return true;
				}

			}
			return false;
		}
		public bool Connect() {
			if (State == InterfaceFactoryState.Created) {
				if (ConnectInternal()) {
					State = InterfaceFactoryState.Connected;
					foreach (InterfaceFactory inner in innerFactories.Values) {
						if (!inner.Connect()) {
							return false;
						}
					}
					return true;
				}
			}
			return false;
		}
		public bool Disconnect() {
			if (State == InterfaceFactoryState.Connected) {
				foreach (InterfaceFactory inner in innerFactories.Values) {
					if (!inner.Disconnect()) {
						return false;
					}
				}
				if (DisconnectInternal()) {
					State = InterfaceFactoryState.Created;
					return true;
				}
			}
			return false;
		}
		public bool Destroy() {
			if (State == InterfaceFactoryState.Created) {
				foreach (InterfaceFactory inner in innerFactories.Values) {
					if (!inner.Destroy()) {
						return false;
					}
				}
				if (DestroyInternal()) {
					State = InterfaceFactoryState.Initial;
					return true;
				}
			}
			return false;
		}
		public bool Init(InterfaceFactoryState state) {
			if (state > State) {
				do {
					bool result;
					switch (State) {
					case InterfaceFactoryState.Initial:
						result = Create();
					break;
					case InterfaceFactoryState.Created:
						result = Connect();
					break;
					default:
						result = false; // should not occur
					break;
					}
					if (!result) {
						return false;
					}
				} while (state > State);
			} else if (state < State) {
				do {
					bool result;
					switch (State) {
					case InterfaceFactoryState.Connected:
						result = Disconnect();
					break;
					case InterfaceFactoryState.Created:
						result = Destroy();
					break;
					default:
						result = false; // should not occur
					break;
					}
					if (!result) {
						return false;
					}
				} while (state < State);
			}
			return true; // already in correct state
		}
		public bool ConstructFloor(string name, IInterfaceFactory floor) {
			floor.Hall = this;
			if (!floor.Init(State)) {
				return false;
			}
			if (innerFactories.ContainsKey(name)) {
				return false;
			}
			innerFactories.Add(name, floor);
			return true;
		}
		public IInterfaceFactory DestructFloor(string name) {
			IInterfaceFactory factory;
			if (innerFactories.TryGetValue(name, out factory)) {
				if (!factory.Init(InterfaceFactoryState.Initial)) {
					return null;
				}
				if (!innerFactories.Remove(name)) {
					return null;
				}
				factory.Hall = null;
				return factory;
			} else {
				return null;
			}
		}

		public T GetFloor<T>(string name) where T : IInterfaceFactory {
			if (name == null) throw new ArgumentNullException("name");
			IInterfaceFactory result;
			if (innerFactories.TryGetValue(name, out result)) {
				if (result is T) {
					return (T)result;
				} else {
					throw new ArgumentException("type argument T");
				}
			} else {
				throw new KeyNotFoundException("name: " + name);
			}
		}
		public IInterfaceFactory GetFloor(string name) {
			return GetFloor<IInterfaceFactory>(name);
		}

		public bool TryGetFloor<T>(string name, out T result) where T : IInterfaceFactory {
			IInterfaceFactory factory;
			if (innerFactories.TryGetValue(name, out factory)) {
				if (factory is T) {
					result = (T)factory;
					return true;
				}
			}
			result = default(T);
			return false;
		}
		public void Invoke(EventHandler handler) {
			if (handler == null) throw new ArgumentNullException("handler");
			Gtk.Application.Invoke(handler);
		}
		public void Dispose() {
			Init(InterfaceFactoryState.Initial); // inner factories uninitialized
			foreach (IInterfaceFactory inner in innerFactories.Values) {
				inner.Dispose();
			}
			innerFactories.Clear();
			Container.Dispose();
		}
		protected void EnsureHallAvailable() {
			if (Hall == null) throw new InvalidOperationException("Hall not available");
		}
		protected abstract bool CreateInternal();
		protected abstract bool ConnectInternal();
		protected abstract bool DisconnectInternal();
		protected abstract bool DestroyInternal();
	}
}

