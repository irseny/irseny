using System;
using System.Collections.Generic;
using Irseny.Core.Util;
using System.Globalization;

namespace Irseny.Core.Sensors {
	public class SensorSettings {

		Dictionary<int, decimal> decimalProps;
		Dictionary<int, int> integerProps;
		Dictionary<int, string> textProps;

		public Type PropertyType { get; private set; }

		public SensorSettings(Type propertyType) {
			if (propertyType == null) throw new ArgumentNullException("propertyType");
			if (!typeof(IConvertible).IsAssignableFrom(propertyType)) {
				throw new ArgumentException("Not convertible", "propertyType");
			}
			PropertyType = propertyType;
			decimalProps = new Dictionary<int, decimal>(16);
			integerProps = new Dictionary<int, int>(16);
			textProps = new Dictionary<int, string>(16);
		}
		public SensorSettings(SensorSettings source) : this(source == null ? typeof(IConvertible) : source.PropertyType) {
			if (source == null) throw new ArgumentNullException("source");
			foreach (var pair in source.decimalProps) {
				this.decimalProps.Add(pair.Key, pair.Value);
			}
			foreach (var pair in source.integerProps) {
				this.integerProps.Add(pair.Key, pair.Value);
			}
			foreach (var pair in source.textProps) {
				this.textProps.Add(pair.Key, pair.Value);
			}
		}

		public int GetInteger<T>(T prop, int fallback) where T : IConvertible {
			int key = prop.ToInt32(null);
			int result;
			if (!integerProps.TryGetValue(key, out result)) {
				return fallback;
			}
			return result;
		}
		public void SetInteger<T>(T property, int value) where T : IConvertible {
			int prop = property.ToInt32(TextParseTools.FormatProvider);
			integerProps[prop] = value;
		}
		public decimal GetDecimal<T>(T property, decimal fallback) where T : IConvertible {
			int prop = property.ToInt32(TextParseTools.FormatProvider);
			decimal result;
			if (!decimalProps.TryGetValue(prop, out result)) {
				return fallback;
			}
			return result;
		}
		public void SetDecimal<T>(T property, decimal value) where T : IConvertible {
			int prop = property.ToInt32(TextParseTools.FormatProvider);
			decimalProps[prop] = value;
		}
		public string GetText<T>(T property, string fallback) where T : IConvertible {
			int prop = property.ToInt32(TextParseTools.FormatProvider);
			string result;
			if (!textProps.TryGetValue(prop, out result)) {
				return fallback;
			}
			return result;
		}
		public void SetText<T>(T property, string text) where T : IConvertible {
			if (text == null) throw new ArgumentNullException("text");
			int prop = property.ToInt32(TextParseTools.FormatProvider);
			textProps[prop] = text;
		}

