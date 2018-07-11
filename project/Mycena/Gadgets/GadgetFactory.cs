using System;
using System.Xml;
using System.Collections.Generic;

namespace Mycena {
	internal abstract class GadgetFactory<T> : IGadgetFactory where T : GLib.Object {
		const string ObjectNodeName = "object";
		const string ObjectIdAttribute = "id";
		const string PropertyNodeName = "property";

		public GadgetFactory() {
			CreationProperties = new Dictionary<string, PropertyApplicationHandler<T>>();
		}
		protected delegate bool PropertyApplicationHandler<TW>(TW gadget, ConfigProperties properties, IInterfaceNode container);

		protected IDictionary<string, PropertyApplicationHandler<T>> CreationProperties { get; private set; }


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
			T gadget = CreateGadget(properties, container);
			if (gadget != null) {
				ApplyProperties(gadget, properties, container);
				container.RegisterGadget(idAttr.Value, gadget);
			} else {
				throw new InvalidOperationException("Cannot instantiate gadget from : " + rootNode.OuterXml);
			}
		}
		/// <summary>
		/// Creates a gadget with the given properties.
		/// </summary>
		/// <returns>The gadget created.</returns>
		/// <param name="properties">Creation properties.</param>
		/// <param name="container">Gadget container.</param>
		protected abstract T CreateGadget(ConfigProperties properties, IInterfaceNode container);

		/// <summary>
		/// Applies all known properties to the given gadget.
		/// </summary>
		/// <param name="gadget">Gadget.</param>
		/// <param name="properties">Available properties.</param>
		/// <param name="container">Widget container.</param>
		/// <param name="exclude">Properties to exclude.</param>
		private void ApplyProperties(T gadget, ConfigProperties properties, IInterfaceNode container, ISet<string> exclude = null) {
			foreach (string p in properties.PropertyNames) {
				if (exclude == null || !exclude.Contains(p)) {
					PropertyApplicationHandler<T> handler;
					if (CreationProperties.TryGetValue(p, out handler)) {
						if (!handler(gadget, properties, container)) {
							throw new InvalidOperationException("Failed to apply property to gadget: " + p);
						}
					}
				}
			}
		}
	}
}

