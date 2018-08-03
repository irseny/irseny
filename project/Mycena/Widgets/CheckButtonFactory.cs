using System;

namespace Mycena {
	internal class CheckButtonFactory : WidgetFactory<Gtk.CheckButton> {
		public CheckButtonFactory() : base() {
			// button
			CreationProperties.Add("xalign", ButtonFactory.SetAligment);
			CreationProperties.Add("yalign", ButtonFactory.SetAligment);
			CreationProperties.Add("margin", ButtonFactory.SetMargin);
			CreationProperties.Add("margin_left", ButtonFactory.SetLeftMargin);
			CreationProperties.Add("margin_right", ButtonFactory.SetRightMargin);
			CreationProperties.Add("margin_top", ButtonFactory.SetTopMargin);
			CreationProperties.Add("margin_bottom", ButtonFactory.SetBottomMargin);
			CreationProperties.Add("use_stock", ButtonFactory.SetUseStock);
			CreationProperties.Add("label", ButtonFactory.SetLabel);
			CreationProperties.Add("image", ButtonFactory.SetImage);
			CreationProperties.Add("image_position", ButtonFactory.SetImagePosition);
			CreationProperties.Add("use_underline", ButtonFactory.SetUnderline);
			// togglue button properties
			CreationProperties.Add("active", ToggleButtonFactory.SetActive);
			CreationProperties.Add("draw_indicator", ToggleButtonFactory.SetDrawIndicator);
		}
		protected override Gtk.CheckButton CreateWidget(ConfigProperties properties, IInterfaceNode container, IInterfaceStock stock) {
			return new Gtk.CheckButton();
		}
	}
}

