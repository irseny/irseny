using System;

namespace Irseny.Viol {
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
		/// Gets the widget container.
		/// </summary>
		/// <value>The widget container.</value>
		Mycena.IInterfaceNode Container { get; }
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
		/// Removes the inner factory identified by the given name from this instance.
		/// </summary>
		/// <returns>The inner factory destructed, null if the operation was not successful.</returns>
		/// <param name="name">Inner factory identifier.</param>
		IInterfaceFactory DestructFloor(string name);

	}
}

