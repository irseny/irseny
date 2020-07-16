using System;
using System.Collections.Generic;

namespace Irseny.Core.Util {
	public static class StringPatternSearch {

		public static string[] Split(this string self, string pattern, StringSplitOptions options = StringSplitOptions.None) {
			return Split(self, pattern, int.MaxValue, options);
		}
		public static string[] Split(this string self, string pattern, int count, StringSplitOptions options = StringSplitOptions.None) {
			if (self == null) throw new ArgumentNullException("this");
			if (pattern == null) throw new ArgumentNullException("pattern");
			if (count < 1) throw new ArgumentOutOfRangeException("count");
			bool removeEmpty = ((options & StringSplitOptions.RemoveEmptyEntries) != 0);
			// special case: result of max length 1
			//return 
			// special case: string is empty
			if (self.Length == 0) {
				if (removeEmpty) {
					return new string[0];
				} else {
					return new string[1] { self };
				}
			}
			// special case: pattern is empty
			if (pattern.Length == 0) {
				// split by character
				int splitNo = System.Math.Min(self.Length - 1, count - 1);
				string[] result = new string[splitNo + 1];
				for (int c = 0; c < splitNo; c++) {
					result[c] = self[c].ToString();
				}
				result[splitNo] = self.Substring(splitNo);
				return result;
			}
			return Split(self, pattern, count - 1, 0, removeEmpty);



		}
		private static string[] Split(this string self, string pattern, int maxSplitNo, int splitNo, bool removeEmpty) {
			// special case: this is empty
			if (self.Length == 0) {
				if (removeEmpty) {
					return new string[splitNo];
				} else {
					var result = new string[splitNo + 1];
					result[splitNo] = self;
					return result;
				}
			}
			// special case: no more splits to make
			if (splitNo >= maxSplitNo) {
				string[] result = new string[splitNo + 1];
				result[splitNo] = self;
				return result;
			}
			// search for pattern
			int appearance = self.IndexOf(pattern, StringComparison.InvariantCulture);
			if (appearance < 0) {
				// pattern not found
				string[] result = new string[splitNo + 1];
				result[splitNo] = self;
				return result;
			} else {
				// split string and call recursively
				string remaining = self.Substring(appearance + pattern.Length);
				if (appearance == 0 && removeEmpty) {
					// special case: string is not added
					return remaining.Split(pattern, maxSplitNo, splitNo, removeEmpty);
				} else {
					// standard case
					string[] result = remaining.Split(pattern, maxSplitNo, splitNo + 1, removeEmpty);
					result[splitNo] = self.Substring(0, appearance);
					return result;
				}
			}
		}
	}
}
