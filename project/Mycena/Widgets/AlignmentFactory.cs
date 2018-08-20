using System;
namespace Mycena {
	internal class AlignmentFactory : WidgetFactory<Gtk.Alignment> {
		public AlignmentFactory() : base() {
			CreationProperties.Add("top_padding", SetPadding);
			CreationProperties.Add("bottom_padding", SetPadding);
			CreationProperties.Add("left_padding", SetPadding);
			CreationProperties.Add("right_padding", SetPadding);
		}
		protected override Gtk.Alignment CreateWidget(ConfigProperties properties, IInterfaceNode container, IInterfaceStock stock) {
			float alignX, alignY, scaleX, scaleY;
			try {
				alignX = TextParseTools.ParseFloat(properties.GetProperty("xalign", 0.5));
				alignY = TextParseTools.ParseFloat(properties.GetProperty("yalign", 0.5));
				scaleX = TextParseTools.ParseFloat(properties.GetProperty("xscale", 1.0));
				scaleY = TextParseTools.ParseFloat(properties.GetProperty("yscale", 1.0));
			} catch (FormatException) {
				return null;
			}
			return new Gtk.Alignment(alignX, alignY, scaleX, scaleY);
		}
		private static bool SetPadding(Gtk.Alignment widget, ConfigProperties properties, IInterfaceNode container, IInterfaceStock stock) {
			uint top, bottom, left, right;
			try {
				top = TextParseTools.ParseUInt(properties.GetProperty("top_padding", 0));
				bottom = TextParseTools.ParseUInt(properties.GetProperty("bottom_padding", 0));
				left = TextParseTools.ParseUInt(properties.GetProperty("left_padding", 0));
				right = TextParseTools.ParseUInt(properties.GetProperty("right_padding", 0));
			} catch (FormatException) {
				return false;
			}
			widget.SetPadding(top, bottom, left, right);
			return true;
		}
	}
}
