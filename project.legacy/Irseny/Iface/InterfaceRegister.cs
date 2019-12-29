using System;
using System.Collections.Generic;

namespace Irseny.Iface {
	public class InterfaceRegister : IInterfaceRegister {
		Dictionary<string, IInterfaceFactory> factories = new Dictionary<string, IInterfaceFactory>(32);

		public InterfaceRegister() {
		}
		public bool Register(string name, IInterfaceFactory factory) {
			if (factories.ContainsKey(name)) {
				return false;
			}
			factories.Add(name, factory);
			return true;
		}
		public IInterfaceFactory Unregister(string name) {
			IInterfaceFactory factory;
			if (factories.TryGetValue(name, out factory)) {
				if (!factories.Remove(name)) {
					return null;
				}
				return factory;
			} else {
				return null;
			}
		}

		public T GetFactory<T>(string name) where T : IInterfaceFactory {
			if (name == null) throw new ArgumentNullException("name");
			IInterfaceFactory result;
			if (factories.TryGetValue(name, out result)) {
				if (result is T) {
					return (T)result;
				} else {
					throw new ArgumentException("type argument T");
				}
			} else {
				throw new KeyNotFoundException("name: " + name);
			}
		}
		public IInterfaceFactory GetFactory(string name) {
			return GetFactory<IInterfaceFactory>(name);
		}

		public bool TryGetFactory<T>(string name, out T result) where T : IInterfaceFactory {
			IInterfaceFactory factory;
			if (factories.TryGetValue(name, out factory)) {
				if (factory is T) {
					result = (T)factory;
					return true;
				}
			}
			result = default(T);
			return false;
		}
	}
}
