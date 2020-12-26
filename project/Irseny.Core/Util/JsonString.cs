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
				if (Type != JsonStringType.Dict) throw new InvalidOperationException("Not a dictionary JSON string");
				return dict;
			}
		}
		public IReadOnlyList<object> Array {
			get {
				if (Type != JsonStringType.Array) throw new InvalidOperationException("Not an array JSON string");
				return array;
			}
		}
		public JsonString GetJsonString(params object[] path) {
			if (path == null) throw new ArgumentNullException("path");
			return GetJsonString(0, path);
		}
		public JsonString TryGetJsonString(params object[] path) {
			if (path == null) throw new ArgumentNullException("path");
			try {
				return GetJsonString(0, path);
			} catch (ArgumentException) {
			} catch (InvalidOperationException) {
			} catch (KeyNotFoundException) {
			} 
			return null;
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
					
					throw new ArgumentException(string.Format("Expected an integer for path[{0}]={1}",
						startAt, path[startAt]));
				}
				int index = (int)path[startAt];
				if (index < 0 || index >= array.Count) {
					throw new ArgumentException(string.Format("Index path[{0}]={1} is out of index range [0...{2}]",
						startAt, index, array.Count - 1));
				}
				value = array[index];
				break;
			case JsonStringType.Dict:
				// read the next child from a dictionary
				if (!(path[startAt] is string)) {
					throw new ArgumentException(string.Format("Expected a string for path[{0}]={1}",
						startAt, path[startAt]));
				}
				string key = (string)path[startAt];
				if (!dict.TryGetValue(key, out value)) {
					throw new ArgumentException(string.Format("Key path[{0}]={1} does not exist",
						startAt, path[startAt]));
				}
				break;
			default:
				throw new InvalidOperationException("Unknown string type: " + Type);
			}
			// test for terminals and call recursively
			var child = (value as JsonString);
			if (child == null) {
				throw new ArgumentException(string.Format("Unexpected terminal {0} on path[{1}]={2}",
					value, startAt, path[startAt]));
			}
			return child.GetJsonString(startAt + 1, path);
		}
		public string GetTerminal(string key, string fallback=null) {
			// test for method applicability
			if (Type != JsonStringType.Dict) {
				throw new InvalidOperationException(string.Format("String indexing not supported on {0} JSON strings",
					Type));
			}
			if (key == null) throw new ArgumentNullException("key");
			// test for out of range
			object value;
			if (!dict.TryGetValue(key, out value)) {
				if (fallback == null) {
					throw new KeyNotFoundException(string.Format("Key {0} not found", 
						key));
				} else {
					return fallback;
				}
			}
			// test for terminal
			string result = (value as string);
			if (result == null) {
				if (fallback == null) {
					throw new InvalidOperationException(string.Format("Deposited value {0} is not a terminal",
						value));
				} else {
					return fallback;
				}
			}
			return result;
		}
		public string GetTerminal(int index, string fallback=null) {
			// test for method applicability
			if (Type != JsonStringType.Array) {
				if (fallback == null) {
					throw new InvalidOperationException(string.Format("Integer indexing not supported on {0} JSON strings",
						Type));
				} else {
					return fallback;
				}
			}
			// test for out of range
			if (index < 0 || index >= array.Count) {
				if (fallback == null) {
					throw new ArgumentOutOfRangeException(string.Format("Index {0} is out of range [0...{1}]",
						index, array.Count - 1));
				} else {
					return fallback;
				}
			}
			string result = (array[index] as string);
			// test for terminal
			if (result == null) {
				if (fallback == null) {
					throw new InvalidOperationException(string.Format("Deposited value {0} is not a terminal",
						array[index]));
				} else {
					return fallback;
				}
			}
			return result;
		}
		public bool AddTerminal(string key, string value, bool overwrite=true) {
			if (key == null) throw new ArgumentNullException("key");
			if (value == null) throw new ArgumentNullException("value");
			switch (Type) {
			case JsonStringType.Array:
				array.Add(value);
				return true;
			case JsonStringType.Dict:
				if (overwrite) {
					dict[key] = value;
				} else {
					try {
						dict.Add(key, value);
					} catch (ArgumentException) {
						return false;
					}
				}
				return true;
			default:
				return false;
			}
		}
		public bool AddJsonString(string key, JsonString value, bool overwrite=true) {
			if (key == null) throw new ArgumentNullException("key");
			if (value == null) throw new ArgumentNullException("value");
			switch (Type) {
			case JsonStringType.Array:
				array.Add(value);
				return true;
			case JsonStringType.Dict:
				if (overwrite) {
					dict[key] = value;
				} else {
					try {
						dict.Add(key, value);
					} catch (ArgumentException) {
						return false;
					}
				}
				return true;
			default:
				return false;
			}
		}
		public JsonString RemoveJsonString(string key, bool failHard=true) {
			if (key == null) throw new ArgumentNullException("key");
			if (Type != JsonStringType.Dict) {
				if (failHard) {
					throw new InvalidOperationException(string.Format("String indexing not supported on {0} JSON strings",
						Type));
				} else {
					return null;
				}
			}
			object value;
			if (!dict.TryGetValue(key, out value)) {
				if (failHard) {
					throw new KeyNotFoundException(string.Format("Key {0} not found",
						key));
				} else {
					return null;
				}
			}
			JsonString result = value as JsonString;
			if (result == null) {
				if (failHard) {
					throw new InvalidOperationException(string.Format("Deposited value {0} is not a JsonString",
						value));
				} else {
					return null;
				}
			}
			dict.Remove(key);
			return result;
			
		}
		public JsonString RemoveJsonString(int index, bool failHard=true) {
			if (Type != JsonStringType.Array) {
				if (failHard) {
					throw new InvalidOperationException(string.Format("Integer indexing not supported on {0} JSON strings",
						Type));
				} else {
					return null;
				}
			}
			if (index < 0 || index >= array.Count) {
				if (failHard) {
					throw new ArgumentOutOfRangeException(string.Format("Index {0} is out of range [0...{1}]",
						index, array.Count - 1));
				} else {
					return null;
				}
			}
			object value = array[index];
			JsonString result = value as JsonString;
			if (result == null) {
				if (failHard) {
					throw new InvalidOperationException(string.Format("Deposited value {0} is not a JsonString",
						value));
				} else {
					return null;
				}
			}
			array.RemoveAt(index);
			return result;
		}
		public string RemoveTerminal(string key, bool failHard=true) {
			if (key == null) throw new ArgumentNullException("key");
			if (Type != JsonStringType.Dict) {
				if (failHard) {
					throw new InvalidOperationException(string.Format("String indexing not supported on {0} JSON strings",
						Type));
				} else {
					return null;
				}
			}
			object value;
			if (!dict.TryGetValue(key, out value)) {
				if (failHard) {
					throw new KeyNotFoundException(string.Format("Key {0} not found",
						key));
				} else {
					return null;
				}
			}
			string result = value as string;
			if (result == null) {
				if (failHard) {
					throw new InvalidOperationException(string.Format("Deposited value {0} is not a string",
						value));
				} else {
					return null;
				}
			}
			dict.Remove(key);
			return result;

		}
		public string RemoveTerminal(int index, bool failHard=true) {
			if (Type != JsonStringType.Array) {
				if (failHard) {
					throw new InvalidOperationException(string.Format("Integer indexing not supported on {0} JSON strings",
						Type));
				} else {
					return null;
				}
			}
			if (index < 0 || index >= array.Count) {
				if (failHard) {
					throw new ArgumentOutOfRangeException(string.Format("Index {0} is out of range [0...{1}]",
						index, array.Count - 1));
				} else {
					return null;
				}
			}
			object value = array[index];
			string result = value as string;
			if (result == null) {
				if (failHard) {
					throw new InvalidOperationException(string.Format("Deposited value {0} is not a JsonString",
						value));
				} else {
					return null;
				}
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
				throw new InvalidOperationException();
			}
		}

		public override string ToString () {
			var result = new StringBuilder();
			ToCompressedString(result);
			return result.ToString();
		}
		private void ToCompressedString(StringBuilder result) {
			switch (Type) {
			case JsonStringType.Dict:
				result.Append("{");
				if (dict.Count > 0) {
					foreach (var pair in dict) {
						result.Append('"').Append(pair.Key).Append("\":");

						if (pair.Value is JsonString) {
							(pair.Value as JsonString).ToCompressedString(result);
						} else if (pair.Value is string) {
							// TODO detect real type and handle quotes accordingly
							result.Append((string)pair.Value);
						} else {
							// invalid, omit
						}
						result.Append(",");
					}
					result.Remove(result.Length - 1, 1);
				}
				result.Append("}");
				break;
			case JsonStringType.Array:
				result.Append("[");
				if (array.Count > 0) {
					foreach (object item in array) {
						if (item is JsonString) {
							(item as JsonString).ToCompressedString(result);
						} else if (item is string) {
							// TODO detect type and handle quotes properly
							result.Append((string)item);
						} else {
							// invalid, omit
						}
						result.Append(",");
					}
					result.Remove(result.Length - 1, 1);
				}
				result.Append("]");
				break;
			}
		}

		public string ToJsonString() {
			var result = new StringBuilder();
			ToJsonString(result, 0);
			return result.ToString();
		}
		private void ToJsonString(StringBuilder result, int indent) {	
			switch (Type) {
			case JsonStringType.Dict:
				// expects the correct indentation in the current line
				result.AppendLine("{");
				if (dict.Count > 0) {
					indent += 1;
					foreach (var pair in dict) {
						result.Append('\t', indent);
						result.Append('"').Append(pair.Key).Append("\": ");
						if (pair.Value is JsonString) {
							(pair.Value as JsonString).ToJsonString(result, indent);
						} else if (pair.Value is string) {
							// TODO properly handle quotes
							result.Append((string)pair.Value);
						} else {
							// invalid, omit
						}
						result.AppendLine(",");
					}
					indent -= 1;
					// need to remove the ,\n of the last value
					// TODO test if this can be different to \n
					result.Remove(result.Length - 2, 2);
					result.AppendLine();
				}
				result.Append('\t', indent).Append('}');
				// leave it to the parent to add more to the last line
				break;
			case JsonStringType.Array:
				// expect correct indentation
				result.AppendLine("[");
				if (array.Count > 0) {
					indent += 1;
					foreach (var item in array) {
						result.Append('\t', indent);
						if (item is JsonString) {
							(item as JsonString).ToJsonString(result, indent);
						} else if (item is string) {
							// TODO add quotes if necessary
							result.Append(item);
						} else {
							// invalid, omit
						}
						result.AppendLine(",");
					}
					indent -= 1;
					// remove the last , after all written values
					result.Remove(result.Length - 2, 2);
					result.AppendLine();
				}
				result.Append('\t', indent).Append(']');
				// further line content added by the parent
				break;
			}

		}
		public static JsonString Parse(string source) {
			List<string> parts = PartitionJson(source);
			return InterpretJson(parts);
		}
		public static JsonString CreateArray() {
			return new JsonString(JsonStringType.Array);
		}
		public static JsonString CreateDict() {
			return new JsonString(JsonStringType.Dict);
		}
	}
}

