using System;

namespace Mycena {
	internal class MiscFactory {
		public static bool SetAligment<T>(T widget, ConfigProperties properties, IInterfaceNode container) where T : Gtk.Misc {
			float alignX, alignY;
			widget.GetAlignment(out alignX, out alignY);
			try {
				string sAlignX, sAlignY;
				if (properties.TryGetProperty("xalign", out sAlignX)) {
					alignX = TextParseTools.ParseFloat(sAlignX);
				}
				if (properties.TryGetProperty("yalign", out sAlignY)) {
					alignY = TextParseTools.ParseFloat(sAlignY);
				}
			} catch (FormatException) {
				return false;
			}
			widget.SetAlignment(alignX, alignY);
			return true;
		}
		public static bool SetPadding<T>(T widget, ConfigProperties properties, IInterfaceNode container) where T : Gtk.Misc {
			int padX, padY;
			widget.GetPadding(out padX, out padY);
			try {
				string sPadX, sPadY;
				if (properties.TryGetProperty("xpadding", out sPadX)) {
					padX = TextParseTools.ParseInt(sPadX);
				}
				if (properties.TryGetProperty("ypadding", out sPadY)) {
					padY = TextParseTools.ParseInt(sPadY);
				}
			} catch (FormatException) {
				return false;
			}
			widget.SetPadding(padX, padY);
			return true;
		}
	}
}