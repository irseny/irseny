using System;

namespace Mycena {
	internal class ComboBoxTextFactory : WidgetFactory<Gtk.ComboBoxText> {
		public ComboBoxTextFactory() : base() {
			CreationProperties.Add("active", ComboBoxFactory.SetActive);
			CreationProperties.Add("active_id", ComboBoxFactory.SetActiveId);
			CreationProperties.Add("add_tearoffs", ComboBoxFactory.SetTearoffEnabled);
			CreationProperties.Add("tearoff_title", ComboBoxFactory.SetTearoffTitle);
		}
		protected override Gtk.ComboBoxText CreateWidget(ConfigProperties properties, IInterfaceNode container, IInterfaceStock stock) {			
			var result = new Gtk.ComboBoxText();
			foreach (string item in properties.GetItems()) {
				result.AppendText(item);
			}
			return result;
		}
	}
}

