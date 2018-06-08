using System;
using System.Xml;
using System.Collections.Generic;

namespace Mycena {
	

	public interface IInterfaceNode : IDisposable {
		#region Widgets
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
		/// <param name="result">Widget target.</param>
		/// <typeparam name="T">Widget type.</typeparam>
		bool TryGetWidget<T>(string name, out T result) where T : Gtk.Widget;
		/// <summary>
		/// Stores the given widget.
		/// </summary>
		/// <param name="name">Unique widget name.</param>
		/// <param name="widget">Widget to store.</param>
		void RegisterWidget(string name, Gtk.Widget widget);
		#endregion

		#region Gadgets
		/// <summary>
		/// Gets the gadget specified by name.
		/// </summary>
		/// <returns>The gadget.</returns>
		/// <param name="name">Name used to store the gadget.</param>
		GLib.Object GetGadget(string name);
		/// <summary>
		/// Gets the gadget specified by name or
		/// the given default value if impossible.
		/// </summary>
		/// <returns>The gadget.</returns>
		/// <param name="name">Name used to store the gadget.</param>
		/// <param name="defaultValue">Default return value.</param>
		GLib.Object GetGadget(string name, GLib.Object defaultValue);
		/// <summary>
		/// Gets the gadget with the specified name.
		/// </summary>
		/// <returns>The gadget.</returns>
		/// <param name="name">Name used to store the gadget.</param>
		/// <typeparam name="T">Gadget type.</typeparam>
		T GetGadget<T>(string name) where T : GLib.Object;
		/// <summary>
		/// Gets the gadget with the specified name or
		/// the given default value if impossible.
		/// </summary>
		/// <returns>The gadget.</returns>
		/// <param name="name">Name used to store the gadget.</param>
		/// <param name="defaultValue">Default return value.</param>
		/// <typeparam name="T">Gadget type.</typeparam>
		T GetGadget<T>(string name, T defaultValue) where T : GLib.Object;
		/// <summary>
		/// Tries to get the the gadget specified by name.
		/// </summary>
		/// <returns><c>true</c>, if result contains a valid gadget, <c>false</c> otherwise.</returns>
		/// <param name="name">Name used to store the gadget.</param>
		/// <param name="result">Gadget target.</param>
		/// <typeparam name="T">Gadget type.</typeparam>
		bool TryGetGadget<T>(string name, out T result) where T : GLib.Object;
		/// <summary>
		/// Stores the given gadget.
		/// </summary>
		/// <param name="name">Unique gadget name.</param>
		/// <param name="gadget">Gadget to store.</param>
		void RegisterGadget(string name, GLib.Object gadget);
		#endregion
	}


}