		public void SetAuto<T>(T property) where T : IConvertible {
			int prop = property.ToInt32(TextParseTools.FormatProvider);
			decimalProps.Remove(prop);
			integerProps.Remove(prop);
			textProps.Remove(prop);
		}
		public bool IsAuto<T>(T property) where T : IConvertible {
			int prop = property.ToInt32(TextParseTools.FormatProvider);
			if (integerProps.ContainsKey(prop)) {
				return false;
			}
			if (decimalProps.ContainsKey(prop)) {
				return false;
			}
			if (textProps.ContainsKey(prop)) {
				return false;
			}
			return true;
		}
		public static JsonString ToJson(SensorSettings source) {
			if (source == null) throw new ArgumentNullException("source");
			if (!source.PropertyType.IsEnum) throw new ArgumentException("Must have enum properties", "source");
			var result = JsonString.CreateDict();
			{
				// TODO move type setting outside
				// result.AddTerminal("type", StringifyTools.StringifyString("Webcam"));
				string[] names = Enum.GetNames(source.PropertyType);
				Array props = Enum.GetValues(source.PropertyType);


				// go through all properties and add existing ones to JSON
				for (int i = 0; i < names.Length; i++) {
					string term = names[i].ToLowerCamelCase();
					int prop = Convert.ToInt32(props.GetValue(i), TextParseTools.FormatProvider);
					decimal mProp;
					int iProp;
					string sProp;
					if (source.integerProps.TryGetValue(prop, out iProp)) {
						result.AddTerminal(term, StringifyTools.StringifyInt(iProp));
					} else if (source.decimalProps.TryGetValue(prop, out mProp)) {
						result.AddTerminal(term, StringifyTools.Stringify(mProp));
					} else if (source.textProps.TryGetValue(prop, out sProp)) {
						result.AddTerminal(term, StringifyTools.StringifyString(sProp));
					}
				}
			}
			return result;
		}
		public static SensorSettings FromJson(JsonString source, Type propertyType) {
			if (source == null) throw new ArgumentNullException("source");
			if (source.Type != JsonStringType.Dict) throw new ArgumentException("Expected a JSON dictionary", "source");
			if (propertyType == null) throw new ArgumentNullException("propertyType");
			if (!propertyType.IsEnum) throw new ArgumentException("Must be an enum type", "propertyType");

			var result = new SensorSettings(propertyType);
			string type = TextParseTools.ParseString(source.GetTerminal("type", string.Empty), string.Empty);
			//T type = ParseProperty(sType);
			bool running = TextParseTools.ParseBool(source.GetTerminal("running", "false"), false);
			string name = TextParseTools.ParseString(source.GetTerminal("name", string.Empty), null);

			// get all enum entries
			// and convert to camelcase as used in javascript
			// note that this may result in unexpected results for certain edge cases
			string[] names = Enum.GetNames(propertyType);
			Array props = Enum.GetValues(propertyType);


			// go through all enum entries and adopt existing data
			for (int i = 0; i < names.Length; i++) {
				string term = names[i].ToLowerCamelCase();
				string value = source.GetTerminal(term, string.Empty);
				if (value.Length == 0) {
					continue;
				}
				int prop = Convert.ToInt32(props.GetValue(i), TextParseTools.FormatProvider);
				decimal mProp;
				int iProp;
				bool bProp;
				if (TextParseTools.IsQuoted(value)) {
					string text = TextParseTools.ParseString(value, null);
					result.textProps.Add(prop, text);
				} else if (TextParseTools.TryParseInt(value, out iProp)) {
					result.integerProps.Add(prop, iProp);
				} else if (TextParseTools.TryParseDecimal(value, out mProp)) {
					result.decimalProps.Add(prop, mProp);
				} else if (TextParseTools.TryParseBool(value, out bProp)) {
					result.integerProps.Add(prop, bProp ? 1 : 0);
				}
			}
			return result;
		}
	}



