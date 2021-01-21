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

namespace Irseny.Core.Util {
	public partial class JsonString {
		public static JsonString InterpretJson(List<string> source) {
			if (source == null) throw new ArgumentNullException("source");
			if (source.Count == 0) {
				return new JsonString(JsonStringType.Dict);
			}
			JsonString result;
			int startAt = 0;
			switch (source[0]) {
			case "{":				
				result = InterpretJsonDict(source, ref startAt);
				break;
			case "[":				
				result = InterpretJsonArray(source, ref startAt);
				break;
			default:
				throw new ArgumentException(string.Format("Unexpected symbol {0} at the start of\n{1}",
					source[0], source.ToJsonString()));
			}
			return result;
		}
		private static JsonString InterpretJsonDict(List<string> source, ref int startAt) {
			var result = new JsonString(JsonStringType.Dict);
			// check for {
			if (startAt >= source.Count) {
				throw new InvalidOperationException();
			}
			if (!source[startAt].Equals("{")) {
				throw new InvalidOperationException();
			}
			// iterate till the end
			bool foundClosing = false;

			for (int i = startAt + 1; i < source.Count; i++) {
				
				if (source[i].StartsWith("'") || source[i].StartsWith("\"")) {
					
					KeyValuePair<string, object> pair = InterpretJsonKeyValue(source, ref i);
					result.dict.Add(pair.Key, pair.Value);
					// jump to next unread part
					i += 1;
				} else {
					throw new ArgumentException(string.Format("Expected ' or \" after\n{0}...\nfollowed by\n...{1}",
						source.ToJsonString(0, i), source.ToJsonString(i + 1)));
				}
				// after pair read continuation or ending character
				if (i >= source.Count) {
					throw new ArgumentException(string.Format("Expected , or } at the end of\n{0}",
						source.ToJsonString()));
				}
				string end = source[i];
				if (end.Equals(",")) {
					// nothing to do
				} else if (source[i].Equals("}")) {
					foundClosing = true;
					startAt = i;
					break;
				} else {
					throw new ArgumentException(string.Format("Expected , after\n{0}\ninstead followed by\n{1}",
						source.ToJsonString(0, i), source.ToJsonString(i + 1)));
				}
			}
			if (!foundClosing) {
				throw new ArgumentException(string.Format("Missing closing } after\n{0}\nin\n{1}",
					source.ToJsonString(0, startAt + 1), source.ToJsonString(startAt)));
			}

			return result;
		}
		private static KeyValuePair<string, object> InterpretJsonKeyValue(List<string> source, ref int startAt) {
			// pair consists of (startAt)"key", ":", "value"
			// where value can be a number, a string, a dictionary or an array
			// TODO implement advanced cases
			if (startAt + 2 >= source.Count) {
				throw new ArgumentException(string.Format("Expected a key-value-pair at end of\n{0}",
					source.ToJsonString()));
			}
			string key = source[startAt].TrimStart('"', '\'').TrimEnd('"', '\'');
			startAt += 1;
			if (!source[startAt].Equals(":")) {
				throw new ArgumentException(string.Format("Expected : after\n{0}...\ninstead followed by\n...{1}",
					source.ToJsonString(0, startAt + 1), source.ToJsonString(startAt + 2)));
			}
			startAt += 1;
			object value = source[startAt];
			if (value.Equals("[")) {
				value = InterpretJsonArray(source, ref startAt);
			} else if (value.Equals("{")) {
				value = InterpretJsonDict(source, ref startAt);
			} else {
				// TODO parse as string, int or float
				value = source[startAt];
//				string text = source[startAt];
//				bool bValue;
//				int iValue;
//				double dValue;
//				if (TextParseTools.TryParseNull(text, out value)) {
//					// nothing to do
//				} else if (TextParseTools.TryParseInt(text, out iValue)) {
//					value = iValue;
//				} else if (TextParseTools.TryParseBool(text, out bValue)) {
//					value = bValue;
//				} else if (TextParseTools.TryParseDouble(text, out dValue)) {
//					value = (float)dValue;
//				} else {
//					value = text;
//				}
			}
			return new KeyValuePair<string, object>(key, value);	

		}
		private static JsonString InterpretJsonArray(List<string> source, ref int startAt) {
			var result =  new JsonString(JsonStringType.Array);
			// check for previous errors
			if (startAt >= source.Count) {
				throw new InvalidOperationException();
			}
			if (!source[startAt].Equals("[")) {
				throw new InvalidOperationException();
			}
			bool foundClosing = false;
			for (int i = startAt + 1; i < source.Count; i++) {
				// read next item
				string value = source[i];
				if (value.Equals("[")) {
					result.array.Add(InterpretJsonArray(source, ref i));
				} else if (value.Equals("{")) {
					result.array.Add(InterpretJsonDict(source, ref i));
				} else {
					// TODO parse string, int or float
					result.array.Add(value);
				}
				// read end or continuation character
				i += 1;
				if (i >= source.Count) {
					throw new ArgumentException(string.Format("Expected , or ] after\n{0}",
						source.ToJsonString()));
				}
				string end = source[i];
				if (end.Equals(",")) {
					// nothing to do
				} else if (end.Equals("]")) {
					foundClosing = true;
					startAt = i;
					break;
				} else {
					throw new ArgumentException(string.Format("Expected , or ] after\n{1}\ninstead of\n{2}",
						source.ToJsonString(0, i), source.ToJsonString(i)));
				}

			}
			if (!foundClosing) {
				throw new ArgumentException(string.Format("Missing closing ] after\n{0}\nin\n{1}",
					source.ToJsonString(0, startAt), source.ToJsonString(startAt + 1)));
			}
			return result;
		}

	}
}

