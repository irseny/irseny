using System;
using System.Xml;
using System.Collections.Generic;

namespace Mycena {
	internal class GadgetFactory {
		IDictionary<string, IGadgetFactory> gadgets;

		public GadgetFactory(IInterfaceStock stock) {
			if (stock == null) throw new ArgumentNullException("stock");
			Stock = stock;
			gadgets = new Dictionary<string, IGadgetFactory>(16);
			gadgets.Add("GtkAdjustment", new AdjustmentFactory());
			gadgets.Add("GtkImage", new ImageGadgetFactory());
			gadgets.Add("GtkTextTagTable", new TextTagTableFactory());
			gadgets.Add("GtkRadioButton", new RadioButtonGroupFactory());
		}
		/// <summary>
		/// Gets the stock resoruces.
		/// </summary>
		/// <value>The stock resources.</value>
		public IInterfaceStock Stock { get; private set; }
		/// <summary>
		/// Creates the gadgets that are on the first level of the given <see cref="System.Xml.XmlNode"/>.
		/// </summary>
		/// <param name="rootNode">Root node to search for gadget information.</param>
		/// <param name="container">Gadget container.</param>
		public void CreateGadgets(XmlNode rootNode, IInterfaceNode container) {
			if (rootNode == null) throw new ArgumentNullException("rootNode");
			if (container == null) throw new ArgumentNullException("container");
			foreach (XmlNode objectNode in rootNode.ChildNodes) {
				if (objectNode.Name.Equals("object")) {
					CreateGadget(objectNode, container);
				}
			}
		}
		/// <summary>
		/// Creates a gadget from the given <see cref="System.Xml.XmlNode"/>.
		/// </summary>
		/// <returns>The gadget created, may be null.</returns>
		/// <param name="rootNode">Node to create the gadget from.</param>
		/// <param name="container">Gadget container.</param>
		private void CreateGadget(XmlNode rootNode, IInterfaceNode container) {
			if (rootNode == null) throw new ArgumentNullException("rootNode");
			if (container == null) throw new ArgumentNullException("container");
			var classAttr = rootNode.Attributes["class"];
			if (classAttr == null) throw new ArgumentException("rootNode: Missing class attribute: " + rootNode.OuterXml);
			IGadgetFactory factory;
			if (gadgets.TryGetValue(classAttr.Value, out factory)) {
				factory.CreateGadget(rootNode, container, this);
			}
		}
	}
	internal abstract class GadgetFactory<T> : IGadgetFactory where T : GLib.Object {

		public GadgetFactory() {
			CreationProperties = new Dictionary<string, PropertyApplicationHandler<T>>();
		}
		protected delegate bool PropertyApplicationHandler<TW>(TW gadget, ConfigProperties properties, IInterfaceNode container, IInterfaceStock stock);

		protected IDictionary<string, PropertyApplicationHandler<T>> CreationProperties { get; private set; }


		public void CreateGadget(XmlNode rootNode, IInterfaceNode container, GadgetFactory rootFactory) {
			if (rootNode == null) throw new ArgumentNullException("rootNode");
			if (container == null) throw new ArgumentNullException("container");
			if (!rootNode.Name.Equals("object")) {
				throw new ArgumentException("rootNode: Not an object node: " + rootNode.OuterXml);
			}
			var idAttr = rootNode.Attributes["id"];
			if (idAttr == null) {
				throw new ArgumentException("rootNode: Missing id attribute: " + rootNode.OuterXml);
			}
			// creation properties
			var properties = new ConfigProperties();
			foreach (XmlNode propertyNode in rootNode.ChildNodes) {
				if (propertyNode.Name.Equals("property")) {
					properties.RegisterProperty(propertyNode);
				}
			}
			T gadget = CreateGadget(properties, container, rootFactory.Stock);
			if (gadget != null) {
				ApplyProperties(gadget, properties, container, rootFactory.Stock);
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
		/// <param name="stock">Stock resources.</param>
		protected abstract T CreateGadget(ConfigProperties properties, IInterfaceNode container, IInterfaceStock stock);

		/// <summary>
		/// Applies all known properties to the given gadget.
		/// </summary>
		/// <param name="gadget">Gadget.</param>
		/// <param name="properties">Available properties.</param>
		/// <param name="container">Widget container.</param>
		/// <param name="stock">Stock resources.</param>
		private void ApplyProperties(T gadget, ConfigProperties properties, IInterfaceNode container, IInterfaceStock stock) {
			foreach (string p in properties.PropertyNames) {
				PropertyApplicationHandler<T> handler;
				if (CreationProperties.TryGetValue(p, out handler)) {
					if (!handler(gadget, properties, container, stock)) {
						throw new InvalidOperationException("Failed to apply property to gadget: " + p);
					}
				}
			}
		}
	}
}

