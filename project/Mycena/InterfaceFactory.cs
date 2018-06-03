using System;
using System.Collections.Generic;

namespace Mycena {
	public class InterfaceFactory {
		const string ObjectClassAttribute = "class";
		static IDictionary<string, IWidgetFactory> widgets;

		static InterfaceFactory() {
			widgets = new Dictionary<string, IWidgetFactory>();
			widgets.Add("GtkEntryBuffer", new IgnoreWidgetFactory());
			widgets.Add("GtkImage", new ImageFactory());
		}
		private InterfaceFactory(System.Xml.XmlDocument doc) {
			if (doc == null) throw new ArgumentNullException("doc");
			Source = doc;
		}
		/// <summary>
		/// Gets or sets the source document.
		/// </summary>
		/// <value>The source document.</value>
		private System.Xml.XmlDocument Source { get; set; }
		/// <summary>
		/// Creates widget defined by the given node alongside its children.
		/// </summary>
		/// <param name="rootNode">Node to create the root widget from.</param>
		/// <param name="result">Widget container.</param>
		internal static void CreateWidget(System.Xml.XmlNode rootNode, IWidgetRegister result) {
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
		/// Creates the widget with the given identifer alongside its children.
		/// </summary>
		/// <returns>Container with the widgets created.</returns>
		/// <param name="id">Root widget identifer.</param>
		public IInterfaceRoot CreateWidget(string id) {
			if (id == null) throw new ArgumentNullException("id");
			var rootNode = FindWidgetNode(Source.DocumentElement, id);
			if (rootNode == null) throw new KeyNotFoundException("id");
			var result = CreateWidget(rootNode);
			return result;
		}
		/// <summary>
		/// Creates the widget defined by the given node alongside its children.
		/// </summary>
		/// <returns>Container with the widgets created.</returns>
		/// <param name="rootNode">Node that defines the widgets to create.</param>
		private IInterfaceRoot CreateWidget(System.Xml.XmlNode rootNode) {
			var result = new InterfaceRoot();
			try {
				CreateWidget(rootNode, result);
			} catch (Exception e) {
				result.Dispose();
				throw e;
			}
			return result;
		}
		/// <summary>
		/// Finds the widget with the given id in the xml tree.
		/// </summary>
		/// <returns>Node that defines the widget, null if it does not exist.</returns>
		/// <param name="rootNode">Root node of the xml tree to search in.</param>
		/// <param name="id">Unique widget identifier.</param>
		private System.Xml.XmlNode FindWidgetNode(System.Xml.XmlNode rootNode, string id) {
			foreach (System.Xml.XmlNode node in rootNode.ChildNodes) {
				if (node.Name.Equals(id)) {
					return node;
				}
			}
			foreach (System.Xml.XmlNode node in rootNode.ChildNodes) {
				var result = FindWidgetNode(node, id);
				if (result != null) {
					return result;
				}
			}
			return null;
		}
		/// <summary>
		/// Creates an instance of this class from the given glade file.
		/// </summary>
		/// <returns>Create instance.</returns>
		/// <param name="filePath">Path to glade file.</param>
		public static InterfaceFactory CreateFromFile(string filePath) {
			if (filePath == null) throw new ArgumentNullException("filePath");
			var doc = new System.Xml.XmlDocument();
			doc.Load(filePath);
			return new InterfaceFactory(doc);
		}
	}
}

