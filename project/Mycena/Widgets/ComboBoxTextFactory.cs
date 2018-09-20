using System;
using System.Diagnostics;
using System.Collections.Generic;

namespace Mycena {
	internal class ComboBoxTextFactory : WidgetFactory<Gtk.ComboBoxText> {
		public ComboBoxTextFactory() : base() {
			CreationProperties.Add("active", ComboBoxFactory.SetActive);
			CreationProperties.Add("active_id", ComboBoxFactory.SetActiveId);
			CreationProperties.Add("add_tearoffs", ComboBoxFactory.SetTearoffEnabled);
			CreationProperties.Add("tearoff_title", ComboBoxFactory.SetTearoffTitle);
			CreationProperties.Add("has_entry", CommonWidgetMods.Noop);
		}
		protected override Gtk.ComboBoxText CreateWidget(ConfigProperties properties, IInterfaceNode container, IInterfaceStock stock) {
			bool withEntry;
			try {
				withEntry = TextParseTools.ParseBool(properties.GetProperty("has_entry", false));
			} catch (FormatException) {
				return null;
			}
			Gtk.ComboBoxText result;
			if (withEntry) {
				result = Gtk.ComboBoxText.NewWithEntry();
			} else {
				result = new Gtk.ComboBoxText();
			}
			foreach (string item in properties.GetItems()) {
				result.AppendText(item);
			}
			return result;
		}
		protected override bool PackWidgets(Gtk.ComboBoxText container, IList<Tuple<Gtk.Widget, ConfigProperties>> children, IInterfaceStock stock) {
			foreach (var pair in children) {
				string childInternal = pair.Item2.GetAttribute("child_internal-child", string.Empty);
				if (childInternal.Equals("entry")) {
					// TODO: migrate entry properties
					//container.Child = pair.Item1;
					//container.Add(pair.Item1);
					/*if (pair.Item1 is Gtk.Entry) {
						container.Child = (Gtk.Entry)pair.Item1;
					} else {
						Debug.WriteLine("Mycena-Warning: Attempting to add non entry widget to ComboBoxText");
					}*/
				} else {
					throw new NotSupportedException(GetType().Name + " can not pack generic widgets");
				}
			}
			return true;
		}
	}
}

