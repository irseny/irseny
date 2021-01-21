// This file is part of Irseny.
//
// Copyright (C) 2021  Thilo Gabel
//
// Irseny is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
//
// Irseny is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
//
// You should have received a copy of the GNU General Public License
// along with this program.  If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;
using Irseny.Core.Util;
using System.Globalization;

namespace Irseny.Core.Util {
	public class EquipmentSettings {

		Dictionary<int, decimal> decimalProps;
		Dictionary<int, int> integerProps;
		Dictionary<int, string> textProps;

		public Type PropertyType { get; private set; }

		public EquipmentSettings(Type propertyType) {
			if (propertyType == null) throw new ArgumentNullException("propertyType");
			if (!typeof(IConvertible).IsAssignableFrom(propertyType)) {
				throw new ArgumentException("Not convertible", "propertyType");
			}
			PropertyType = propertyType;
			decimalProps = new Dictionary<int, decimal>(16);
			integerProps = new Dictionary<int, int>(16);
			textProps = new Dictionary<int, string>(16);
		}
		public EquipmentSettings(EquipmentSettings source) : this(source == null ? typeof(IConvertible) : source.PropertyType) {
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
			int prop = property.ToInt32(JsonString.FormatProvider);
			integerProps[prop] = value;
		}
		public decimal GetDecimal<T>(T property, decimal fallback) where T : IConvertible {
			int prop = property.ToInt32(JsonString.FormatProvider);
			decimal result;
			if (!decimalProps.TryGetValue(prop, out result)) {
				return fallback;
			}
			return result;
		}
		public void SetDecimal<T>(T property, decimal value) where T : IConvertible {
			int prop = property.ToInt32(JsonString.FormatProvider);
			decimalProps[prop] = value;
		}
		public string GetText<T>(T property, string fallback) where T : IConvertible {
			int prop = property.ToInt32(JsonString.FormatProvider);
			string result;
			if (!textProps.TryGetValue(prop, out result)) {
				return fallback;
			}
			return result;
		}
		public void SetText<T>(T property, string text) where T : IConvertible {
			if (text == null) throw new ArgumentNullException("text");
			int prop = property.ToInt32(JsonString.FormatProvider);
			textProps[prop] = text;
		}

		public void SetAuto<T>(T property) where T : IConvertible {
			int prop = property.ToInt32(JsonString.FormatProvider);
			decimalProps.Remove(prop);
			integerProps.Remove(prop);
			textProps.Remove(prop);
		}
		public bool IsAuto<T>(T property) where T : IConvertible {
			int prop = property.ToInt32(JsonString.FormatProvider);
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
		public static JsonString ToJson(EquipmentSettings source) {
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
					int prop = Convert.ToInt32(props.GetValue(i), JsonString.FormatProvider);
					decimal mProp;
					int iProp;
					string sProp;
					if (source.integerProps.TryGetValue(prop, out iProp)) {
						result.AddTerminal(term, JsonString.Stringify(iProp));
					} else if (source.decimalProps.TryGetValue(prop, out mProp)) {
						result.AddTerminal(term, JsonString.Stringify(mProp));
					} else if (source.textProps.TryGetValue(prop, out sProp)) {
						result.AddTerminal(term, JsonString.StringifyString(sProp));
					}
				}
			}
			return result;
		}
		public static EquipmentSettings FromJson(JsonString source, Type propertyType) {
			if (source == null) throw new ArgumentNullException("source");
			if (source.Type != JsonStringType.Dict) throw new ArgumentException("Expected a JSON dictionary", "source");
			if (propertyType == null) throw new ArgumentNullException("propertyType");
			if (!propertyType.IsEnum) throw new ArgumentException("Must be an enum type", "propertyType");

			var result = new EquipmentSettings(propertyType);
			string type = JsonString.ParseString(source.GetTerminal("type", string.Empty), string.Empty);
			//T type = ParseProperty(sType);
			bool running = JsonString.ParseBool(source.GetTerminal("running", "false"), false);
			string name = JsonString.ParseString(source.GetTerminal("name", string.Empty), null);

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
				int prop = Convert.ToInt32(props.GetValue(i), JsonString.FormatProvider);
				decimal mProp;
				int iProp;
				bool bProp;
				if (JsonString.IsQuoted(value)) {
					string text = JsonString.ParseString(value, null);
					result.textProps.Add(prop, text);
				} else if (JsonString.TryParseInt(value, out iProp)) {
					result.integerProps.Add(prop, iProp);
				} else if (JsonString.TryParseDecimal(value, out mProp)) {
					result.decimalProps.Add(prop, mProp);
				} else if (JsonString.TryParseBool(value, out bProp)) {
					result.integerProps.Add(prop, bProp ? 1 : 0);
				}
			}
			return result;
		}
	}
}
