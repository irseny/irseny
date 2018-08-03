using System;

namespace Mycena {
	internal class MiscFactory {
		public static bool SetAligment<T>(T widget, ConfigProperties properties, IInterfaceNode container, IInterfaceStock stock) where T : Gtk.Misc {
			
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
		public static bool SetPadding<T>(T widget, ConfigProperties properties, IInterfaceNode container, IInterfaceStock stock) where T : Gtk.Misc {
			int padX, padY;
			widget.GetPadding(out padX, out padY);
			try {
				string sPadX, sPadY;
				if (properties.TryGetProperty("xpad", out sPadX)) {
					padX = TextParseTools.ParseInt(sPadX);
				}
				if (properties.TryGetProperty("ypad", out sPadY)) {
					padY = TextParseTools.ParseInt(sPadY);
				}
			} catch (FormatException) {
				return false;
			}
			widget.SetPadding(padX, padY);
			return true;
		}
		public static bool SetMargin<T>(T widget, ConfigProperties properties, IInterfaceNode container, IInterfaceStock stock) where T : Gtk.Misc {
			int margin;
			try {
				margin = TextParseTools.ParseInt(properties.GetProperty("margin", 0));
			} catch (FormatException) {
				return false;
			}
			widget.Margin = margin;
			return true;
		}
		public static bool SetLeftMargin<T>(T widget, ConfigProperties properties, IInterfaceNode container, IInterfaceStock stock) where T : Gtk.Misc {
			int margin;
			try {
				margin = TextParseTools.ParseInt(properties.GetProperty("margin_left", 0));
			} catch (FormatException) {
				return false;
			}
			widget.MarginLeft = margin;
			return true;
		}
		public static bool SetRightMargin<T>(T widget, ConfigProperties properties, IInterfaceNode container, IInterfaceStock stock) where T : Gtk.Misc {
			int margin;
			try {
				margin = TextParseTools.ParseInt(properties.GetProperty("margin_right", 0));
			} catch (FormatException) {
				return false;
			}
			widget.MarginRight = margin;
			return true;
		}
		public static bool SetTopMargin<T>(T widget, ConfigProperties properties, IInterfaceNode container, IInterfaceStock stock) where T : Gtk.Misc {
			int margin;
			try {
				margin = TextParseTools.ParseInt(properties.GetProperty("margin_top", 0));
			} catch (FormatException) {
				return false;
			}
			widget.MarginTop = margin;
			return true;
		}
		public static bool SetBottomMargin<T>(T widget, ConfigProperties properties, IInterfaceNode container, IInterfaceStock stock) where T : Gtk.Misc {
			int margin;
			try {
				margin = TextParseTools.ParseInt(properties.GetProperty("margin_bottom", 0));
			} catch (FormatException) {
				return false;
			}
			widget.MarginBottom = margin;
			return true;
		}
	}
}