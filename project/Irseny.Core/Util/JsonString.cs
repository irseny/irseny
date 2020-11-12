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

		public Dictionary<string, object> Dict { 
			get {
				if (Type != JsonStringType.Dict) throw new InvalidOperationException("not a dictionary JSON string");
				return dict;
			}
		}
		public List<object> Array {
			get {
				if (Type != JsonStringType.Array) throw new InvalidOperationException("not an array JSON string");
				return array;
			}
		}
		public void Add(object value, params string[] path) {
			if (value == null) throw new ArgumentNullException("value");
			if (path == null) throw new ArgumentNullException("path");
			Add(value, 0, path);
		}
		private void Add(object value, int startAt, object[] path) {
			switch (Type) {
			case JsonStringType.Array:
				if (path.Length - startAt > 0) {
					// non empty path indicates recursive call
					object oIndex = path[startAt];
					if (!(oIndex is int)) {
						throw new ArgumentException(string.Format("Not an index in path[{0}]={1}",
							startAt, path[startAt]), "path");
					}
					int index = (int)oIndex;
					if (index < 0 || index >= array.Count) {
						throw new ArgumentException(string.Format("Index path[{0}]={1} is out of index range [0...{2}]",
							startAt, index, array.Count - 1), "path");
					}
					object oIter = array[index];
					if (!(oIter is JsonString)) {
						throw new ArgumentException(string.Format("Terminal {0} on path[{1}]={2}", 
							oIter, startAt, index), "path");
					}
					(oIter as JsonString).Add(value, startAt + 1, path);
				} else {
					// empty path indicates add value here
					array.Add(value);
					return;
				}
				break;
			case JsonStringType.Dict:
				if (path.Length - startAt > 0) {
					// path is not empty
					object oKey = path[startAt];
					if (!(oKey is string)) {
						throw new ArgumentException(string.Format("Not a key in path[{0}]={1}",
							startAt, path[startAt]), "path");
					}
					string key = (string)oKey;
					if (path.Length - startAt > 1) {
						// longer path indicates recursive call path
						object oIter;
						if (!dict.TryGetValue(key, out oIter)) {
							throw new ArgumentException(string.Format("Key path[{0}]={1} does not exist",
								startAt, path[startAt]), "path");
						}

						if (!(oIter is JsonString)) {
							throw new ArgumentException(string.Format("Terminal {0} on path[{1}]={2}",
								oIter, startAt, key), "path");
						}
						(oIter as JsonString).Add(value, startAt + 1, path);
					} else {
						// short path indicates adding the value here
						if (dict.ContainsKey(key)) {
							throw new ArgumentException(string.Format("Key path[{0}]={2} is already in use",
								startAt, path[startAt]), "path");
						}
						dict.Add(key, value);
					}
				} else {
					// no more elements left in path
					throw new ArgumentException(string.Format("Cannot add path[{0}]={1} without a key",
						startAt, path[startAt]), "path");
				}
				break;
			default:
				throw new InvalidOperationException(string.Format("Unsupported JSON type {0} at path[{1}]={2}",
					Type, startAt, path[startAt]));
			}
				
		}
		public override string ToString () {
			var result = new StringBuilder();
			ToString(result);
			return result.ToString();
		}
		private void ToString(StringBuilder result) {
			switch (Type) {
			case JsonStringType.Dict:
				result.Append("{");
				foreach (var pair in dict) {
					result.Append('"').Append(pair.Key).Append("\":");
					if (pair.Value is JsonString) {
						(pair.Value as JsonString).ToString(result);
					} else if (pair.Value is string) {
						result.Append('"').Append((string)pair.Value).Append('"');
					} else {
						result.Append(StringifyTools.StringifyPrimitive(pair.Value));
					}
					result.Append(",");
				}
				result.Remove(result.Length - 1, 1);
				result.Append("}");
				break;
			case JsonStringType.Array:
				result.Append("[");
				foreach (var item in array) {
					if (item is JsonString) {
						(item as JsonString).ToString(result);
					} else if (item is string) {
						result.Append('"').Append(item).Append('"');
					} else {
						result.Append(StringifyTools.StringifyPrimitive(item));
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
							result.Append('"').Append((string)pair.Value).Append('"');
						} else {
							result.Append(StringifyTools.StringifyPrimitive(pair.Value));
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
							result.Append('"').Append(item).Append('"');
						} else {
							result.Append(StringifyTools.StringifyPrimitive(item));
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

