using System;
using System.Text;
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
			CommonModProperties = new Dictionary<string, PropertyApplicationHandler<Gtk.Widget>>();
			CommonModProperties.Add("visible", SetVisibility);
			CommonModProperties.Add("can_focus", SetFocusable);
		}
		public WidgetFactory() {			
			IsContainer = false;
			AllowRegister = true;
			PackChildren = true;
			CreationProperties = new HashSet<string>();
			ModProperties = new Dictionary<string, PropertyApplicationHandler<T>>();
			PackProperties = new HashSet<string>();
		}
		protected delegate bool PropertyApplicationHandler<TW>(TW widget, XmlNode property);

		private static IDictionary<string, PropertyApplicationHandler<Gtk.Widget>> CommonModProperties { get; set; }

		public bool AllowRegister { get; protected set; }
		public bool PackChildren { get; protected set; }
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
				throw new KeyNotFoundException("Missing class attribute in object node: " + rootNode);
			} 
			var idAttr = rootNode.Attributes[ObjectIdAttribute];
			if (idAttr == null) {
				throw new KeyNotFoundException("Missing id attribute in object node: " + rootNode);
			}
			if (!CheckClassName(classAttr.Value)) {
				throw new ArgumentException(string.Format("rootNode: wrong class name: {0} should be {1}", classAttr.Value, ClassName));
			}
			// creation pass
			var creationProperties = new Dictionary<string, XmlNode>(16);
			var missingCreationProperties = new HashSet<string>(CreationProperties);
			foreach (XmlNode propertyNode in rootNode.ChildNodes) {
				if (propertyNode.Name.Equals(PropertyNodeName)) {
					if (propertyNode.InnerText == null || propertyNode.InnerText.Length == 0) {
						throw new KeyNotFoundException("Missing value in property node: " + propertyNode);
					}
					var nameAttribute = propertyNode.Attributes[PropertyNameAttribute];
					if (nameAttribute == null) {
						throw new KeyNotFoundException("Missing name attribute in property node: " + propertyNode);
					}
					if (missingCreationProperties.Contains(nameAttribute.Value)) {
						missingCreationProperties.Remove(nameAttribute.Value);
					}
					creationProperties.Add(nameAttribute.Value, propertyNode);
				} 
			}
			if (missingCreationProperties.Count > 0) {
				throw new KeyNotFoundException(
					string.Format("Missing creation properties ({0}) for object node: {1}", CreatePropertyList(missingCreationProperties), rootNode));
			}
			T widget = CreateWidget(creationProperties);
			if (widget == null) {
				return null;
			}
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
						if (!ModProperties[nameAttribute.Value](widget, propertyNode)) {
							throw new InvalidOperationException("Unable to apply property: " + propertyNode);
						}
					} else if (CommonModProperties.ContainsKey(nameAttribute.Value)) {
						if (!CommonModProperties[nameAttribute.Value](widget, propertyNode)) {
							throw new InvalidOperationException("Unable to apply property: " + propertyNode);
						}
					}
				}
			}
			// children pass
			if (PackChildren && IsContainer) {
				foreach (XmlNode childNode in rootNode.ChildNodes) {
					if (childNode.Name.Equals(ChildNodeName)) {
						Gtk.Widget child = null;
						var missingPackProperties = new HashSet<string>(PackProperties);
						var packProperties = new Dictionary<string, XmlNode>(16);
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
										if (propertyNode.InnerText == null || propertyNode.InnerText.Length == 0) {
											throw new FormatException("Missing value in property node: " + propertyNode);
										}
										var nameAttribute = propertyNode.Attributes[PropertyNameAttribute];
										if (nameAttribute == null) {
											throw new KeyNotFoundException("Missing name attribute in property node: " + propertyNode);
										}
										if (missingPackProperties.Contains(nameAttribute.Value)) {
											missingPackProperties.Remove(nameAttribute.Value);
										}
										packProperties.Add(nameAttribute.Value, propertyNode);
									}
								}
							}
						}
						if (child != null) { // child node may be empty (actuallly contains placeholder)
							if (missingPackProperties.Count > 0) {
								throw new KeyNotFoundException(
									string.Format("Missing packing properties ({0}) for child node: {1}", CreatePropertyList(missingPackProperties), childNode));
							}
							if (!PackWidget(widget, child, packProperties)) {
								throw new InvalidOperationException("Unable to pack widget: " + childNode);
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
		protected abstract T CreateWidget(IDictionary<string, XmlNode> properties);
		/// <summary>
		/// Packs the given child in the container widget.
		/// </summary>
		/// <returns><c>true</c>, if packing was successful, <c>false</c> otherwise.</returns>
		/// <param name="container">Container to place the child in.</param>
		/// <param name="child">Child widget to place in the container.</param>
		/// <param name="properties">Packing properties.</param>
		protected virtual bool PackWidget(T container, Gtk.Widget child, IDictionary<string, XmlNode> properties) {
			throw new InvalidOperationException(ClassName + " can not pack widgets");
		}
		/// <summary>
		/// Indicates whether the available properties contain all required properties.
		/// </summary>
		/// <returns><c>true</c>, if all required properties are available, <c>false</c> otherwise.</returns>
		/// <param name="available">Avaialable properties.</param>
		/// <param name="required">Required properties.</param>
		protected static bool CheckRequiredProperties(IDictionary<string, XmlNode> available, params string[] required) {
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
		protected static void EnsureRequiredProperties(IDictionary<string, XmlNode> available, params string[] required) {
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
		private static bool SetVisibility(Gtk.Widget widget, XmlNode property) {
			try {
				bool visible = TextParseTools.ParseBool(property.InnerText);
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
		private static bool SetFocusable(Gtk.Widget widget, XmlNode property) {
			try {
				bool focusable = TextParseTools.ParseBool(property.InnerText);
				widget.CanFocus = focusable;
				return true;
			} catch (FormatException) {
				return false;
			}
		}
		/// <summary>
		/// Creates a string that contains the given properties.
		/// </summary>
		/// <returns>The property list.</returns>
		/// <param name="properties">String entries.</param>
		private static string CreatePropertyList(IEnumerable<string> properties) {
			var result = new StringBuilder();
			var enumerator = properties.GetEnumerator();

			if (enumerator.MoveNext()) {
				result.Append(enumerator.Current);
			}
			while (enumerator.MoveNext()) {
				result.Append(", ");
				result.Append(enumerator.Current);
			}
			return result.ToString();
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

