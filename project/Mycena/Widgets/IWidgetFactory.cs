using System;

namespace Mycena {
	internal interface IWidgetFactory {
		bool IsContainer { get; }
		bool AllowRegister { get; }
		bool AllowChildren { get; }
		Gtk.Widget CreateWidget(System.Xml.XmlNode rootNode, IWidgetRegister target);
	}
}

