using System;
using System.Xml;
using System.Collections.Generic;

namespace Mycena {
	internal abstract class GadgetFactory<T> : IGadgetFactory where T : GLib.Object {
		const string ObjectNodeName = "object";
		const string ObjectIdAttribute = "id";
		const string PropertyNodeName = "property";

		public GadgetFactory() {
			GLib.Object txb = new Gtk.TextBuffer(null as Gtk.TextTagTable);
			GLib.Object adj = new Gtk.Adjustment(0, 0, 1, 0.1, 1, 2);
		}

		public void CreateGadget(XmlNode rootNode, IInterfaceNode container) {
			if (rootNode == null) throw new ArgumentNullException("rootNode");
			if (container == null) throw new ArgumentNullException("container");
			if (!rootNode.Name.Equals(ObjectNodeName)) {
				throw new ArgumentException("rootNode: Not an object node: " + rootNode.OuterXml);
			}
			var idAttr = rootNode.Attributes[ObjectIdAttribute];
			if (idAttr == null) {
				throw new ArgumentException("rootNode: Missing id attribute: " + rootNode.OuterXml);
			}
			// creation properties
			var properties = new ConfigProperties();
			foreach (XmlNode propertyNode in rootNode.ChildNodes) {
				if (propertyNode.Name.Equals(PropertyNodeName)) {
					properties.RegisterProperty(propertyNode);
				}
			}
			GLib.Object gadget = CreateGadget(properties, container);
			if (gadget != null) {
				container.RegisterGadget(idAttr.Value, gadget);
			}
		}
		/// <summary>
		/// Creates a gadget with the given properties.
		/// </summary>
		/// <returns>The gadget created.</returns>
		/// <param name="properties">Creation properties.</param>
		/// <param name="container">Gadget container.</param>
		protected abstract T CreateGadget(ConfigProperties properties, IInterfaceNode container);
	}
}

