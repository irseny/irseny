using System;

namespace Mycena {
	internal class ButtonFactory : WidgetFactory<Gtk.Button> {
		public ButtonFactory() : base() {
			CreationProperties.Add("use_stock", SetUseStock);
			CreationProperties.Add("label", SetLabel);
			CreationProperties.Add("image", SetImage);
			CreationProperties.Add("image_position", SetImagePosition);
			CreationProperties.Add("use_underline", SetUnderline);
			CreationProperties.Add("xalign", SetAligment);
			CreationProperties.Add("yalign", SetAligment);
			CreationProperties.Add("margin", SetMargin);
			CreationProperties.Add("margin_left", SetLeftMargin);
			CreationProperties.Add("margin_right", SetRightMargin);
			CreationProperties.Add("margin_top", SetTopMargin);
			CreationProperties.Add("margin_bottom", SetBottomMargin);
		}
		protected override Gtk.Button CreateWidget(ConfigProperties properties, IInterfaceNode container, IInterfaceStock stock) {
			return new Gtk.Button();
		}

		public static bool SetUseStock<T>(T widget, ConfigProperties properties, IInterfaceNode container, IInterfaceStock stock) where T : Gtk.Button {
			bool useStock;
			try {
				useStock = TextParseTools.ParseBool(properties.GetProperty("use_stock"));
			} catch (FormatException) {
				return false;
			}
			widget.UseStock = useStock;
			return true;
		}
		public static bool SetLabel<T>(T widget, ConfigProperties properties, IInterfaceNode container, IInterfaceStock stock) where T : Gtk.Button {
			string label = properties.GetProperty("label");
			widget.Label = label;
			return true;
		}
		public static bool SetImage<T>(T widget, ConfigProperties properties, IInterfaceNode container, IInterfaceStock stock) where T : Gtk.Button {
			Gtk.Image image = container.GetGadget<Gtk.Image>(properties.GetProperty("image"), null);
			if (image == null) {
				return false;
			} else {
				widget.Image = image;
				return true;
			}
		}
		public static bool SetImagePosition<T>(T widget, ConfigProperties properties, IInterfaceNode container, IInterfaceStock stock) where T : Gtk.Button {
			Gtk.PositionType position;
			try {
				position = TextParseTools.ParsePositionType(properties.GetProperty("image_position"));
			} catch (FormatException) {
				return false;
			}
			widget.ImagePosition = position;
			return true;
		}
		public static bool SetUnderline<T>(T widget, ConfigProperties properties, IInterfaceNode container, IInterfaceStock stock) where T : Gtk.Button {
			bool underline;
			try {
				underline = TextParseTools.ParseBool(properties.GetProperty("use_underline"));
			} catch (FormatException) {
				return false;
			}
			widget.UseUnderline = underline;
			return true;
		}
		public static bool SetAligment<T>(T widget, ConfigProperties properties, IInterfaceNode container, IInterfaceStock stock) where T : Gtk.Button {
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
		public static bool SetMargin<T>(T widget, ConfigProperties properties, IInterfaceNode container, IInterfaceStock stock) where T : Gtk.Button {
			int margin;
			try {
				margin = TextParseTools.ParseInt(properties.GetProperty("margin", 0));
			} catch (FormatException) {
				return false;
			}
			widget.Margin = margin;
			return true;
		}
		public static bool SetLeftMargin<T>(T widget, ConfigProperties properties, IInterfaceNode container, IInterfaceStock stock) where T : Gtk.Button {
			int margin;
			try {
				margin = TextParseTools.ParseInt(properties.GetProperty("margin_left", 0));
			} catch (FormatException) {
				return false;
			}
			widget.MarginLeft = margin;
			return true;
		}
		public static bool SetRightMargin<T>(T widget, ConfigProperties properties, IInterfaceNode container, IInterfaceStock stock) where T : Gtk.Button {
			int margin;
			try {
				margin = TextParseTools.ParseInt(properties.GetProperty("margin_right", 0));
			} catch (FormatException) {
				return false;
			}
			widget.MarginRight = margin;
			return true;
		}
		public static bool SetTopMargin<T>(T widget, ConfigProperties properties, IInterfaceNode container, IInterfaceStock stock) where T : Gtk.Button {
			int margin;
			try {
				margin = TextParseTools.ParseInt(properties.GetProperty("margin_top", 0));
			} catch (FormatException) {
				return false;
			}
			widget.MarginTop = margin;
			return true;
		}
		public static bool SetBottomMargin<T>(T widget, ConfigProperties properties, IInterfaceNode container, IInterfaceStock stock) where T : Gtk.Button {
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
