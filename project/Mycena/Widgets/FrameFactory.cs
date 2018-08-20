using System;
namespace Mycena {
	internal class FrameFactory : WidgetFactory<Gtk.Frame> {
		public FrameFactory() : base() {
			CreationProperties.Add("label_xalign", SetLabelAlignmentX);
			CreationProperties.Add("label_yalign", SetLabelAlignmentY);
			CreationProperties.Add("shadow_type", SetShadowType);
		}
		protected override Gtk.Frame CreateWidget(ConfigProperties properties, IInterfaceNode container, IInterfaceStock stock) {
			return new Gtk.Frame();
		}
		private static bool SetLabelAlignmentX(Gtk.Frame widget, ConfigProperties properties, IInterfaceNode container, IInterfaceStock stock) {
			float align;
			try {
				align = TextParseTools.ParseFloat(properties.GetProperty("label_xalign"));
			} catch (FormatException) {
				return false;
			}
			widget.LabelXalign = align;
			return true;
		}
		private static bool SetLabelAlignmentY(Gtk.Frame widget, ConfigProperties properties, IInterfaceNode container, IInterfaceStock stock) {
			float align;
			try {
				align = TextParseTools.ParseFloat(properties.GetProperty("label_yalign"));
			} catch (FormatException) {
				return false;
			}
			widget.LabelYalign = align;
			return true;
		}
		private static bool SetShadowType(Gtk.Frame widget, ConfigProperties properties, IInterfaceNode container, IInterfaceStock stock) {
			Gtk.ShadowType shadow;
			try {
				shadow = TextParseTools.ParseShadowType(properties.GetProperty("shadow_type"));
			} catch (FormatException) {
				return false;
			}
			widget.ShadowType = shadow;
			return true;
		}
	}
}
