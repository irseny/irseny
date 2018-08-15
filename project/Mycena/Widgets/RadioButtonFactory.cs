using System;
namespace Mycena {
	internal class RadioButtonFactory : WidgetFactory<Gtk.RadioButton> {
		public RadioButtonFactory() : base() {
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
		protected override Gtk.RadioButton CreateWidget(ConfigProperties properties, IInterfaceNode container, IInterfaceStock stock) {
			Gtk.RadioButton group = null;
			string label;
			try {
				label = properties.GetProperty("label", null);
				string groupName = properties.GetProperty("group", null);
				if (groupName != null) {
					group = container.GetGadget<Gtk.RadioButton>(groupName);
				}
			} catch (FormatException) {
				return null;
			}
			if (group != null) {
				Console.WriteLine("group: " + group.Label);
				return new Gtk.RadioButton(group, label);
			} else {
				Console.WriteLine("no group");
				return new Gtk.RadioButton(label);
			}

		}

	}
}
