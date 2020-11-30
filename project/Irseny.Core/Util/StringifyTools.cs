using System;
using System.Globalization;

namespace Irseny.Core.Util
{
	public static class StringifyTools {
		public readonly static NumberStyles NumberStyle = NumberStyles.Float;
		public readonly static CultureInfo FormatProvider = CultureInfo.InvariantCulture;

		public static string StringifyPrimitive(object primitive) {
			if (primitive == null) {
				return "null";
			} else if (primitive is int) {
				return StringifyInt((int)primitive);
			} else if (primitive is uint) {
				return StringifyUInt((uint)primitive);
			} else if (primitive is bool) {
				return StringifyBool((bool)primitive);
			} else if (primitive is float) {
				return StringifyFloat((float)primitive);
			} else if (primitive is double) {
				return StringifyDouble((double)primitive);
			} else if (primitive is string) {
				return StringifyString((string)primitive);
			} else {
				throw new ArgumentException("primitive");
			}
		}
		public static string StringifyString(string primitive) {
			// TODO check for quotes first
			return string.Format("\"{0}\"", primitive);
		}
		public static string StringifyInt(int primitive) {
			return primitive.ToString(FormatProvider);
		}
		public static string StringifyUInt(uint primitive) {
			return primitive.ToString(FormatProvider);
		}
		public static string StringifyBool(bool primitive) {
			if (primitive) {
				return "true";
			} else {
				return "false";
			}
		}
		public static string StringifyFloat(float primitive) {
			return primitive.ToString(FormatProvider);
		}
		public static string StringifyDouble(double primitive) {
			return primitive.ToString(FormatProvider);
		}
	}
}

