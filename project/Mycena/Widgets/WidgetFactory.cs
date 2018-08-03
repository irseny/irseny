using System;
using System.Text;
using System.Xml;
using System.Collections.Generic;
using System.Diagnostics;

namespace Mycena {
	internal class WidgetFactory {

		IDictionary<string, IWidgetFactory> widgets;

		public WidgetFactory(IInterfaceStock stock) {
			if (stock == null) throw new ArgumentNullException("stock");
			Stock = stock;
			widgets = new Dictionary<string, IWidgetFactory>(32);
			widgets.Add("GtkEntryBuffer", new IgnoreWidgetFactory());
			widgets.Add("GtkImage", new ImageWidgetFactory());

			widgets.Add("GtkScrolledWindow", new ScrolledWindowFactory());
			widgets.Add("GtkViewport", new ViewportFactory());
			widgets.Add("GtkWindow", new WindowFactory());
			widgets.Add("GtkBox", new BoxFactory());
			widgets.Add("GtkPaned", new PanedFactory());
			widgets.Add("GtkTable", new TableFactory());
			widgets.Add("GtkGrid", new GridFactory());
			widgets.Add("GtkNotebook", new NotebookFactory());

			widgets.Add("GtkHSeparator", new HorizontalSeparatorFactory());
			widgets.Add("GtkVSeparator", new VerticalSeparatorFactory());

			widgets.Add("GtkScale", new ScaleFactory());

			widgets.Add("GtkTextView", new TextViewFactory());
			widgets.Add("GtkLabel", new LabelFactory());
			widgets.Add("GtkEntry", new EntryFactory());

			widgets.Add("GtkButton", new ButtonFactory());
			widgets.Add("GtkToggleButton", new ToggleButtonFactory());
			widgets.Add("GtkCheckButton", new CheckButtonFactory());
			widgets.Add("GtkSpinButton", new SpinButtonFactory());

			widgets.Add("GtkComboBoxText", new ComboBoxTextFactory());
		}
		/// <summary>
		/// Gets the stock resources for widget creation.
		/// </summary>
		/// <value>The stock resources.</value>
		public IInterfaceStock Stock { get; private set; }
		/// <summary>
		/// Creates the widget defined by the given node alongside its children.
		/// </summary>
		/// <param name="rootNode">Node to create the root widget from.</param>
		/// <param name="container">Widget container.</param>
		/// <returns>The created widget.</returns>
		public Tuple<Gtk.Widget, ConfigProperties> CreateWidget(XmlNode rootNode, IInterfaceNode container) {
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
				Gtk.Widget widget = factory.CreateWidget(objectNode, container, this);
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
		public XmlNode FindWidgetNode(XmlNode rootNode, XmlNode widgetNode, string id) {
			if (widgetNode == null) throw new ArgumentNullException("widgetNode");
			if (id == null) throw new ArgumentNullException("id");
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
		private void ReadPackProperties(XmlNode childNode, XmlNode packNode, ConfigProperties properties) {
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

	internal abstract class WidgetFactory<T> : IWidgetFactory where T : Gtk.Widget {


		static WidgetFactory() {
			CommonCreationProperties = new Dictionary<string, PropertyApplicationHandler<Gtk.Widget>>();
			CommonCreationProperties.Add("visible", CommonWidgetMods.SetVisibility);
			CommonCreationProperties.Add("can_focus", CommonWidgetMods.SetFocusable);
			CommonCreationProperties.Add("receives_default", CommonWidgetMods.SetReceiveDefault);
			CommonCreationProperties.Add("sensitive", CommonWidgetMods.SetSensitivity);
			CommonCreationProperties.Add("hadjustment", CommonWidgetMods.SetScrollAdjustment);
			CommonCreationProperties.Add("vadjustment", CommonWidgetMods.SetScrollAdjustment);
		}
		public WidgetFactory() {
			CreationProperties = new Dictionary<string, PropertyApplicationHandler<T>>();
			CreationAttributes = new Dictionary<string, PropertyApplicationHandler<T>>();
			PackProperties = new HashSet<string>();
		}
		protected delegate bool PropertyApplicationHandler<TW>(TW widget, ConfigProperties properties, IInterfaceNode container, IInterfaceStock stock);

		private static IDictionary<string, PropertyApplicationHandler<Gtk.Widget>> CommonCreationProperties { get; set; }


		protected ISet<string> PackProperties { get; private set; }
		protected IDictionary<string, PropertyApplicationHandler<T>> CreationProperties { get; private set; }
		protected IDictionary<string, PropertyApplicationHandler<T>> CreationAttributes { get; private set; }



		public Gtk.Widget CreateWidget(XmlNode rootNode, IInterfaceNode container, WidgetFactory rootFactory) {
			if (rootNode == null) throw new ArgumentNullException("rootNode");
			if (container == null) throw new ArgumentNullException("container");
			if (!rootNode.Name.Equals("object")) {
				throw new ArgumentException("rootNode: Not an object node: " + rootNode.OuterXml);
			}
			var idAttr = rootNode.Attributes["id"]; // may be null
			// creation property pass
			var creationProperties = new ConfigProperties();
			foreach (XmlNode propertyNode in rootNode.ChildNodes) {
				if (propertyNode.Name.Equals("property")) {
					creationProperties.RegisterProperty(propertyNode);
				} else if (propertyNode.Name.Equals("attributes")) {
					foreach (XmlNode attributeNode in propertyNode) {
						if (attributeNode.Name.Equals("attribute")) {
							creationProperties.RegisterAttribute(attributeNode);
						}
					}
				} else if (propertyNode.Name.Equals("items")) {
					foreach (XmlNode itemNode in propertyNode) {
						if (itemNode.Name.Equals("item")) {
							creationProperties.RegisterItem(itemNode);
						}
					}
				}
			}
			// creation (calls property application)
#if DEBUG
			creationProperties.BeginMark();
#endif
			T widget = CreateWidget(creationProperties, container, rootFactory.Stock);
			if (widget == null) {
				throw new InvalidOperationException("Cannot instantiate widget from: " + rootNode.OuterXml);
			}
			ApplyProperties(widget, creationProperties, container, rootFactory.Stock);
			ApplyAttributes(widget, creationProperties, container, rootFactory.Stock);
#if DEBUG
			creationProperties.EndMark();
#endif
			if (idAttr != null) {
				container.RegisterWidget(idAttr.Value, widget);
			} else {
				// the attribute can be unset, add anonymous in such cases
				container.AddWidget(widget);
			}
			var children = new List<Tuple<Gtk.Widget, ConfigProperties>>();
			// children pass
			foreach (XmlNode childNode in rootNode.ChildNodes) {
				if (childNode.Name.Equals("child")) {
					Tuple<Gtk.Widget, ConfigProperties> childTuple = rootFactory.CreateWidget(childNode, container);
					Gtk.Widget child = childTuple.Item1;
					if (child != null) { // can be null for placeholder
						ConfigProperties packProperties = childTuple.Item2;
						try {
							EnsureRequiredProperties(packProperties, PackProperties);
						} catch (KeyNotFoundException e) {
							throw new KeyNotFoundException("Missing pack properties in child node: " + childNode.OuterXml, e);
						}
					}
					children.Add(childTuple);
				}
			}
			if (!PackWidgets(widget, children, rootFactory.Stock)) {
				throw new InvalidOperationException("Unable to pack widgets: " + rootNode.OuterXml);
			}
			return widget;
		}
		/// <summary>
		/// Indicates whether the available properties contain all required properties.
		/// </summary>
		/// <returns><c>true</c>, if all required properties are available, <c>false</c> otherwise.</returns>
		/// <param name="available">Avaialable properties.</param>
		/// <param name="required">Required properties.</param>
		protected static bool CheckRequiredProperties(ConfigProperties available, ICollection<string> required) {
			if (available == null) throw new ArgumentNullException("available");
			if (required == null) throw new ArgumentNullException("required");
			foreach (string p in required) {
				if (available.GetProperty(p, null) == null) {
					return false;
				}
			}
			return true;
		}
		/// <summary>
		/// Throws an exception if one of the required properties is missing from the available properties.
		/// </summary>
		/// <param name="available">Available properties.</param>
		/// <param name="required">Required properties.</param>
		protected static void EnsureRequiredProperties(ConfigProperties available, ICollection<string> required) {
			if (available == null) throw new ArgumentNullException("available");
			if (required == null) throw new ArgumentNullException("required");
			foreach (string p in required) {
				if (available.GetProperty(p, null) == null) {
					throw new KeyNotFoundException("Property not found: " + p);
				}
			}
		}
		/// <summary>
		/// Applies all known properties to the given widget.
		/// </summary>
		/// <param name="widget">Widget.</param>
		/// <param name="properties">Available properties.</param>
		/// <param name="container">Widget container.</param>
		/// <param name="stock">Stock resources.</param>
		private void ApplyProperties(T widget, ConfigProperties properties, IInterfaceNode container, IInterfaceStock stock) {
			foreach (string p in properties.PropertyNames) {
				PropertyApplicationHandler<T> handler;
				PropertyApplicationHandler<Gtk.Widget> commonHandler;
				if (CreationProperties.TryGetValue(p, out handler)) {
					if (!handler(widget, properties, container, stock)) {
						throw new InvalidOperationException("Failed to apply property to widget: " + p);
					}
				} else if (CommonCreationProperties.TryGetValue(p, out commonHandler)) {
					if (!commonHandler(widget, properties, container, stock)) {
						throw new InvalidOperationException("Failed to apply property to widget: " + p);
					}
				}
			}
#if DEBUG
			var marked = properties.MarkedProperties;
			var available = properties.PropertyNames;
			foreach (string name in available) {
				if (!marked.Contains(name)) {
					Debug.WriteLine("Property '{0}' unused in: {1}", name, GetType().Name);
				}
			}
#endif
		}
		/// <summary>
		/// Applies all known attributes to the given widget.
		/// </summary>
		/// <param name="widget">Widget.</param>
		/// <param name="properties">Available properties.</param>
		/// <param name="container">Widget container.</param>
		/// <param name="stock">Stock resources.</param>
		private void ApplyAttributes(T widget, ConfigProperties properties, IInterfaceNode container, IInterfaceStock stock) {
			foreach (string a in properties.AttributeNames) {
				PropertyApplicationHandler<T> handler;
				if (CreationAttributes.TryGetValue(a, out handler)) {
					if (!handler(widget, properties, container, stock)) {
						throw new InvalidOperationException("Failed to apply attribute to widget: " + a);
					}
				}
			}
#if DEBUG
			var marked = properties.MarkedAttributes;
			var available = properties.AttributeNames;
			foreach (string name in available) {
				if (!marked.Contains(name)) {
					Debug.WriteLine("Attribute '{0}' unused in: {1}", name, GetType().Name);
				}
			}
#endif
		}
		/// <summary>
		/// Creates a widget with the given properties.
		/// </summary>
		/// <returns>The widget created.</returns>
		/// <param name="properties">Properties for instantiation.</param>
		/// <param name="container">Interface element container.</param>
		/// <param name="stock">Stock resources.</param>
		protected abstract T CreateWidget(ConfigProperties properties, IInterfaceNode container, IInterfaceStock stock);
		/// <summary>
		/// Packs the given children in the container widget.
		/// </summary>
		/// <returns><c>true</c>, if packing was successful, <c>false</c> otherwise.</returns>
		/// <param name="container">Container to place children in.</param>
		/// <param name="children">Children and properties to pack.</param>
		/// <param name="stock">Stock resources.</param>
		protected virtual bool PackWidgets(T container, IList<Tuple<Gtk.Widget, ConfigProperties>> children, IInterfaceStock stock) {
			foreach (var childTuple in children) {
				if (childTuple.Item1 != null) { // null for placeholder
					if (!PackWidget(container, childTuple.Item1, childTuple.Item2, stock)) {
						return false;
					}
				}
			}
			return true;
		}
		/// <summary>
		/// Packs the given child in the container widget.
		/// </summary>
		/// <returns><c>true</c>, if packing was successful, <c>false</c> otherwise.</returns>
		/// <param name="container">Container to place the child in.</param>
		/// <param name="child">Child widget to place in the container.</param>
		/// <param name="properties">Packing properties.</param>
		/// <param name="stock">Stock resources.</param>
		protected virtual bool PackWidget(T container, Gtk.Widget child, ConfigProperties properties, IInterfaceStock stock) {
			throw new InvalidOperationException(GetType().Name + " can not pack widgets");
		}



	}
}

