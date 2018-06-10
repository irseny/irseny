using System;

namespace Mycena {
	internal class ToggleButtonFactory : WidgetFactory<Gtk.ToggleButton> {
		public ToggleButtonFactory() : base() {
			// button properties
			CreationProperties.Add("use_stock", ButtonFactory.SetUseStock);
			CreationProperties.Add("label", ButtonFactory.SetLabel);
			CreationProperties.Add("image", ButtonFactory.SetImage);
			CreationProperties.Add("image_position", ButtonFactory.SetImagePosition);
			CreationProperties.Add("use_underline", ButtonFactory.SetUnderline);
			// togglue button properties
			CreationProperties.Add("active", ToggleButtonFactory.SetActive);
			CreationProperties.Add("draw_indicator", ToggleButtonFactory.SetDrawIndicator);
		}
		protected override Gtk.ToggleButton CreateWidget(ConfigProperties properties, IInterfaceNode container) {
			return new Gtk.ToggleButton();
		}

		public static bool SetActive<T>(T widget, ConfigProperties properties, IInterfaceNode container) where T : Gtk.ToggleButton {
			bool active;
			try {
				active = TextParseTools.ParseBool(properties.GetProperty("active"));
			} catch (FormatException) {
				return false;
			}
			widget.Active = active;
			return true;
		}
		public static bool SetDrawIndicator<T>(T widget, ConfigProperties properties, IInterfaceNode container) where T : Gtk.ToggleButton {
			bool drawIndicator;
			try {
				drawIndicator = TextParseTools.ParseBool(properties.GetProperty("draw_indicator"));

			} catch (FormatException) {
				return false;
			}
			widget.DrawIndicator = drawIndicator;
			return true;
		}


	}
}

