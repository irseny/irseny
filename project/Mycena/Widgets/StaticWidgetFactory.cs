using System;
using System.Xml;
using System.Collections.Generic;

namespace Mycena {
	internal static partial class WidgetFactory {

		static IDictionary<string, IWidgetFactory> widgets;

		static WidgetFactory() {
			widgets = new Dictionary<string, IWidgetFactory>(32);
			widgets.Add("GtkEntryBuffer", new IgnoreWidgetFactory());
			widgets.Add("GtkImage", new ImageWidgetFactory());

			widgets.Add("GtkScrolledWindow", new ScrolledWindowFactory());
			widgets.Add("GtkWindow", new WindowFactory());
			widgets.Add("GtkHBox", new HorizontalBoxFactory());
			widgets.Add("GtkVBox", new VerticalBoxFactory());
			widgets.Add("GtkHPaned", new HorizontalPanedFactory());
			widgets.Add("GtkVPaned", new VerticalPanedFactory());
			widgets.Add("GtkTable", new TableFactory());
			widgets.Add("GtkNotebook", new NotebookFactory());

			widgets.Add("GtkHSeparator", new HorizontalSeparatorFactory());
			widgets.Add("GtkVSeparator", new VerticalSeparatorFactory());

			widgets.Add("GtkHScale", new HorizontalScaleFactory());
			widgets.Add("GtkVScale", new VerticalScaleFactory());

			widgets.Add("GtkTextView", new TextViewFactory());
			widgets.Add("GtkLabel", new LabelFactory());
			widgets.Add("GtkEntry", new EntryFactory());

			widgets.Add("GtkButton", new ButtonFactory());
			widgets.Add("GtkToggleButton", new ToggleButtonFactory());
			widgets.Add("GtkCheckButton", new CheckButtonFactory());
			widgets.Add("GtkSpinButton", new SpinButtonFactory());

			widgets.Add("GtkComboBoxText", new ComboBoxFactory());
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
			var classAttr = rootNode.Attributes["class"]; // may be null in case of child and not object node
			XmlNode packNode = null;
			XmlNode objectNode = null;
			XmlNode childNode = null;
			bool hasPlaceholder = false;
			if (classAttr != null) {
				objectNode = rootNode;
			} else {
				childNode = rootNode;
				foreach (XmlNode node in rootNode.ChildNodes) {
					if (node.Name.Equals("object")) {
						if (objectNode != null) throw new InvalidOperationException("Child node with multiple children: " + rootNode.OuterXml);
						objectNode = node;
					} else if (node.Name.Equals("packing")) {
						if (packNode != null) throw new InvalidOperationException("Child node with multiple pack nodes: " + rootNode.OuterXml);
						packNode = node;
					} else if (node.Name.Equals("placeholder")) {
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
			classAttr = objectNode.Attributes["class"];
			if (classAttr == null) throw new KeyNotFoundException("Object node without class attribute: " + objectNode.OuterXml);
			IWidgetFactory factory;
			if (widgets.TryGetValue(classAttr.Value, out factory)) {
				var config = new ConfigProperties();
				if (packNode != null) {
					ReadPackProperties(childNode, packNode, config);
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
					var idAttr = node.Attributes["id"];
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
		/// <param name="childNode">Child node to read type properties from, may be null.</param>
		/// <param name="packNode">Packing node to read properties from.</param>
		/// <param name="properties">Properties read from the packing node.</param>
		private static void ReadPackProperties(XmlNode childNode, XmlNode packNode, ConfigProperties properties) {
			if (childNode != null && childNode.Attributes != null) {
				foreach (XmlAttribute attr in childNode.Attributes) {
					var propertyName = "child_" + attr.Name;
					properties.RegisterAttribute("child_" + attr.Name, attr.Value);
				}
			}
			foreach (XmlNode propertyNode in packNode) {
				if (propertyNode.Name.Equals("property")) {
					properties.RegisterProperty(propertyNode);
				}
			}
		}
	}
}

