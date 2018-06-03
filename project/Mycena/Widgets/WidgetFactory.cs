using System;
using System.Xml;
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
		protected delegate bool PropertyApplicationHandler<TW>(TW widget, string property, string value);

		private static ISet<string> CommonCreationProperties { get; set; }
		private static ISet<string> CommonPackProperties { get; set; }
		private static IDictionary<string, PropertyApplicationHandler<Gtk.Widget>> CommonModProperties { get; set; }

		public bool AllowRegister { get; protected set; }
		public bool AllowChildren { get; protected set; }
		public bool IsContainer { get; protected set; }
		public abstract string ClassName { get; }
		protected ISet<string> CreationProperties { get; private set; }
		protected ISet<string> PackProperties { get; private set; }
		protected IDictionary<string, PropertyApplicationHandler<T>> ModProperties { get; private set; }



		public Gtk.Widget CreateWidget(XmlNode rootNode, IWidgetRegister target) {
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
			var creationProperties = new Dictionary<string, string>(16);
			foreach (XmlNode propertyNode in rootNode.ChildNodes) {
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
				foreach (XmlNode propertyNode in rootNode.ChildNodes) {
					if (propertyNode.Name.Equals(PropertyNodeName)) {
						var nameAttribute = propertyNode.Attributes[PropertyNameAttribute];
						if (nameAttribute == null) {
							throw new KeyNotFoundException("Missing name attribute in property node: " + propertyNode);
						}
						if (ModProperties.ContainsKey(nameAttribute.Value)) {
							if (!ModProperties[nameAttribute.Value](widget, nameAttribute.Value, propertyNode.InnerText)) {
								throw new InvalidOperationException("Unable to apply property: " + propertyNode);
							}
						} else if (CommonModProperties.ContainsKey(nameAttribute.Value)) {
							if (!CommonModProperties[nameAttribute.Value](widget, nameAttribute.Value, propertyNode.InnerText)) {
								throw new InvalidOperationException("Unable to apply property: " + propertyNode);
							}
						}
					}
				}

				// children pass
				if (AllowChildren && IsContainer) {
					foreach (XmlNode childNode in rootNode.ChildNodes) {
						if (childNode.Name.Equals(ChildNodeName)) {
							Gtk.Widget child = null;
							var packProperties = new Dictionary<string, string>(16);
							foreach (XmlNode innerNode in childNode.ChildNodes) {
								if (innerNode.Name.Equals(ObjectNodeName)) {
									if (child != null) {
										throw new InvalidOperationException("Multiple objects defined in child node: " + childNode);
									} else {
										child = WidgetFactory.CreateWidget(innerNode, target);
									}
								} else if (innerNode.Name.Equals(PackNodeName)) {
									foreach (XmlNode propertyNode in innerNode.ChildNodes) {
										if (propertyNode.Name.Equals(PropertyNodeName)) {
											var nameAttribute = propertyNode.Attributes[PropertyNameAttribute];
											if (nameAttribute == null) {
												throw new KeyNotFoundException("Missing name attribute in property node: " + propertyNode);
											}
											if (PackProperties.Contains(nameAttribute.Value) || CommonPackProperties.Contains(nameAttribute.Value)) {
												packProperties.Add(nameAttribute.Value, propertyNode.InnerText);
											}
										}
									}
								}
							}

						}
					}
				}
			}
			return widget;
		}
		/// <summary>
		/// Creates a widget with the given properties.
		/// </summary>
		/// <returns>The widget created.</returns>
		/// <param name="properties">Properties for instantiation.</param>
		protected abstract T CreateWidget(IDictionary<string, string> properties);
		/// <summary>
		/// Packs the given child in the container widget.
		/// </summary>
		/// <param name="container">Container to place the child in.</param>
		/// <param name="child">Child widget to place in the container.</param>
		/// <param name="properties">Packing properties.</param>
		protected abstract void PackWidget(T container, Gtk.Widget child, IDictionary<string, string> properties);
		/// <summary>
		/// Indicates whether the available properties contain all required properties.
		/// </summary>
		/// <returns><c>true</c>, if all required properties are available, <c>false</c> otherwise.</returns>
		/// <param name="available">Avaialable properties.</param>
		/// <param name="required">Required properties.</param>
		protected static bool CheckRequiredProperties(IDictionary<string, string> available, params string[] required) {
			if (available == null) throw new ArgumentNullException("available");
			if (required == null) throw new ArgumentNullException("required");
			foreach (string p in required) {
				if (!available.ContainsKey(p)) {
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
		protected static void EnsureReqquiredProperties(IDictionary<string, string> available, params string[] required) {
			if (available == null) throw new ArgumentNullException("available");
			if (required == null) throw new ArgumentNullException("required");
			foreach (string p in required) {
				if (!available.ContainsKey(p)) {
					throw new KeyNotFoundException(string.Format("Property '{0}' not found", p));
				}
			}
		}
		/// <summary>
		/// Sets the visibility of the given <see cref="Gtk.Widget"/> 
		/// </summary>
		/// <returns><c>true</c>, if operation was successful, <c>false</c> otherwise.</returns>
		/// <param name="widget">Widget to modify.</param>
		/// <param name="property">Property name.</param> 
		/// <param name="value">Property value.</param>
		private static bool SetVisibility(Gtk.Widget widget, string property, string value) {
			try {
				bool visible = TextParseTools.ParseBool(value);
				widget.Visible = visible;
				return true;
			} catch (FormatException) {
				return false;
			}
		}
		/// <summary>
		/// Sets whether the given <see cref="Gtk.Widget/> can be focused.
		/// </summary>
		/// <returns><c>true</c>, if the operation was successful, <c>false</c> otherwise.</returns>
		/// <param name="widget">Widget to modify.</param>
		/// <param name="property">Property name.</param>
		/// <param name="value">Property value.</param>
		private static bool SetFocusable(Gtk.Widget widget, string property, string value) {
			try {
				bool focusable = TextParseTools.ParseBool(value);
				widget.CanFocus = focusable;
				return true;
			} catch (FormatException) {
				return false;
			}
		}
		/// <summary>
		/// Checks whether the given class name corresponds to the class identifer of this instance.
		/// </summary>
		/// <returns><c>true</c>, if the names correspond or the class identifer is null, <c>false</c> otherwise.</returns>
		/// <param name="name">Class name to compare.</param>
		private bool CheckClassName(string name) {
			if (ClassName != null) {
				return ClassName.Equals(name);
			} else {
				return true;
			}
		}

	}
}

