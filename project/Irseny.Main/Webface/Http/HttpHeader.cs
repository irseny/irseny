// This file is part of Irseny.
//
// Copyright (C) 2021  Thilo Gabel
//
// Irseny is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
//
// Irseny is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
//
// You should have received a copy of the GNU General Public License
// along with this program.  If not, see <https://www.gnu.org/licenses/>.

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


		private HttpHeader() {
			Method = HttpMethod.Response;
			Status = HttpStatusCode.OK;
			Resource = string.Empty;
			Version = HttpVersion.Version11;
			Fields = new Dictionary<string, string>(32, StringComparer.OrdinalIgnoreCase);
		}
		

		public HttpHeader(Version version, HttpStatusCode status) : this() {
			Version = version;
			Status = status;
		}
		public HttpHeader(HttpMethod method, string resource, Version version, HttpStatusCode status) : this() {
			Method = method;
			Resource = resource;
			Version = version;
			Status = status;
		}
		public HttpHeader(HttpHeader source) : this() {
			if (source == null) throw new ArgumentNullException("source");
			this.Method = source.Method;
			this.Status = source.Status;
			this.Resource = source.Resource;
			this.Version = source.Version;
			this.Fields = new Dictionary<string, string>(source.Fields, StringComparer.OrdinalIgnoreCase);
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
			var result = new HttpHeader();
			// TODO make compatible with RESPONSE messages
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
			} else if (text.Equals(string.Empty)) {
				return HttpMethod.Response;
			} else {
				throw new FormatException(text);
			}
		}
	}
}
