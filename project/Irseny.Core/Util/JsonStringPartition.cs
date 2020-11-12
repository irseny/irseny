using System;
using System.Collections.Generic;
using System.Text;

namespace Irseny.Core.Util {
	public partial class JsonString {
		public static List<string> PartitionJson(string source) {
			var partition = new List<string>();
			var bracketMatch = new Stack<char>();
			for (int i = 0; i < source.Length; i++) {
				switch (source[i]) {
				// skip invisible characters
				case ' ':
				case '\t':
				case '\r':
				case '\n':
					break;
					// parse strings
				case '\'':
				case '"':
					partition.Add(ExtractQuoted(source, ref i));
					break;
					// match parantheses
				case '(':
					partition.Add("(");
					bracketMatch.Push(')');
					break;
				case '[':
					partition.Add("[");
					bracketMatch.Push(']');
					break;
				case '{':
					partition.Add("{");
					bracketMatch.Push('}');
					break;
				case ')':
				case ']':
				case '}':					
					if (bracketMatch.Count > 0 && bracketMatch.Peek() == source[i]) {
						bracketMatch.Pop();
						partition.Add(source[i].ToString());
					} else {
						if (i + 1 < source.Length) {
							throw new ArgumentException(string.Format("Unexpected symbol {0} after\n{1}...\nfollowed by\n...{2}", 
								source[i], source.Substring(0, i), source.Substring(i + 1)));
						} else {
							throw new ArgumentException(string.Format("Unexpected symbol {0} after\n{1}",
								source[i], source.Substring(0, i)));
						}
					}
					break;
					// handle separation characters
				case ',':
				case ':':
					partition.Add(source[i].ToString());
					break;
					// handle everything else as non quoted strings	
				default:
					partition.Add(ExtractUnquoted(source, ref i));
					break;

				}
			}
			if (bracketMatch.Count > 0) {
				throw new ArgumentException(string.Format("Unexpected EOF: {0} open after\n{1}",
					new string(bracketMatch.ToArray()), source));
			}
			return partition;
		}
		private static string ExtractQuoted(string source, ref int startAt) {
			char searchFor = source[startAt];
			if (searchFor != '\'' && searchFor != '"') throw new InvalidOperationException();
			for (int i = startAt + 1; i < source.Length; i++) {
				if (source[i] == searchFor) {
					// include the last character read
					string result = source.Substring(startAt, i - startAt + 1);
					startAt = i;
					return result;
				}
			}
			if (startAt + 1 < source.Length) {
				throw new ArgumentException(string.Format("No matching end quote for {0} after\n{1}...\nin\n...{2}",
					source[startAt], source.Substring(0, startAt), source.Substring(startAt + 1)));
			} else {
				throw new ArgumentException(string.Format("No matching end quote for {0} after\n{1}",
					source[startAt], source.Substring(0, startAt)));
			}
		}
		private static string ExtractUnquoted(string source, ref int startAt) {
			string result;
			// only numbers allowed
			for (int i = startAt; i < source.Length; i++) {
				switch (source[i]) {
				// accept numbers
				case '+':
				case '-':
				case '0':
				case '1':
				case '2':
				case '3':
				case '4':
				case '5':
				case '6':
				case '7':
				case '8':
				case '9':
				case '.':
					break;
				// accept other constants
				case 'f': // accepts false
				case 'a':
				case 'l':
				case 's':
				case 'e':
				case 't': // enables true accpetance
				case 'r':
				case 'u':
				case 'x': // numbers in hex format
				case 'n': // enables null acceptance
					break;
				// final characters
				case '\t':
				case '\r':
				case '\n':
				case ',':
				case ':':
				case ' ':
				case ')':
				case '(':
				case '[':
				case ']':
				case '{':
				case '}':
					// exclude the last character read
					result = source.Substring(startAt, i - startAt);
					if (result.Length == 0) {
						throw new ArgumentException(string.Format("Empty field after\n{0}...\nfollowed by\n...{1}",
							source.Substring(0, startAt), source.Substring(startAt)));
					}
					startAt = i - 1;
					return result;
					// not accepted characters
				default:
					if (i + 1 < source.Length) {
						throw new ArgumentException(string.Format("Invalid symbol {0} after\n{1}...\nfollowed by\n...{2}",
							source[i], source.Substring(0, i), source.Substring(i + 1)));
					} else {
						throw new ArgumentException(string.Format("Invalid symbol {0} after\n{1}",
							source[i], source.Substring(0, i)));
					}
				}
			}
			// interpret string end as final character
			// parsing still fails later as matching braces are missing
			result = source.Substring(startAt);
			startAt = source.Length;
			return result;
		}

	}
	public static class JsonStringPartition {
		public static string ToJsonString(this List<string> partition) {
			return ToJsonString(partition, 0, partition.Count);
		}
		public static string ToJsonString(this List<string> partition, int startAt) {
			return ToJsonString(partition, startAt, partition.Count - startAt);
		}
		public static string ToJsonString(this List<string> partition, int startAt, int length) {
			if (partition == null) throw new ArgumentNullException("this");
			if (startAt < 0) throw new ArgumentOutOfRangeException("startAt cannot be less than 0");
			if (length < 0) throw new ArgumentOutOfRangeException("length cannot be less than 0");
			if (startAt + length > partition.Count) {
				throw new ArgumentOutOfRangeException("startAt and length must refer to a region within the partition");
			}
			var result = new StringBuilder();
			int indent = 0;
			for (int i = 0; i < startAt + length; i++) {				
				if (partition[i] == null || partition[i].Length == 0) {
					continue;
				}
				if (i == startAt) {
					// start with a properly indented first line
					result.Append('\t', indent);
				}
				if (i >= startAt) {
					// generate output with the correct indentation
					switch (partition[i][0]) {
					case '{':
					case '[':
					case '(':
						result.AppendLine(partition[i]);
						indent += 1;
						result.Append('\t', indent);
						break;
					case '}':
					case ']':
					case ')':
						result.AppendLine();
						indent -= 1;
						result.Append('\t', indent);
						result.Append(partition[i]);
						break;
					case ':':
						result.Append(": ");
						break;
					case ',':
						result.AppendLine(",");
						result.Append('\t', indent);
						break;
					default:
						result.Append(partition[i]);
						break;
					}
				} else {
					// do not generate output but ensure the correct indentation
					switch (partition[i][0]) {
					case '{':
					case '[':
					case '(':
						indent += 1;
						break;
					case '}':
					case ']':
					case ')':
						indent -= 1;
						break;
					}
				}
			}
			return result.ToString();
		}
	}
}

