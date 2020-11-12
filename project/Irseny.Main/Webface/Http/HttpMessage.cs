using System;
namespace Irseny.Main.Webface {
	// TODO change to struct
	public class HttpMessage {
		public HttpHeader Header {  get; private set; }
		public byte[] Content { get; private set; }
		bool HasContent {
			get { return Content != null && Content.Length > 0; }
		}

		public HttpMessage(HttpHeader header, byte[] content) {
			if (header == null) throw new ArgumentNullException("header");
			if (content == null) throw new ArgumentNullException("content");
			Header = header;
			Content = content;
		}
		public HttpMessage(HttpHeader header) {
			if (header == null) throw new ArgumentNullException("header");
			Header = header;
			Content = new byte[0];
		}

	}
}
