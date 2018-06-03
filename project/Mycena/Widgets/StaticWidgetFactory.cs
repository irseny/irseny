using System;
using System.Xml;
using System.Collections.Generic;

namespace Mycena {
	internal static class WidgetFactory {
		const string ObjectClassAttribute = "class";
		const string ObjectIdAttribute = "id";

		static IDictionary<string, IWidgetFactory> widgets;

		static WidgetFactory() {
			widgets = new Dictionary<string, IWidgetFactory>();
			widgets.Add("GtkEntryBuffer", new IgnoreWidgetFactory());
			widgets.Add("GtkImage", new ImageFactory());
			widgets.Add("GtkWindow", new WindowFactory());
		}
		/// <summary>
		/// Creates the widget defined by the given node alongside its children.
		/// </summary>
		/// <returns>Container with the widgets created.</returns>
		/// <param name="rootNode">Node that defines the widgets to create.</param>
		public static IInterfaceNode CreateWidget(XmlNode rootNode) {
			var result = new InterfaceNode();
			bool exceptional = true;
			try {
				CreateWidget(rootNode, result);
				exceptional = false;
			} finally {
				if (exceptional) {
					result.Dispose();
				}
			}
			return result;
		}
		/// <summary>
		/// Creates the widget defined by the given node alongside its children.
		/// </summary>
		/// <param name="rootNode">Node to create the root widget from.</param>
		/// <param name="result">Widget container.</param>
		private static void CreateWidget(XmlNode rootNode, IWidgetRegister result) {
			if (rootNode == null) throw new ArgumentNullException("rootNode");
			if (result == null) throw new ArgumentNullException("result");
			var classAttr = rootNode.Attributes[ObjectClassAttribute];
			if (classAttr == null) {
				throw new ArgumentException("rootNode: missing class name attribute (" + rootNode + ")");
			}
			IWidgetFactory factory;
			if (widgets.TryGetValue(classAttr.Value, out factory)) {
				factory.CreateWidget(rootNode, result);
			} else {
				throw new NotSupportedException(string.Format("unknown class name {0} ({1})", classAttr.Value, rootNode));
			}
		}

		/// <summary>
		/// Finds the widget with the given id in the xml tree.
		/// </summary>
		/// <returns>Node that defines the widget, null if it does not exist.</returns>
		/// <param name="rootNode">Root node of the xml tree to search in.</param>
		/// <param name="id">Unique widget identifier.</param>
		public static XmlNode FindWidgetNode(XmlNode rootNode, string id) {
			foreach (XmlNode node in rootNode.ChildNodes) {
				if (node.Attributes != null) { // null if comment node
					var idAttr = node.Attributes[ObjectIdAttribute];
					if (idAttr != null && idAttr.Value.Equals(id)) {
						return node;
					}
				}
			}
			foreach (XmlNode node in rootNode.ChildNodes) {
				var result = FindWidgetNode(node, id);
				if (result != null) {
					return result;
				}
			}
			return null;
		}
	}
}

