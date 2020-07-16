using System;
namespace Irseny.Main.Webface {
	// TODO change to struct
	public class HttpMessage {
		public HttpHeader Header {  get; private set; }
		public byte[] Content { get; private set; }

		public HttpMessage(HttpHeader header, byte[] content) {
			if (header == null) throw new ArgumentNullException("header");
			if (content == null) throw new ArgumentNullException("content");
			this.Header = header;
			this.Content = content;
		}
	}
}
