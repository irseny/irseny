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
			CreationProperties.Add("relief", SetRelief);
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
		public static bool SetRelief<T>(T widget, ConfigProperties properties, IInterfaceNode container, IInterfaceStock stock) where T : Gtk.Button {
			Gtk.ReliefStyle style;
			try {
				style = TextParseTools.ParseReliefStyle(properties.GetProperty("relief", Gtk.ReliefStyle.Normal));
			} catch (FormatException) {
				return false;
			}
			widget.Relief = style;
			return true;
		}
	}
}
