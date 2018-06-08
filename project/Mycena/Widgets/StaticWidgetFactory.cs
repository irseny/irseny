using System;
using System.Xml;
using System.Collections.Generic;

namespace Mycena {
	internal static class WidgetFactory {
		const string ObjectClassAttribute = "class";
		const string ObjectIdAttribute = "id";
		const string ObjectNodeName = "object";
		const string PackNodeName = "packing";
		const string PropertyNodeName = "property";
		const string PlaceholderNodeName = "placeholder";

		static IDictionary<string, IWidgetFactory> widgets;

		static WidgetFactory() {
			widgets = new Dictionary<string, IWidgetFactory>(32);
			widgets.Add("GtkEntryBuffer", new IgnoreWidgetFactory());
			widgets.Add("GtkImage", new ImageFactory());
			widgets.Add("GtkWindow", new WindowFactory());
			widgets.Add("GtkHBox", new HorizontalBoxFactory());
			widgets.Add("GtkVBox", new VerticalBoxFactory());
			widgets.Add("GtkHPaned", new HorizontalPanedFactory());
			widgets.Add("GtkVPaned", new VerticalPanedFactory());
			widgets.Add("GtkScrolledWindow", new ScrolledWindowFactory());
			widgets.Add("GtkTextView", new TextViewFactory());
		}
		/// <summary>
		/// Creates the widget defined by the given node alongside its children.
		/// </summary>
		/// <param name="rootNode">Node to create the root widget from.</param>
		/// <param name="container">Widget container.</param>
		/// <returns>The created widget.</returns>
		public static Tuple<Gtk.Widget, ConfigProperties> CreateWidget(XmlNode rootNode, IInterfaceNode container) {
			if (rootNode == null) throw new ArgumentNullException("rootNode");
			if (container == null) throw new ArgumentNullException("result");
			var classAttr = rootNode.Attributes[ObjectClassAttribute]; // may be null in case of child and not object node
			XmlNode packNode = null;
			XmlNode objectNode = null;
			bool hasPlaceholder = false;
			if (classAttr != null) {
				objectNode = rootNode;
			} else {
				foreach (XmlNode node in rootNode.ChildNodes) {
					if (node.Name.Equals(ObjectNodeName)) {
						if (objectNode != null) throw new InvalidOperationException("Child node with multiple children: " + rootNode.OuterXml);
						objectNode = node;
					} else if (node.Name.Equals(PackNodeName)) {
						if (packNode != null) throw new InvalidOperationException("Child node with multiple pack nodes: " + rootNode.OuterXml);
						packNode = node;
					} else if (node.Name.Equals(PlaceholderNodeName)) {
						hasPlaceholder = true;
					}
				}
			}
			if (objectNode == null) {
				if (hasPlaceholder) {
					return new Tuple<Gtk.Widget, ConfigProperties>(null, new ConfigProperties());
				} else {
					throw new ArgumentException("rootNode: Does not contain an object node: " + rootNode.OuterXml);
				}
			}
			classAttr = objectNode.Attributes[ObjectClassAttribute];
			if (classAttr == null) throw new KeyNotFoundException("Object node without class attribute: " + objectNode.OuterXml);
			IWidgetFactory factory;
			if (widgets.TryGetValue(classAttr.Value, out factory)) {
				var config = new ConfigProperties();
				if (packNode != null) {
					ReadPackProperties(packNode, config);
				}
				Gtk.Widget widget = factory.CreateWidget(objectNode, container);
				return new Tuple<Gtk.Widget, ConfigProperties>(widget, config);
			} else {
				throw new NotSupportedException(string.Format("Unknown object class name '{0}' in node: {1}", classAttr.Value, objectNode.OuterXml));
			}
		}



		/// <summary>
		/// Finds the widget with the given id in the xml tree.
		/// </summary>
		/// <returns>Node that defines the widget, null if it does not exist.</returns>
		/// <param name="rootNode">Owning child node of the tree to search in, may be null.</param>
		/// <param name="widgetNode">Root node to start searching from.</param>
		/// <param name="id">Unique widget identifier.</param>
		public static XmlNode FindWidgetNode(XmlNode rootNode, XmlNode widgetNode, string id) {
			foreach (XmlNode node in widgetNode.ChildNodes) {
				if (node.Attributes != null) { // null if comment node
					var idAttr = node.Attributes[ObjectIdAttribute];
					if (idAttr != null && idAttr.Value.Equals(id)) {
						if (rootNode != null) {
							return rootNode;
						} else {
							return node;
						}
					}
				}
			}
			foreach (XmlNode node in widgetNode.ChildNodes) {
				XmlNode result;
				if (node.Name.Equals("child")) {
					result = FindWidgetNode(node, node, id);
				} else {
					result = FindWidgetNode(rootNode, node, id);
				}
				if (result != null) {
					return result;
				}
			}
			return null;
		}
		/// <summary>
		/// Reads the pack properties from the given packing node.
		/// </summary>
		/// <param name="packNode">Packing node to read properties from.</param>
		/// <param name="properties">Properties read from the packing node.</param>
		private static void ReadPackProperties(XmlNode packNode, ConfigProperties properties) {
			foreach (XmlNode propertyNode in packNode) {
				if (propertyNode.Name.Equals(PropertyNodeName)) {
					properties.RegisterProperty(propertyNode);
				}
			}	
		}
	}
}

