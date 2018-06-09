using System;
using System.Xml;
using System.Collections.Generic;

namespace Mycena {
	public class ConfigProperties {
		Dictionary<string, string> properties;

		public ConfigProperties() {
			properties = new Dictionary<string, string>(32);
		}
		public string this[string name] {
			get { return GetProperty(name); }
		}
		public string this[string name, string defaultValue] {
			get { return GetProperty(name, defaultValue); }
		}
		public ICollection<string> PropertyNames {
			get { return properties.Keys; }
		}
		public void RegisterProperty(XmlNode property) {
			if (property.Name == null || property.Name.Length == 0) throw new ArgumentException("propery: Missing name: " + property.OuterXml);
			if (property.InnerText == null || property.InnerText.Length == 0) throw new ArgumentException("property: Empty value: " + property.OuterXml);
			if (properties.ContainsKey(property.Name)) {
				properties[property.Name] = property.InnerText;
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
		public string GetProperty(string name, string defaultValue) {
			if (name == null) throw new ArgumentNullException("name");
			string result;
			if (properties.TryGetValue(name, out result)) {
				return result;
			} else {
				return defaultValue;
			}
		}
		public bool TryGetProperty(string name, out string result) {
			if (name == null) throw new ArgumentNullException("name");
			return properties.TryGetValue(name, out result);
		}


	}
}

