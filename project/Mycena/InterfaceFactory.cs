﻿using System;
using System.Xml;
using System.Collections.Generic;

namespace Mycena {
	public class InterfaceFactory {
		/// <summary>
		/// Initializes a new instance of the <see cref="Mycena.InterfaceFactory"/> class 
		/// that uses the given document for widget creation.
		/// </summary>
		/// <param name="doc">Document.</param>
		private InterfaceFactory(XmlDocument doc) {
			if (doc == null) throw new ArgumentNullException("doc");
			Source = doc;
		}
		/// <summary>
		/// Gets or sets the source document.
		/// </summary>
		/// <value>The source document.</value>
		private XmlDocument Source { get; set; }

		/// <summary>
		/// Creates the widget with the given identifer alongside its children.
		/// </summary>
		/// <returns>Container with the widgets created.</returns>
		/// <param name="id">Root widget identifer.</param>
		public IInterfaceNode CreateWidget(string id) {
			if (id == null) throw new ArgumentNullException("id");
			var rootNode = WidgetFactory.FindWidgetNode(Source.DocumentElement, id);
			if (rootNode == null) throw new KeyNotFoundException("id");
			var result = WidgetFactory.CreateWidget(rootNode);
			return result;
		}

		/// <summary>
		/// Creates an instance of this class from the given glade file.
		/// </summary>
		/// <returns>Create instance.</returns>
		/// <param name="filePath">Path to glade file.</param>
		public static InterfaceFactory CreateFromFile(string filePath) {
			if (filePath == null) throw new ArgumentNullException("filePath");
			var doc = new XmlDocument();
			doc.Load(filePath);
			return new InterfaceFactory(doc);
		}
	}
}