	/// <summary>
	/// Generic settings for sensor configuration.
	/// </summary>
//	public class SensorSettings<T> where T : IConvertible {
//		static readonly string[] PropertyNames;
//		static readonly T[] PropertyValues;
//		static readonly Dictionary<string, T> PropertyMap;
//
//
//
//
//		Dictionary<T, double> fProps;
//		Dictionary<T, int> iProps;
//
//		public bool Running { get; set; }
//		public string Name { get; set; }
//
//		static SensorSettings() {
//			PropertyNames = Enum.GetNames(typeof(T));
//			PropertyValues = (T[])Enum.GetValues(typeof(T));
//			PropertyMap = new Dictionary<string, T>(Math.Max(16, PropertyNames.Length*2), StringComparer.InvariantCultureIgnoreCase);
//			for (int i = 0; i < PropertyNames.Length; i++) {
//				PropertyMap.Add(PropertyNames[i], PropertyValues[i]);
//			}
//		}
//
//		public SensorSettings() {
//			// leave everything on auto
//			fProps = new Dictionary<T, double>(16);
//			iProps = new Dictionary<T, int>(16);
//			Running = false;
//			Name = string.Empty;
//		}
//
//		public SensorSettings(SensorSettings<T> source) : this() {
//			if (source == null) throw new ArgumentNullException("source");
//			this.Running = source.Running;
//			this.Name = source.Name;
//			foreach (var pair in source.fProps) {
//				this.fProps.Add(pair.Key, pair.Value);
//			}
//			foreach (var pair in source.iProps) {
//				this.iProps.Add(pair.Key, pair.Value);
//			}
//		}
//		public int GetInteger(T prop, int fallback) {
//			int result;
//			if (!iProps.TryGetValue(prop, out result)) {
//				return fallback;
//			}
//			return result;
//		}
//		public void SetInteger(T prop, int value) {
//			iProps[prop] = value;
//		}
//		public double GetDecimal(T prop, double fallback) {
//			double result;
//			if (!fProps.TryGetValue(prop, out result)) {
//				return fallback;
//			}
//			return result;
//		}
//		public void SetDecimal(T prop, double value) {
//			prop.ToInt32(CultureInfo.InvariantCulture);
//
//			fProps[prop] = value;
//		}
//		public void SetAuto(T prop) {
//			if (fProps.ContainsKey(prop)) {
//				fProps.Remove(prop);
//			}
//			if (iProps.ContainsKey(prop)) {
//				iProps.Remove(prop);
//			}
//		}
//		public bool IsAuto(T prop) {
//			return !fProps.ContainsKey(prop) && !iProps.ContainsKey(prop);
//		}
//		public JsonString ToJson() {
//			var result = JsonString.CreateDict();
//			{
//				result.AddTerminal("type", StringifyTools.StringifyString("Webcam"));
//				result.AddTerminal("running", StringifyTools.StringifyBool(Running));
//				result.AddTerminal("name", StringifyTools.StringifyString(Name));
//				foreach (T prop in PropertyValues) {
//					string term = prop.ToString().ToLowerCamelCase();
//					double fProp;
//					int iProp;
//
//					if (fProps.TryGetValue(prop, out fProp)) {
//						result.AddTerminal(term, StringifyTools.StringifyDouble(fProp));
//					} else if (iProps.TryGetValue(prop, out iProp)) {
//						result.AddTerminal(term, StringifyTools.StringifyInt(iProp));
//					}
//				}
//			}
//			return result;
//		}
//		public static SensorSettings<T> FromJson(JsonString source) {
//			if (source == null) throw new ArgumentNullException("source");
//			if (source.Type != JsonStringType.Dict) throw new ArgumentException("Expected a JSON dictionary", "source");
//			var result = new SensorSettings<T>();
//			string type = TextParseTools.ParseString(source.GetTerminal("type", string.Empty), string.Empty);
//			//T type = ParseProperty(sType);
//			bool running = TextParseTools.ParseBool(source.GetTerminal("running", "false"), false);
//			string name = TextParseTools.ParseString(source.GetTerminal("name", string.Empty), null);
//
//			result.Name = name;
//			result.Running = running;
//
//			foreach (var pair in source.Dict) {
//				T prop;
//				try {
//					prop = ParseProperty(pair.Key);
//				} catch (FormatException) {
//					// not a parsable property -> skip
//					continue;
//				}
//				// test for parsable content
//				string value = pair.Value as string;
//				if (value == null) {
//					continue;
//				}
//				// test value for integer and float parsability
//				int iProp;
//				double fProp;
//				if (TextParseTools.TryParseInt(value, out iProp)) {
//					result.SetInteger(prop, iProp);
//				} else if (TextParseTools.TryParseDouble(value, out fProp)) {
//					result.SetDecimal(prop, fProp);
//				} else {
//					// skip unsupported content
//					// TODO add more configuration options
//				}
//			}
//
//			return result;
//		}
//		public static T ParseProperty(string type) {
//			if (type == null) throw new ArgumentNullException("type");
//
//			T result;
//			if (!PropertyMap.TryGetValue(type, out result)) {
//				throw new FormatException();
//			}
//			return result;
//		}
//	}
}
