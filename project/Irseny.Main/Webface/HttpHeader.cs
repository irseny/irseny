using System;
using System.Text;
using System.Collections.Generic;
using System.Net;
using Irseny.Core.Util;


namespace Irseny.Main.Webface {

	public class HttpHeader {
		public HttpMethod Method {  get; set; }
		public HttpStatusCode Status { get; set; }
		public Version Version {  get; set; }
		public string Resource { get; set; }
		public Dictionary<string, string> Fields { get; private set; }


		public HttpHeader(HttpMethod method) {
			Method = method;
			Status = HttpStatusCode.OK;
			Resource = string.Empty;
			Version = HttpVersion.Version11;
			Fields = new Dictionary<string, string>(32, StringComparer.OrdinalIgnoreCase);
		}
		public HttpHeader(HttpHeader source) {
			if (source == null) throw new ArgumentNullException("source");
			this.Method = source.Method;
			this.Status = source.Status;
			this.Resource = source.Resource;
			this.Version = source.Version;
			this.Fields = new Dictionary<string, string>(source.Fields);
		}

		public override string ToString() {
			var result = new StringBuilder((Fields.Count + 1)*32);
			// build head
			if (Method == HttpMethod.Response) { 
				result.Append("HTTP/");
				result.Append(Version.ToString());
				result.Append(' ');
				result.Append((int)Status);
				result.Append(' ');
				string sStatus = Enum.GetName(typeof(HttpStatusCode), Status);
				result.Append(sStatus.ToUpper());
				result.Append("\r\n");
			} else {
				string method = Enum.GetName(typeof(HttpMethod), Method);				
				result.Append(method.ToUpper());
				result.Append(' ');
				result.Append(Resource);
				result.Append(' ');
				result.Append("HTTP/");
				result.Append(Version.ToString());
				result.Append("\r\n");
			}
			// build fields
			foreach (var pair in Fields) {
				result.Append(pair.Key);
				result.Append(": ");
				result.Append(pair.Value);
				result.Append("\r\n");
			}
			// end with newline
			result.Append("\r\n");
			// try not to send more data after the header as this will clash with a content-length field
			//result.Append("\r\n");
			return result.ToString();
		}

		public static HttpHeader Parse(string text) {
			var result = new HttpHeader(HttpMethod.Response);

			// TODO test with existing method
			string[] split = text.Split("\r\n", StringSplitOptions.RemoveEmptyEntries);
			if (split.Length == 0) {
				throw new FormatException(text);
			}
			// read header
			string head = split[0];
			string[] sHead = head.Split(new char[1] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
			if (sHead.Length < 3) {
				throw new FormatException(head);
			}
			// read method

			try {
				result.Method = ParseHttpMethod(sHead[0]);
			} catch (FormatException) {
				throw new FormatException(sHead[0]);
			}
			// read resource
			result.Resource = sHead[1];
			// read protocol
			if (sHead[2].Length < ("HTTP/".Length + "1.1".Length)) {
				throw new FormatException(sHead[2]);
			}
			if (!sHead[2].StartsWith("HTTP/", StringComparison.InvariantCulture)) {
				throw new FormatException(sHead[2]);
			}
			string version = sHead[2].Substring("HTTP/".Length, 3);
			try {
				result.Version = Version.Parse(version);
			} catch (FormatException) {
				throw new FormatException(sHead[2]);
			}
			// read fields
			for (int s = 1; s < split.Length; s++) {
				string field = split[s];
				int appearance = field.IndexOf(':');
				if (appearance < 0) { 
					throw new FormatException(split[s]);
				}
				//if (appearance >= field.Length - 1) throw new FormatException(split[s]);
				string key = field.Substring(0, appearance).Trim();
				string value = string.Empty;
				if (appearance < field.Length - 1) {
					value = field.Substring(appearance + 1).Trim();
				}
				result.Fields.Add(key, value);
			}
			return result;
		}
		private static HttpMethod ParseHttpMethod(string text) {
			text = text.Trim().ToLower();
			if (text.Equals("get")) {
				return HttpMethod.Get;
			} else if (text.Equals("post")) {
				return HttpMethod.Post;
			} else {
				throw new FormatException(text);
			}
		}
	}
}
