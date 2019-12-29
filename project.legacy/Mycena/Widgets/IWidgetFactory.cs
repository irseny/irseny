using System;
using System.Xml;

namespace Mycena {
	internal interface IWidgetFactory {
		/// <summary>
		/// Creates the widget defined by the given <see cref="System.Xml.XmlNode"/>  alongside its children.
		/// </summary>
		/// <returns>The widget created.</returns>
		/// <param name="rootNode">Node to create the widget from.</param>
		/// <param name="container">Widget container.</param>
		/// <param name="rootFactory">Factory to create child elements with.</param>
		Gtk.Widget CreateWidget(XmlNode rootNode, IInterfaceNode container, WidgetFactory rootFactory);
	}
}

