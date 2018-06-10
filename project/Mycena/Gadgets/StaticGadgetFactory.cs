using System;
using System.Xml;
using System.Collections.Generic;

namespace Mycena {
	internal static class GadgetFactory {
		const string ObjectNodeName = "object";
		const string ObjectClassAttribute = "class";
		const string ObjectIdAttribute = "id";
		const string PropertyNodeName = "property";
		const string PropertyNameAttribute = "name";

		static IDictionary<string, IGadgetFactory> gadgets;

		static GadgetFactory() {
			gadgets = new Dictionary<string, IGadgetFactory>(16);
			gadgets.Add("GtkAdjustment", new AdjustmentFactory());
			gadgets.Add("GtkImage", new ImageGadgetFactory());
			gadgets.Add("GtkTextTagTable", new TextTagTableFactory());
		}
		/// <summary>
		/// Creates the gadgets that are on the first level of the given <see cref="System.Xml.XmlNode"/>.
		/// </summary>
		/// <param name="rootNode">Root node to search for gadget information.</param>
		/// <param name="container">Gadget container.</param>
		public static void CreateGadgets(XmlNode rootNode, IInterfaceNode container) {
			if (rootNode == null) throw new ArgumentNullException("rootNode");
			if (container == null) throw new ArgumentNullException("container");
			foreach (XmlNode objectNode in rootNode.ChildNodes) {
				if (objectNode.Name.Equals(ObjectNodeName)) {
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
		private static void CreateGadget(XmlNode rootNode, IInterfaceNode container) {
			if (rootNode == null) throw new ArgumentNullException("rootNode");
			if (container == null) throw new ArgumentNullException("container");
			var classAttr = rootNode.Attributes[ObjectClassAttribute];
			if (classAttr == null) throw new ArgumentException("rootNode: Missing class attribute: " + rootNode.OuterXml);
			IGadgetFactory factory;
			if (gadgets.TryGetValue(classAttr.Value, out factory)) {
				factory.CreateGadget(rootNode, container);
			}
		}
	}
}

