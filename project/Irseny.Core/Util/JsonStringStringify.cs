using System;
using System.Globalization;
using System.Text;

namespace Irseny.Core.Util {
	public partial class JsonString {
		public readonly static NumberStyles NumberStyle = NumberStyles.Float;
		public readonly static CultureInfo FormatProvider = CultureInfo.InvariantCulture;

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


		public static string StringifyString(string primitive) {
			// TODO check for quotes first
			return string.Format("\"{0}\"", primitive);
		}
		public static string Stringify(int primitive) {
			return primitive.ToString(FormatProvider);
		}
		public static string Stringify(uint primitive) {
			return primitive.ToString(FormatProvider);
		}
		public static string Stringify(bool primitive) {
			if (primitive) {
				return "true";
			} else {
				return "false";
			}
		}
		public static string Stringify(float primitive) {
			return primitive.ToString(FormatProvider);
		}
		public static string Stringify(double primitive) {
			return primitive.ToString(FormatProvider);
		}
		public static string Stringify(decimal primitive) {
			return primitive.ToString(FormatProvider);
		}

	}
}

