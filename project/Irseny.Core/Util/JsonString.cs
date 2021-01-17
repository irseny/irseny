using System;
using System.Text;
using System.Collections.Generic;

namespace Irseny.Core.Util {
	public partial class JsonString {
		Dictionary<string, object> dict = null;
		List<object> array = null;

		public JsonString(JsonStringType type) {
			this.Type = type;
			switch (type) {
			case JsonStringType.Dict:
				this.dict = new Dictionary<string, object>();
				break;
			case JsonStringType.Array:
				this.array = new List<object>();
				break;
			}
		}
		public JsonString(JsonString source) {
			if (source == null) throw new ArgumentNullException("source");
			this.Type = source.Type;
			switch (source.Type) {
			case JsonStringType.Array:
				this.array = new List<object>();
				foreach (var entry in source.array) {
					if (entry != null) {
						if (entry is string) {
							this.array.Add(entry);
						} else if (entry is JsonString) {
							this.array.Add(new JsonString((JsonString)entry));
						}
					}
				}
				break;
			case JsonStringType.Dict:
				this.dict = new Dictionary<string, object>();
				foreach (var pair in source.dict) {
					if (pair.Key != null && pair.Value != null) {
						if (pair.Value is string) {
							this.dict.Add(pair.Key, pair.Value);
						} else if (pair.Value is JsonString) {
							this.dict.Add(pair.Key, new JsonString((JsonString)pair.Value));
						}
					}
				}
				break;
			}
		}


		public JsonStringType Type { get; private set; }

		public IReadOnlyDictionary<string, object> Dict { 
			get {
				if (Type != JsonStringType.Dict) {
					return new Dictionary<string, object>(0);
				}
				return dict;
			}
		}
		public IReadOnlyList<object> Array {
			get {
				if (Type != JsonStringType.Array) {
					return new List<object>(0);
				}
				return array;
			}
		}

		public JsonString GetJsonString(params object[] path) {
			if (path == null) throw new ArgumentNullException("path");
			return GetJsonString(0, path);
		}
		private JsonString GetJsonString(int startAt, object[] path) {
			if (startAt >= path.Length) {
				return this;
			}
			object value;
			switch (Type) {
			case JsonStringType.Array:
				// read the next child from an array
				if (!(path[startAt] is int)) {
					return null;
				}
				int index = (int)path[startAt];
				if (index < 0 || index >= array.Count) {
					return null;
				}
				value = array[index];
				break;
			case JsonStringType.Dict:
				// read the next child from a dictionary
				if (!(path[startAt] is string)) {
					return null;
				}
				string key = (string)path[startAt];
				if (!dict.TryGetValue(key, out value)) {
					return null;
				}
				break;
			default:
				return null;
			}
			// test for terminals and call recursively
			var child = (value as JsonString);
			if (child == null) {
				// unexpected terminal
				return null;
			}
			return child.GetJsonString(startAt + 1, path);
		}
		public string GetTerminal(string key, string fallback=null) {
			if (key == null) throw new ArgumentNullException("key");
			// test for method applicability
			if (Type != JsonStringType.Dict) {
				return fallback;
			}
			// test for out of range
			object value;
			if (!dict.TryGetValue(key, out value)) {
				return fallback;
			}
			// test for terminal
			string result = (value as string);
			if (result == null) {
				return fallback;
			}
			return result;
		}
		public string GetTerminal(int index, string fallback=null) {
			// test for method applicability
			if (Type != JsonStringType.Array) {
				return fallback;
			}
			// test for out of range
			if (index < 0 || index >= array.Count) {
				return fallback;
			}
			string result = (array[index] as string);
			// test for terminal
			if (result == null) {
				return fallback;
			}
			return result;
		}
		public void AddTerminal(string key, string value) {
			if (key == null) throw new ArgumentNullException("key");
			if (value == null) throw new ArgumentNullException("value");
			switch (Type) {
			case JsonStringType.Array:
				array.Add(value);
			break;
			case JsonStringType.Dict:
				dict[key] = value;
			break;
			default:
			break;
			}
		}
		public void AddJsonString(string key, JsonString value) {
			if (key == null) throw new ArgumentNullException("key");
			if (value == null) throw new ArgumentNullException("value");
			switch (Type) {
			case JsonStringType.Array:
				array.Add(value);
			break;
			case JsonStringType.Dict:
				dict[key] = value;
			break;
			default:
			break;
			}
		}
		public JsonString RemoveJsonString(string key) {
			if (key == null) throw new ArgumentNullException("key");
			// test for applicability
			if (Type != JsonStringType.Dict) {
				return null;
			}
			object value;
			// test for out of range
			if (!dict.TryGetValue(key, out value)) {
				return null;
			}
			JsonString result = value as JsonString;
			// test type
			if (result == null) {
				return null;
			}
			dict.Remove(key);
			return result;
			
		}
		public JsonString RemoveJsonString(int index) {
			// test for applicability
			if (Type != JsonStringType.Array) {
				return null;
			}
			// test for out of range
			if (index < 0 || index >= array.Count) {
				return null;
			}
			object value = array[index];
			// test for no terminal
			JsonString result = value as JsonString;
			if (result == null) {
				return null;
			}
			array.RemoveAt(index);
			return result;
		}
		public string RemoveTerminal(string key) {
			if (key == null) throw new ArgumentNullException("key");
			// test for applicability
			if (Type != JsonStringType.Dict) {
				return null;
			}
			object value;
			// test for out of range
			if (!dict.TryGetValue(key, out value)) {
				return null;
			}
			// test for terminal
			string result = value as string;
			if (result == null) {
				return null;
			}
			dict.Remove(key);
			return result;

		}
		public string RemoveTerminal(int index) {
			// test for applicability
			if (Type != JsonStringType.Array) {
				return null;
			}
			// test for out of range
			if (index < 0 || index >= array.Count) {
				return null;
			}
			object value = array[index];
			// test for terminal
			string result = value as string;
			if (result == null) {
				return null;
			}
			array.RemoveAt(index);
			return result;
		}

		public void Clear() {
			switch (Type) {
			case JsonStringType.Array:
				array.Clear();
			break;
			case JsonStringType.Dict:
				dict.Clear();
			break;
			default:
			break;
			}
		}


		public static JsonString CreateArray() {
			return new JsonString(JsonStringType.Array);
		}
		public static JsonString CreateDict() {
			return new JsonString(JsonStringType.Dict);
		}
	}
}

