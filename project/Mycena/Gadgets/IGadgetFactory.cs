﻿using System;
using System.Xml;

namespace Mycena {
	internal interface IGadgetFactory {
		/// <summary>
		/// Creates the gadget that is defined by the given node.
		/// </summary>
		/// <param name="rootNode">Node to create the gadget from.</param>
		/// <param name="container">Gadget container.</param>
		void CreateGadget(XmlNode rootNode, IInterfaceNode container);
	}
}

