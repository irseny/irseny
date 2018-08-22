using System;
using System.Collections.Generic;

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
		protected override bool PackWidgets(Gtk.ComboBoxText container, IList<Tuple<Gtk.Widget, ConfigProperties>> children, IInterfaceStock stock) {
			foreach (var pair in children) {
				string childInternal = pair.Item2.GetAttribute("child_internal-child", string.Empty);
				if (childInternal.Equals("entry")) {
					container.Child = pair.Item1;
				} else {
					throw new NotSupportedException(GetType().Name + " can not pack generic widgets");
				}
			}
			return true;
		}
	}
}

