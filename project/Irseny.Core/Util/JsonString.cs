using System;
using System.Text;
using System.Collections.Generic;

namespace Irseny.Core.Util {
	public partial class JsonString {
		Dictionary<string, object> dict = null;
		List<object> array = null;

		public JsonString (JsonStringType type) {
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
						startAt, path[startAt]), "path");
				}
				int index = (int)path[startAt];
				if (index < 0 || index >= array.Count) {
					throw new ArgumentException(string.Format("Index path[{0}]={1} is out of index range [0...{2}]",
						startAt, index, array.Count - 1), "path");
				}
				value = array[index];
				break;
			case JsonStringType.Dict:
				// read the next child from a dictionary
				if (!(path[startAt] is string)) {
					throw new ArgumentException(string.Format("Expected a string for path[{0}]={1}",
						startAt, path[startAt]), "path");
				}
				string key = (string)path[startAt];
				if (!dict.TryGetValue("key", out value)) {
					throw new ArgumentException(string.Format("Key path[{0}]={1} does not exist",
						startAt, path[startAt]), "path");
				}
				break;
			default:
				throw new InvalidOperationException("Unknown string type: " + Type);
			}
			// test for terminals and call recursively
			var child = (value as JsonString);
			if (child == null) {
				throw new ArgumentException(string.Format("Unexpected terminal {0} on path[{1}]={2}",
					value, startAt, path[startAt]), "path");
			}
			return child.GetJsonString(startAt + 1, path);
		}
		public string GetTerminal(string key, string fallback=null) {
			// test for method applicability
			if (Type != JsonStringType.Dict) {
				throw new InvalidOperationException(string.Format("Cannot string-index {0} JSON strings",
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
					throw new InvalidOperationException(string.Format("Cannot integer-index {0} JSON strings",
						Type));
				} else {
					return fallback;
				}
			}
			// test for out of range
			if (index < 0 || index >= array.Count) {
				if (fallback == null) {
					throw new ArgumentOutOfRangeException("index", string.Format("Index {0} is out of range [0...{1}]",
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
		public bool AddTerminal(string key, string value) {
			if (key == null) throw new ArgumentNullException("key");
			if (value == null) throw new ArgumentNullException("value");
			switch (Type) {
			case JsonStringType.Array:
				array.Add(value);
				return true;
			case JsonStringType.Dict:
				try {
					dict.Add(key, value);
				} catch (ArgumentException) {
					return false;
				}
				return true;
			default:
				return false;
			}
		}
		public bool AddJsonString(string key, JsonString value) {
			if (key == null) throw new ArgumentNullException("key");
			if (value == null) throw new ArgumentNullException("value");
			switch (Type) {
			case JsonStringType.Array:
				array.Add(value);
				return true;
			case JsonStringType.Dict:
				try {
					dict.Add(key, value);
				} catch (ArgumentException) {
					return false;
				}
				return true;
			default:
				return false;
			}
		}

//		public void Add(object value, params string[] path) {
//			if (value == null) throw new ArgumentNullException("value");
//			if (path == null) throw new ArgumentNullException("path");
//			Add(value, 0, path);
//		}
//		private void Add(object value, int startAt, object[] path) {
//			switch (Type) {
//			case JsonStringType.Array:
//				if (path.Length - startAt > 0) {
//					// non empty path indicates recursive call
//					object oIndex = path[startAt];
//					if (!(oIndex is int)) {
//						
//					}
//					int index = (int)oIndex;
//					if (index < 0 || index >= array.Count) {
//						
//					}
//					object oIter = array[index];
//					if (!(oIter is JsonString)) {
//						throw new ArgumentException(string.Format("Terminal {0} on path[{1}]={2}", 
//							oIter, startAt, index), "path");
//					}
//					(oIter as JsonString).Add(value, startAt + 1, path);
//				} else {
//					// empty path indicates add value here
//					array.Add(value);
//					return;
//				}
//				break;
//			case JsonStringType.Dict:
//				if (path.Length - startAt > 0) {
//					// path is not empty
//					object oKey = path[startAt];
//					if (!(oKey is string)) {
//						throw new ArgumentException(string.Format("Not a key in path[{0}]={1}",
//							startAt, path[startAt]), "path");
//					}
//					string key = (string)oKey;
//					if (path.Length - startAt > 1) {
//						// longer path indicates recursive call path
//						object oIter;
//						if (!dict.TryGetValue(key, out oIter)) {
//							throw new ArgumentException(string.Format("Key path[{0}]={1} does not exist",
//								startAt, path[startAt]), "path");
//						}
//
//						if (!(oIter is JsonString)) {
//							throw new ArgumentException(string.Format("Terminal {0} on path[{1}]={2}",
//								oIter, startAt, key), "path");
//						}
//						(oIter as JsonString).Add(value, startAt + 1, path);
//					} else {
//						// short path indicates adding the value here
//						if (dict.ContainsKey(key)) {
//							throw new ArgumentException(string.Format("Key path[{0}]={2} is already in use",
//								startAt, path[startAt]), "path");
//						}
//						dict.Add(key, value);
//					}
//				} else {
//					// no more elements left in path
//					throw new ArgumentException(string.Format("Cannot add path[{0}]={1} without a key",
//						startAt, path[startAt]), "path");
//				}
//				break;
//			default:
//				throw new InvalidOperationException(string.Format("Unsupported JSON type {0} at path[{1}]={2}",
//					Type, startAt, path[startAt]));
//			}
//				
//		}
		public override string ToString () {
			var result = new StringBuilder();
			ToCompressedString(result);
			return result.ToString();
		}
		private void ToCompressedString(StringBuilder result) {
			switch (Type) {
			case JsonStringType.Dict:
				result.Append("{");
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
				result.Append("}");
				break;
			case JsonStringType.Array:
				result.Append("[");
				foreach (object item in array) {
					if (item is JsonString) {
						(item as JsonString).ToCompressedString(result);
					} else if (item is string) {
						// TODO detect type and handle quotes properly
						result.Append((string)item);
					} else {
						// invalid, omit
					}
					result.AppendLine(",");
				}
				result.Remove(result.Length - 1, 1);
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

