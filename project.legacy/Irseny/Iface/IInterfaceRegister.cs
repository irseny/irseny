using System;
namespace Irseny.Iface {
	public interface IInterfaceRegister {

		bool Register(string name, IInterfaceFactory factory);
		IInterfaceFactory Unregister(string name);

		T GetFactory<T>(string name) where T : IInterfaceFactory;
		IInterfaceFactory GetFactory(string name);
		bool TryGetFactory<T>(string name, out T result) where T : IInterfaceFactory;
	}
}
