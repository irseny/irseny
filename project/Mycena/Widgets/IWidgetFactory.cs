using System;

namespace Mycena {
	internal interface IWidgetFactory {
		/// <summary>
		/// Gets the class identifier.
		/// </summary>
		/// <value>The identifier of the class.</value>
		string ClassName { get; }
		/// <summary>
		/// Gets a value indicating whether the widgets created can contain other widgets.
		/// </summary>
		/// <value><c>true</c> if this instance produces container widgets; otherwise, <c>false</c>.</value>
		bool IsContainer { get; }
		/// <summary>
		/// Gets a value indicating whether to register the widgets created.
		/// This value does not impact child widgets.
		/// </summary>
		/// <value><c>true</c> if registering is enabled; otherwise, <c>false</c>.</value>
		bool AllowRegister { get; }
		/// <summary>
		/// Gets a value indicating whether to create child widgets.
		/// </summary>
		/// <value><c>true</c> if child creation is enabled; otherwise, <c>false</c>.</value>
		bool AllowChildren { get; }
		/// <summary>
		/// Creates the widget defined by the given <see cref="System.Xml.XmlNode"/>  alongside its children.
		/// </summary>
		/// <returns>The widget created.</returns>
		/// <param name="rootNode">Node to create the widget from.</param>
		/// <param name="target">Widget register.</param>
		Gtk.Widget CreateWidget(System.Xml.XmlNode rootNode, IWidgetRegister target);
	}
}

