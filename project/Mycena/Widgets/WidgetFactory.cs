using System;
using System.Text;
using System.Xml;
using System.Collections.Generic;
using System.Diagnostics;

namespace Mycena {
	internal abstract partial class WidgetFactory<T> : IWidgetFactory where T : Gtk.Widget {


		static WidgetFactory() {
			CommonCreationProperties = new Dictionary<string, PropertyApplicationHandler<Gtk.Widget>>();
			CommonCreationProperties.Add("visible", WidgetFactory.SetVisibility);
			CommonCreationProperties.Add("can_focus", WidgetFactory.SetFocusable);
			CommonCreationProperties.Add("receives_default", WidgetFactory.SetReceiveDefault);
			CommonCreationProperties.Add("sensitive", WidgetFactory.SetSensitivity);
			CommonCreationProperties.Add("hadjustment", WidgetFactory.SetScrollAdjustment);
			CommonCreationProperties.Add("vadjustment", WidgetFactory.SetScrollAdjustment);
		}
		public WidgetFactory() {
			CreationProperties = new Dictionary<string, PropertyApplicationHandler<T>>();
			CreationAttributes = new Dictionary<string, PropertyApplicationHandler<T>>();
			PackProperties = new HashSet<string>();
		}
		protected delegate bool PropertyApplicationHandler<TW>(TW widget, ConfigProperties properties, IInterfaceNode container);

		private static IDictionary<string, PropertyApplicationHandler<Gtk.Widget>> CommonCreationProperties { get; set; }


		protected ISet<string> PackProperties { get; private set; }
		protected IDictionary<string, PropertyApplicationHandler<T>> CreationProperties { get; private set; }
		protected IDictionary<string, PropertyApplicationHandler<T>> CreationAttributes { get; private set; }



		public Gtk.Widget CreateWidget(XmlNode rootNode, IInterfaceNode container) {
			if (rootNode == null) throw new ArgumentNullException("rootNode");
			if (container == null) throw new ArgumentNullException("container");
			if (!rootNode.Name.Equals("object")) {
				throw new ArgumentException("rootNode: Not an object node: " + rootNode.OuterXml);
			}
			var idAttr = rootNode.Attributes["id"];
			if (idAttr == null) {
				throw new KeyNotFoundException("Missing id attribute in object node: " + rootNode.OuterXml);
			}
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
			T widget = CreateWidget(creationProperties, container);
			if (widget == null) {
				throw new InvalidOperationException("Cannot instantiate widget from: " + rootNode.OuterXml);
			}
			ApplyProperties(widget, creationProperties, container);
			ApplyAttributes(widget, creationProperties, container);
#if DEBUG
			creationProperties.EndMark();
#endif
			container.RegisterWidget(idAttr.Value, widget);
			var children = new List<Tuple<Gtk.Widget, ConfigProperties>>();
			// children pass
			foreach (XmlNode childNode in rootNode.ChildNodes) {
				if (childNode.Name.Equals("child")) {
					Tuple<Gtk.Widget, ConfigProperties> childTuple = WidgetFactory.CreateWidget(childNode, container);
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
			if (!PackWidgets(widget, children)) {
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
		private void ApplyProperties(T widget, ConfigProperties properties, IInterfaceNode container) {
			foreach (string p in properties.PropertyNames) {
				PropertyApplicationHandler<T> handler;
				PropertyApplicationHandler<Gtk.Widget> commonHandler;
				if (CreationProperties.TryGetValue(p, out handler)) {
					if (!handler(widget, properties, container)) {
						throw new InvalidOperationException("Failed to apply property to widget: " + p);
					}
				} else if (CommonCreationProperties.TryGetValue(p, out commonHandler)) {
					if (!commonHandler(widget, properties, container)) {
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
		private void ApplyAttributes(T widget, ConfigProperties properties, IInterfaceNode container) {
			foreach (string a in properties.AttributeNames) {
				PropertyApplicationHandler<T> handler;
				if (CreationAttributes.TryGetValue(a, out handler)) {
					if (!handler(widget, properties, container)) {
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
		protected abstract T CreateWidget(ConfigProperties properties, IInterfaceNode container);
		/// <summary>
		/// Packs the given children in the container widget.
		/// </summary>
		/// <returns><c>true</c>, if packing was successful, <c>false</c> otherwise.</returns>
		/// <param name="container">Container to place children in.</param>
		/// <param name="children">Children and properties to pack.</param>
		protected virtual bool PackWidgets(T container, IList<Tuple<Gtk.Widget, ConfigProperties>> children) {
			foreach (var childTuple in children) {
				if (childTuple.Item1 != null) { // null for placeholder
					if (!PackWidget(container, childTuple.Item1, childTuple.Item2)) {
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
		protected virtual bool PackWidget(T container, Gtk.Widget child, ConfigProperties properties) {
			throw new InvalidOperationException(GetType().Name + " can not pack widgets");
		}



	}
}

