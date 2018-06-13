using System;
using System.Xml;
using System.Collections.Generic;

namespace Mycena {
	public class ConfigProperties {
		Dictionary<string, string> properties;
		Dictionary<string, string> attributes;

		public ConfigProperties() {
			properties = new Dictionary<string, string>(32);
			attributes = new Dictionary<string, string>(32);
		}
		public ICollection<string> PropertyNames {
			get { return properties.Keys; }
		}
		public ICollection<string> AttributeNames {
			get { return attributes.Keys; }
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
			string name = nameAttr.Value;
			if (attributes.ContainsKey(name)) {
				attributes[name] = valueAttr.Value;
			} else {
				attributes.Add(name, valueAttr.Value);
			}
		}
		public string GetProperty(string name) {
			if (name == null) throw new ArgumentNullException("name");
			string result;
			if (properties.TryGetValue(name, out result)) {
				return result;
			} else {
				throw new KeyNotFoundException("name");
			}
		}
		public string GetAttribute(string name) {
			if (name == null) throw new ArgumentNullException("name");
			string result;
			if (attributes.TryGetValue(name, out result)) {
				return result;
			} else {
				throw new KeyNotFoundException("name");
			}
		}
		public string GetProperty<T>(string name, T defaultValue) {
			if (name == null) throw new ArgumentNullException("name");
			string result;
			if (properties.TryGetValue(name, out result)) {
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
			return properties.TryGetValue(name, out result);
		}
		public bool TryGetAttribute(string name, out string result) {
			if (name == null) throw new ArgumentNullException("name");
			return properties.TryGetValue(name, out result);
		}

	}
}

