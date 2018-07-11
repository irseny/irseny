using System;
using System.Xml;
using System.Collections.Generic;

namespace Mycena {
	internal class TextViewFactory : WidgetFactory<Gtk.TextView> {
		public TextViewFactory() : base() {
			CreationProperties.Add("editable", SetEditable);
			CreationProperties.Add("wrap_mode", SetWrapMode);
			CreationProperties.Add("accepts_tab", SetTabAcceptance);
			CreationProperties.Add("buffer", SetBuffer);
		}

		protected override Gtk.TextView CreateWidget(ConfigProperties properties, IInterfaceNode container) {


			Gtk.TextBuffer buffer = TextBufferFactory.GetBuffer(properties, container);
			return new Gtk.TextView(buffer);
		}
		private static bool SetEditable(Gtk.TextView widget, ConfigProperties properties, IInterfaceNode container) {
			bool editable;
			try {
				editable = TextParseTools.ParseBool(properties.GetProperty("editable", true));
			} catch (FormatException) {
				return false;
			}
			widget.Editable = editable;
			return true;
		}
		private static bool SetWrapMode(Gtk.TextView widget, ConfigProperties properties, IInterfaceNode container) {
			Gtk.WrapMode mode;
			try {
				mode = TextParseTools.ParseWrapMode(properties.GetProperty("wrap_mode", String.Empty));
			} catch (FormatException) {
				return false;
			}
			widget.WrapMode = mode;
			return true;
		}
		private static bool SetTabAcceptance(Gtk.TextView widget, ConfigProperties properties, IInterfaceNode container) {
			bool accepted;
			try {
				accepted = TextParseTools.ParseBool(properties.GetProperty("accepts_tab", true));
			} catch (FormatException) {
				return false;
			}
			widget.AcceptsTab = accepted;
			return true;
		}
		private static bool SetBuffer(Gtk.TextView widget, ConfigProperties properties, IInterfaceNode container) {
			string bufferName = properties.GetProperty("buffer", null);
			if (bufferName != null) {
				Gtk.TextBuffer buffer = TextBufferFactory.GetBuffer(bufferName, container);
				widget.Buffer = buffer;
			}
			return true;

		}
	}
}

