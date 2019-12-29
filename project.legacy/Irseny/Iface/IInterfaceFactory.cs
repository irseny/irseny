using System;
using System.Collections.Generic;

namespace Irseny.Iface {
	public interface IInterfaceFactory : IDisposable {
		/// <summary>
		/// Gets or sets the outer factory.
		/// </summary>
		/// <value>The outer factory.</value>
		IInterfaceFactory Hall { get; set; }
		/// <summary>
		/// Gets the creation state of this instance.
		/// </summary>
		/// <value>The creation state.</value>
		InterfaceFactoryState State { get; }
		/// <summary>
		/// Indicates whether this instance has been fully initialized.
		/// This is the case if this instance is in <see cref="InterfaceFactoryState.Connected"/> state.
		/// </summary>
		/// <value><c>true</c> if fully initialized; otherwise, <c>false</c>.</value>
		bool Initialized { get; }
		/// <summary>
		/// Gets the widget container.
		/// </summary>
		/// <value>The widget container.</value>
		Mycena.IInterfaceNode Container { get; }
		/// <summary>
		/// Gets the floor names of this instance.
		/// </summary>
		/// <value>The floor names.</value>
		ICollection<string> FloorNames { get; }
		/// <summary>
		/// Gets the floors of this instance.
		/// </summary>
		/// <value>The floors.</value>
		ICollection<IInterfaceFactory> Floors { get; }
		/// <summary>
		/// Creates the widgets of this instance.
		/// </summary>
		bool Create();
		/// <summary>
		/// Connects the widgets of this instance.
		/// </summary>
		bool Connect();
		/// <summary>
		/// Disconnect the widgets of this instance.
		/// </summary>
		bool Disconnect();
		/// <summary>
		/// Destroys the widgets of this instance.
		/// </summary>
		bool Destroy();
		/// <summary>
		/// Brings this instance to the given creation state.
		/// </summary>
		/// <param name="state">Creation state.</param>
		bool Init(InterfaceFactoryState state);
		/// <summary>
		/// Adds the given inner factory to this instance.
		/// </summary>
		/// <returns><c>true</c>, if the operation was successful, <c>false</c> otherwise.</returns>
		/// <param name="name">Inner factory identifier.</param>
		/// <param name="floor">Inner factory.</param>
		bool ConstructFloor(string name, IInterfaceFactory floor);
		/// <summary>
		/// Gets the floor specified by the given identifier.
		/// </summary>
		/// <returns>The floor.</returns>
		/// <param name="name">Floor identifier.</param>
		IInterfaceFactory GetFloor(string name);
		/// <summary>
		/// Gets the floor specified by name.
		/// </summary>
		/// <returns>The floor.</returns>
		/// <param name="name">Floor identifier.</param>
		/// <typeparam name="T">Floor type.</typeparam>
		T GetFloor<T>(string name) where T : IInterfaceFactory;
		/// <summary>
		/// Gets the floor specified by the given identifier.
		/// </summary>
		/// <returns><c>true</c>, if the operation wass successful, <c>false</c> otherwise.</returns>
		/// <param name="name">Floor identifier.</param>
		/// <param name="result">Floor instance.</param>
		/// <typeparam name="T">Floor type.</typeparam>
		bool TryGetFloor<T>(string name, out T result) where T : IInterfaceFactory;
		/// <summary>
		/// Removes the inner factory identified by the given identifier.
		/// </summary>
		/// <returns>The inner factory destructed, null if the operation was not successful.</returns>
		/// <param name="name">Inner factory identifier.</param>
		IInterfaceFactory DestructFloor(string name);
		/// <summary>
		/// Invokes the given handler in the GUI thread.
		/// </summary>
		/// <param name="handler">Handler.</param>
		void Invoke(EventHandler handler);

	}
}

