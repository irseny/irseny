using System;

namespace Mycena {
	public interface IInterfaceRoot : IDisposable {
		/// <summary>
		/// Gets the <see cref="Gtk.Widget"/> with the specified name.
		/// </summary>
		/// <param name="name">Name used to store the widget.</param>
		Gtk.Widget this[string name] { get; }
		/// <summary>
		/// Gets the <see cref="Gtk.Widget"/> with the specified name or 
		/// the given default value if impossible.
		/// </summary>
		/// <param name="name">Name used to store the widget.</param>
		/// <param name="defaultValue">Default return value.</param>
		Gtk.Widget this[string name, Gtk.Widget defaultValue] { get; }
		/// <summary>
		/// Gets the widget with the specified name.
		/// </summary>
		/// <returns>The widget.</returns>
		/// <param name="name">Name used to store the widget.</param>
		Gtk.Widget GetWidget(string name);
		/// <summary>
		/// Gets the widget with the specified name or
		/// the given default value if impossible.
		/// </summary>
		/// <returns>The widget.</returns>
		/// <param name="name">Name used to store the widget.</param>
		/// <param name="defaultValue">Default return value.</param>
		Gtk.Widget GetWidget(string name, Gtk.Widget defaultValue);
		/// <summary>
		/// Gets the widget with the specified name.
		/// </summary>
		/// <returns>The widget.</returns>
		/// <param name="name">Name used to store the widget.</param>
		/// <typeparam name="T">Widget type.</typeparam>
		T GetWidget<T>(string name) where T : Gtk.Widget;
		/// <summary>
		/// Gets the widget with the specified name or
		/// the given default value if impossible.
		/// </summary>
		/// <returns>The widget.</returns>
		/// <param name="name">Name used to store the widget.</param>
		/// <param name="defaultValue">Default return value.</param>
		/// <typeparam name="T">Widget type.</typeparam>
		T GetWidget<T>(string name, T defaultValue) where T : Gtk.Widget;
		/// <summary>
		/// Tries to get the widget with the specified name.
		/// </summary>
		/// <returns><c>true</c>, if result contains a valid widget <c>false</c> otherwise.</returns>
		/// <param name="name">Name used to store the widget.</param>
		/// <param name="result">Whether the operation was succesful.</param>
		/// <typeparam name="T">Widget type.</typeparam>
		bool TryGetWidget<T>(string name, out T result) where T : Gtk.Widget;
	}
}

