using System;
using System.Xml;
using System.Collections.Generic;

namespace Mycena {
	public class InterfaceNode : IInterfaceNode {

		Dictionary<string, Gtk.Widget> widgets;
		Dictionary<string, GLib.Object> gadgets;
		List<GLib.Object> anonym;

		/// <summary>
		/// Initializes an empty instance of the <see cref="Mycena.InterfaceNode"/> class.
		/// </summary>
		public InterfaceNode() {
			widgets = new Dictionary<string, Gtk.Widget>(32);
			gadgets = new Dictionary<string, GLib.Object>(16);
			anonym = new List<GLib.Object>(16);
		}

		#region Widget
		public Gtk.Widget this[string name] {
			get { return GetWidget(name); }
		}
		public Gtk.Widget this[string name, Gtk.Widget defaultValue] {
			get { return GetWidget(name, defaultValue); }
		}
		public T GetWidget<T>(string name) where T : Gtk.Widget {
			if (name == null) throw new ArgumentNullException("name");
			Gtk.Widget result;
			if (widgets.TryGetValue(name, out result)) {
				if (result is T) {
					return (T)result;
				} else {
					throw new ArgumentException("type argument");
				}
			} else {
				throw new ArgumentException("name");
			}
		}
		public Gtk.Widget GetWidget(string name) {
			return GetWidget<Gtk.Widget>(name);
		}
		public T GetWidget<T>(string name, T defaultValue) where T : Gtk.Widget {
			if (name == null) throw new ArgumentNullException("name");
			Gtk.Widget result;
			if (widgets.TryGetValue(name, out result)) {
				if (result is T) {
					return (T)result;
				}
			}
			return defaultValue;
		}
		public Gtk.Widget GetWidget(string name, Gtk.Widget defaultValue) {
			return GetWidget<Gtk.Widget>(name, defaultValue);
		}
		public bool TryGetWidget<T>(string name, out T result) where T : Gtk.Widget {
			if (name == null) throw new ArgumentNullException("name");
			Gtk.Widget widget;
			if (widgets.TryGetValue(name, out widget)) {
				if (widget is T) {
					result = (T)widget;
					return true;
				}
			}
			result = default(T);
			return false;

		}
		public void RegisterWidget(string name, Gtk.Widget widget) {
			if (name == null) throw new ArgumentNullException("name");
			if (widget == null) throw new ArgumentNullException("widget");
			try {
				widgets.Add(name, widget);
			} catch (ArgumentException) {
				throw new ArgumentException("name: widget with this name does already exist");
			}
		}
		public void AddWidget(Gtk.Widget widget) {
			if (widget == null) throw new ArgumentNullException("widget");
			anonym.Add(widget);
		}
		#endregion

		#region Gadget
		public T GetGadget<T>(string name) where T : GLib.Object {
			if (name == null) throw new ArgumentNullException("name");
			GLib.Object result;
			if (gadgets.TryGetValue(name, out result)) {
				if (result is T) {
					return (T)result;
				} else {
					throw new ArgumentException("type argument");
				}
			} else {
				throw new ArgumentException("name");
			}
		}
		public GLib.Object GetGadget(string name) {
			return GetGadget<GLib.Object>(name);
		}
		public T GetGadget<T>(string name, T defaultValue) where T : GLib.Object {
			if (name == null) throw new ArgumentNullException("name");
			GLib.Object result;
			if (gadgets.TryGetValue(name, out result)) {
				if (result is T) {
					return (T)result;
				}
			}
			return defaultValue;
		}
		public GLib.Object GetGadget(string name, GLib.Object defaultValue) {
			return GetGadget<GLib.Object>(name, defaultValue);
		}
		public bool TryGetGadget<T>(string name, out T result) where T : GLib.Object {
			if (name == null) throw new ArgumentNullException("name");
			GLib.Object Gadget;
			if (gadgets.TryGetValue(name, out Gadget)) {
				if (Gadget is T) {
					result = (T)Gadget;
					return true;
				}
			}
			result = default(T);
			return false;

		}
		public void RegisterGadget(string name, GLib.Object gadget) {
			if (name == null) throw new ArgumentNullException("name");
			if (gadget == null) throw new ArgumentNullException("gadget");
			try {
				gadgets.Add(name, gadget);
			} catch (ArgumentException) {
				throw new ArgumentException("name: Gadget with this name does already exist");
			}
		}
		public void AddGadget(GLib.Object gadget) {
			if (gadget == null) throw new ArgumentNullException("gadget");
			anonym.Add(gadget);
		}
		#endregion

		public void Dispose() {
			foreach (var widget in widgets.Values) {
				widget.Dispose();
			}
			widgets.Clear();
			foreach (var gadget in gadgets.Values) {
				gadget.Dispose();
			}
			gadgets.Clear();
			foreach (var obj in anonym) {
				obj.Dispose();
			}
			anonym.Clear();
		}




	}
}

