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
namespace Irseny.Core.Util {
	public static class BytePatternSearch {
		public static int IndexOfPattern(this byte[] self, byte[] pattern, int startAt=0, int length=-1) { 
			if (self == null) throw new ArgumentNullException("this");
			if (pattern == null) throw new ArgumentNullException("pattern");
			if (startAt < 0) throw new ArgumentOutOfRangeException("startAt");
			if (length < 0) {
				// automatic evaluation
				// set remaining array length 
				length = self.Length - startAt;
			}
			if (startAt + length > self.Length) throw new ArgumentOutOfRangeException("length");
			if (pattern.Length == 0) {
				// trivial empty pattern case
				return startAt;
			}
			if (length == 0) {
				// no pattern in an empty range
				return -1;
			}
			// search through whole range
			int end = startAt + length;
			for (int i = startAt; i < end; i++) {
				if (self[i] == pattern[0]) {
					// search through rest of
					int p = 1;
					for (; p < pattern.Length && i + p < end; p++) {
						if (self[i + p] != pattern[p]) {
							break;
						}
					}
					if (p >= pattern.Length) {
						// found all pattern characters
						return i;
					}
				}
			}
			return -1;
		}
	}
}
