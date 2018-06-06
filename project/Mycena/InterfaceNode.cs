using System;
using System.Xml;
using System.Collections.Generic;

namespace Mycena {
	public class InterfaceNode : IInterfaceNode {

		Dictionary<string, Gtk.Widget> widgets = new Dictionary<string, Gtk.Widget>();
		//IDictionary<string, Dictionary<string, XmlNode>> packInfo = new Dictionary<string, Dictionary<string, XmlNode>>();

		/// <summary>
		/// Initializes an empty instance of the <see cref="Mycena.InterfaceNode"/> class.
		/// </summary>
		public InterfaceNode() {
		}

		public Gtk.Widget this[string name] {
			get { return GetWidget(name); }
		}
		public Gtk.Widget this[string name, Gtk.Widget defaultValue] {
			get { return GetWidget(name, defaultValue); }
		}
		public T GetWidget<T>(string name) where T : Gtk.Widget {
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
		/*public IDictionary<string, XmlNode> GetPackInfo(string name) {
			if (name == null) throw new ArgumentNullException("name");
			Dictionary<string, XmlNode> info;
			if (packInfo.TryGetValue(name, out info)) {
				return new Dictionary<string, XmlNode>(info);
			} else {
				return new Dictionary<string, XmlNode>();
			}
		}
		public void StorePackInfo(string name, IDictionary<string, XmlNode> properties) {
			if (name == null) throw new ArgumentNullException("name");
			if (properties == null) throw new ArgumentNullException("properties");
			packInfo[name] = new Dictionary<string, XmlNode>(properties);
		}*/

		public void Dispose() {
			foreach (var widget in widgets.Values) {
				widget.Dispose();
			}
			widgets.Clear();
			//packInfo.Clear();
		}




	}
}

