using System;

namespace Mycena {
	internal abstract class WidgetFactory<T> where T : Gtk.Widget {
		
		public WidgetFactory() {
			RootNode = null;
			Register = null;
		}
		/// <summary>
		/// Gets the widget root node.
		/// </summary>
		/// <value>The root node.</value>
		protected System.Xml.XmlNode RootNode { get; private set; }
		/// <summary>
		/// Gets the widget register.
		/// </summary>
		/// <value>The register.</value>
		protected IWidgetRegister Register { get; private set; }
		/// <summary>
		/// Setups widget creation.
		/// </summary>
		/// <param name="rootNode">Root node to use.</param>
		/// <param name="register">Register to use.</param>
		public void Start(System.Xml.XmlNode rootNode, IWidgetRegister register) {
			if (rootNode == null) throw new ArgumentNullException("rootNode");
			if (register == null) throw new ArgumentNullException("register");
			RootNode = rootNode;
			Register = register;
		}

	}
}

