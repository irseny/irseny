using System;
using System.Collections.Generic;

namespace Mycena {
	internal abstract class WidgetFactory<T> : IWidgetFactory where T : Gtk.Widget {
		protected const string ObjectNodeName = "object";
		protected const string ObjectIdAttribute = "id";
		protected const string ObjectClassAttribute = "class";
		protected const string PropertyNodeName = "property";
		protected const string PropertyNameAttribute = "name";
		protected const string PackNodeName = "packing";
		protected const string ChildNodeName = "child";

		static WidgetFactory() {
			CommonCreationProperties = new HashSet<string>(new string[] {
				"visible",
				"can_focus"
			});
			CommonPackProperties = new HashSet<string>(new string[] {
				"expand",
				"fill",
				"pack_type",
				"position"
			});
			CommonModProperties = new Dictionary<string, PropertyApplicationHandler<Gtk.Widget>>();
			CommonModProperties.Add("visible", SetVisibility);
			CommonModProperties.Add("can_focus", SetFocusable);
		}
		public WidgetFactory() {			
			IsContainer = false;
			AllowRegister = true;
			AllowChildren = true;
			CreationProperties = new HashSet<string>();
			ModProperties = new Dictionary<string, PropertyApplicationHandler<T>>();
			PackProperties = new HashSet<string>();
		}
		protected delegate bool PropertyApplicationHandler<TW>(TW widget, string text);

		private static ISet<string> CommonCreationProperties { get; set; }
		private static ISet<string> CommonPackProperties { get; set; }
		private static IDictionary<string, PropertyApplicationHandler<Gtk.Widget>> CommonModProperties { get; set; }

		public bool AllowRegister { get; protected set; }
		public bool AllowChildren { get; protected set; }
		public bool IsContainer { get; protected set; }
		protected ISet<string> CreationProperties { get; private set; }
		protected ISet<string> PackProperties { get; private set; }
		protected IDictionary<string, PropertyApplicationHandler<T>> ModProperties { get; private set; }
		public abstract string ClassName { get; }

		private static bool SetVisibility(Gtk.Widget widget, string text) {
			try {
				bool visible = TextParseTools.ParseBool(text);
				widget.Visible = visible;
				return true;
			} catch (FormatException) {
				return false;
			}
		}
		private static bool SetFocusable(Gtk.Widget widget, string text) {
			try {
				bool focusable = TextParseTools.ParseBool(text);
				widget.CanFocus = focusable;
				return true;
			} catch (FormatException) {
				return false;
			}
		}

		public Gtk.Widget CreateWidget(System.Xml.XmlNode rootNode, IWidgetRegister target) {
			if (rootNode == null) throw new ArgumentNullException("rootNode");
			if (target == null) throw new ArgumentNullException("target");
			if (!rootNode.Name.Equals(ObjectNodeName)) {
				throw new ArgumentException("rootNode: not an object node (" + rootNode + ")");
			}
			var classAttr = rootNode.Attributes[ObjectClassAttribute];
			if (classAttr == null) {
				throw new KeyNotFoundException("rootNode: missing class attribute (" + rootNode + ")");
			} 
			var idAttr = rootNode.Attributes[ObjectIdAttribute];
			if (idAttr == null) {
				throw new KeyNotFoundException("rootNode: missing id attribute (" + rootNode + ")");
			}
			if (!CheckClassName(classAttr.Value)) {
				throw new KeyNotFoundException("rootNode: wrong class name (" + rootNode + ")");
			}
			// creation pass
			var creationProperties = new Dictionary<string, string>();
			foreach (System.Xml.XmlNode propertyNode in rootNode.ChildNodes) {
				if (propertyNode.Name.Equals(PropertyNodeName)) {
					var nameAttribute = propertyNode.Attributes[PropertyNameAttribute];
					if (nameAttribute == null) {
						throw new KeyNotFoundException("missing name attribute in property node: " + propertyNode);
					}
					if (CreationProperties.Contains(nameAttribute.Value)) {
						creationProperties.Add(nameAttribute.Value, propertyNode.InnerText);
					}

				} 
			}
			T widget = CreateWidget(creationProperties);
			if (widget != null) {
				if (AllowRegister) {
					target.RegisterWidget(idAttr.Value, widget);
				}
				// mod property pass
				foreach (System.Xml.XmlNode propertyNode in rootNode.ChildNodes) {
					if (propertyNode.Name.Equals(PropertyNodeName)) {
						var nameAttribute = propertyNode.Attributes[PropertyNameAttribute];
						if (nameAttribute == null) {
							throw new KeyNotFoundException("missing name attribute in property node: " + propertyNode);
						}
						if (ModProperties.ContainsKey(nameAttribute.Value)) {
							if (!ModProperties[nameAttribute.Value](widget, propertyNode.InnerText)) {
								throw new InvalidOperationException("unable to apply property: " + propertyNode);
							}
						} else if (CommonModProperties.ContainsKey(nameAttribute.Value)) {
							if (!CommonModProperties[nameAttribute.Value](widget, propertyNode.InnerText)) {
								throw new InvalidOperationException("unable to apply property: " + propertyNode);
							}
						}
					}
				}

				// children pass
				foreach (System.Xml.XmlNode childNode in rootNode.ChildNodes) {
					if (childNode.Name.Equals(ChildNodeName)) {
					
					}
				}
			}
			return widget;
		}
		private bool CheckClassName(string name) {
			if (ClassName != null) {
				return ClassName.Equals(name);
			} else {
				return true;
			}
		}
		protected abstract T CreateWidget(IDictionary<string, string> properties);

	}
}

