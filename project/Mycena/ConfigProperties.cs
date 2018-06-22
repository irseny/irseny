using System;
using System.Xml;
using System.Collections.Generic;

namespace Mycena {
	internal class ConfigProperties {
		Dictionary<string, string> properties;
		Dictionary<string, string> attributes;
		LinkedList<string> markedProperties;
		LinkedList<string> markedAttributes;
		bool marking;

		public ConfigProperties() {
			properties = new Dictionary<string, string>(32);
			attributes = new Dictionary<string, string>(32);
			markedProperties = new LinkedList<string>();
			markedAttributes = new LinkedList<string>();
		}
		public ICollection<string> PropertyNames {
			get { return properties.Keys; }
		}
		public ICollection<string> MarkedProperties {
			get { return markedProperties; }
		}
		public ICollection<string> AttributeNames {
			get { return attributes.Keys; }
		}
		public ICollection<string> MarkedAttributes {
			get { return markedAttributes; }
		}
		public void RegisterProperty(XmlNode property) {
			if (property == null) throw new ArgumentNullException("property");
			if (property.Attributes == null) throw new ArgumentException("property: Has no attributes.");
			var nameAttr = property.Attributes["name"];
			if (nameAttr == null) {
				throw new ArgumentException("property: Missing name attribute: " + property.OuterXml);
			}
			string name = nameAttr.Value;
			if (property.InnerText == null) throw new ArgumentException("property: Empty value: " + property.OuterXml);
			if (properties.ContainsKey(name)) {
				properties[name] = property.InnerText;
			} else {
				properties.Add(name, property.InnerText);
			}
		}
		public void RegisterAttribute(XmlNode attribute) {
			if (attribute == null) throw new ArgumentNullException("attribute");
			if (attribute.Attributes == null) throw new ArgumentException("attribute: No attributes");
			var nameAttr = attribute.Attributes["name"];
			if (nameAttr == null) {
				throw new ArgumentException("attribute: Missing name attribute: " + attribute.OuterXml);
			}
			var valueAttr = attribute.Attributes["value"];
			if (valueAttr == null) {
				throw new ArgumentException("attribute: Missing value attribute: " + attribute.OuterXml);
			}
			RegisterAttribute(nameAttr.Value, valueAttr.Value);

		}
		public void RegisterAttribute(string name, string value) {
			if (name == null) throw new ArgumentNullException("name");
			if (value == null) throw new ArgumentNullException("value");
			if (attributes.ContainsKey(name)) {
				attributes[name] = value;
			} else {
				attributes.Add(name, value);
			}
		}
		public string GetProperty(string name) {
			if (name == null) throw new ArgumentNullException("name");
			string result;
			if (properties.TryGetValue(name, out result)) {
				if (marking) {
					markedProperties.AddLast(name);
				}
				return result;
			} else {
				throw new KeyNotFoundException("name");
			}
		}
		public string GetAttribute(string name) {
			if (name == null) throw new ArgumentNullException("name");
			string result;
			if (attributes.TryGetValue(name, out result)) {
				if (marking) {
					markedAttributes.AddLast(name);
				}
				return result;
			} else {
				throw new KeyNotFoundException("name");
			}
		}
		public string GetProperty<T>(string name, T defaultValue) {
			if (name == null) throw new ArgumentNullException("name");
			string result;
			if (properties.TryGetValue(name, out result)) {
				if (marking) {
					markedProperties.AddLast(name);
				}
				return result;
			} else {
				if (defaultValue == null) {
					return null;
				} else {
					return defaultValue.ToString();
				}

			}
		}
		public string GetAttribute<T>(string name, T defaultValue) {
			if (name == null) throw new ArgumentNullException("name");
			string result;
			if (attributes.TryGetValue(name, out result)) {
				if (marking) {
					markedAttributes.AddLast(name);
				}
				return result;
			} else {
				if (defaultValue == null) {
					return null;
				} else {
					return defaultValue.ToString();
				}

			}
		}
		public string GetProperty(string name, string defaultValue) {
			return GetProperty<string>(name, defaultValue);
		}
		public string GetAttribute(string name, string defaultValue) {
			return GetAttribute<string>(name, defaultValue);
		}
		public bool TryGetProperty(string name, out string result) {
			if (name == null) throw new ArgumentNullException("name");
			if (properties.TryGetValue(name, out result)) {
				if (marking) {
					markedProperties.AddLast(name);
				}
				return true;
			} else {
				return false;
			}
		}
		public bool TryGetAttribute(string name, out string result) {
			if (name == null) throw new ArgumentNullException("name");
			if (properties.TryGetValue(name, out result)) {
				if (marking) {
					markedAttributes.AddLast(name);
				}
				return true;
			} else {
				return false;
			}
		}
		public void BeginMark() {
			if (!marking) {
				markedAttributes.Clear();
				markedProperties.Clear();
				marking = true;
			}
		}
		public void EndMark() {
			marking = false;
		}
	}
}

