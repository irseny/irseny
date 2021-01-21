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
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace Irseny.Main.Webface {
	public class HttpChannel : IWebChannel<HttpMessage> {
		TcpChannel source;
		LinkedList<HttpMessage> availableMessages;
		List<byte[]> messageBits;

		int snoopedBitNo = 0;
		string messageHeader = string.Empty;
		byte[] messageContent = new byte[0];
		int missingContentLength = 0;


		public int AvailableMessageNo {
			get { return availableMessages.Count; }
		}
		private bool HeaderAvailable {
			get {  return messageHeader.Length > 0; }
		}
		private bool ContentAvailable {
			get { return messageContent.Length > 0 && messageContent.Length >= missingContentLength; }
		}
		public WebChannelState State { get; protected set; }

		private HttpChannel() {
			this.availableMessages = new LinkedList<HttpMessage>();
			this.messageBits = new List<byte[]>();
			this.State = WebChannelState.Initializing;
			//this.messageBits = new StringBuilder();
		}
		public HttpChannel(TcpChannel source) : this() {
			if (source == null) throw new ArgumentNullException("source");
			this.source = source;

		}

		public bool SendMessage(HttpMessage message) {
			if (message == null) throw new ArgumentNullException("message");
			string header = message.Header.ToString();

			byte[] bHeader = Encoding.UTF8.GetBytes(header);
			if (source.SendMessage(bHeader)) {
				byte[] content = message.Content;
				if (content.Length > 0) {
					return source.SendMessage(content);
				}
				return true;
			}
			return false;
			//byte[] buffer = Encoding.UTF8.GetBytes(message);
			//source.SendMessage(buffer, 0, buffer.Length);
		}
		public HttpMessage SnoopMessage(int index=0) {
			if (availableMessages.Count == 0 || index >= availableMessages.Count) {
				return null;
			}
			var node = availableMessages.First;
			for (int i = index; index > 0; i++) {
				node = node.Next;
			}
			return node.Value;
		}
		public HttpMessage EmitMessage() {
			if (availableMessages.Count == 0) {
				return null;
			}
			HttpMessage result = availableMessages.First.Value;
			availableMessages.RemoveFirst();
			return result;
		}

		public void Process() {
			// test for invalid state
			if (State == WebChannelState.Closed || State == WebChannelState.InitFailed) {
				return;
			}
			source.Process();
			if (source.State == WebChannelState.Closed) {
				State = WebChannelState.Closed;
				return;
			}
			if (State == WebChannelState.InitFailed) {
				State = WebChannelState.InitFailed;
				return;
			}
			if (State == WebChannelState.Initializing) { 
				// only snoop new messages if still initializing
				int messageNo = source.AvailableMessageNo;
				while(messageNo > snoopedBitNo) {
					byte[] bit = source.SnoopMessage(snoopedBitNo);
					if (bit.Length <= 0) throw new InvalidOperationException();
					messageBits.Add(bit);
					snoopedBitNo += 1;
				}
				// try to extract a header
				if (messageHeader.Length == 0) {
					int headerLength = ExtractHeaderLength();
					if (headerLength > 0) {
						messageHeader = ExtractHeader(headerLength);
						missingContentLength = ExtractContentLength(messageHeader);
					}
				}
				// try to extract message content
				if (messageHeader.Length > 0 && missingContentLength > 0) {
					messageContent = ExtractContent(messageHeader.Length, missingContentLength);
					if (messageContent.Length >= 0) {
						// first message extraction successful
						PostMessage(messageHeader.Length, missingContentLength);
						BecomeOpen();
					}
				}
				// or go with no content if this is not required
				if (messageHeader.Length > 0 && missingContentLength == 0) {
					PostMessage(messageHeader.Length, 0);
					BecomeOpen();
				}
			} else {
				// read new messages
				int messageNo = source.AvailableMessageNo;
				if (messageNo > 0) {
					byte[] bit = source.EmitMessage();
					if (bit.Length == 0) throw new InvalidOperationException();
					messageBits.Add(bit);
				}
				// try to extract a header
				if (messageHeader.Length == 0) {
					int headerLength = ExtractHeaderLength();
					if (headerLength > 0) {
						messageHeader = ExtractHeader(headerLength);
						missingContentLength = ExtractContentLength(messageHeader);
					}
				}
				// try to extract message content
				if (messageHeader.Length > 0 && missingContentLength > 0) {
					messageContent = ExtractContent(messageHeader.Length, missingContentLength);
					if (messageContent.Length >= 0) {
						PostMessage(messageHeader.Length, missingContentLength);
					}
				}
			}
		}
		private int ExtractHeaderLength() {
			char[] pattern = new char[] { '\r', '\n', '\r', '\n' };
			int patternProgress = 0;
			int headerLength = 0;
			// go through all bits so far
			for (int b = 0; b < messageBits.Count; b++) {
				byte[] bit = messageBits[b];
				for (int i = 0; i < bit.Length; i++) {
					headerLength += 1;
					// and match the pattern
					if (bit[i] == pattern[patternProgress]) {
						patternProgress += 1;
						// stop when the pattern has been found
						if (patternProgress >= pattern.Length) {
							return headerLength;
						}
					} else {
						if (patternProgress > 0) {
							patternProgress = 0;
							// also test the current bit against the first pattern character
							if (bit[i] == pattern[0]) {
								patternProgress = 1;
							}
						} 
					}
				}
			}
			return -1;
		}
		private string ExtractHeader(int headerLength) {
			if (headerLength <= 0) throw new InvalidOperationException();
			var result = new StringBuilder(headerLength);
			int bytesProcessed = 0;
			// preprocess to estimate 
			for (int b = 0; b < messageBits.Count; b++) {
				byte[] bit = messageBits[b];
				// progress the last bit
				if (bytesProcessed + bit.Length >= headerLength) {
					string sBit = Encoding.UTF8.GetString(bit, 0, headerLength - bytesProcessed);
					result.Append(sBit);
					return result.ToString();
				} else { 
					string sBit = Encoding.UTF8.GetString(bit);
					result.Append(sBit);
				}
			}
			return string.Empty;
		}
		private int ExtractContentLength(string header) {
			if (!header.Contains("Content-Length:")) {
				return 0;
			}
			var match = Regex.Match(header, "Content-Length:(.*)\r\n");
			Console.WriteLine("match: " + match.Groups[1]);
			return 0;
		}
		private byte[] ExtractContent(int headerLength, int contentLength) {
			if (contentLength <= 0) throw new InvalidOperationException();
			// first evaluate that enough data has been received
			int messageLength = 0;
			for (int b = 0; b < messageBits.Count; b++) {
				messageLength += messageBits[b].Length;
			}
			if (messageLength < headerLength + contentLength) {
				// stop if not enough data available
				return new byte[0];
			}
			// extract content
			byte[] result = new byte[contentLength];
			int resultLength = 0;
			int toSkip = headerLength;
			for (int b = 0; b < messageBits.Count; b++) {
				byte[] bit = messageBits[b];
				// first skip the header
				if (toSkip > 0) {
					if (bit.Length <= toSkip) {
						// skip the whole bit
						toSkip -= bit.Length;
					} else {
						// skip only parts of the bit
						int toCopy = bit.Length - toSkip;
						Array.Copy(bit, toSkip, result, resultLength, toCopy);
						resultLength += toCopy;
						toSkip = 0;
					}
				} else { 
					// copy to the result buffer
					int contentRemaining = contentLength - resultLength;
					if (bit.Length <= contentRemaining) {
						// copy the whole bit over
						Array.Copy(bit, 0, result, resultLength, bit.Length);
						resultLength += bit.Length;
					} else {
						// copy only parts of the bit
						Array.Copy(bit, 0, result, resultLength, contentRemaining);
						resultLength += contentRemaining;
					}
					if (resultLength >= contentLength) {
						break;
					}
				}
			}
			return result;
		}
		private void PostMessage(int headerLength, int contentLength) {
			if (headerLength <= 0 && contentLength <= 0) throw new InvalidOperationException();
			// throw away the message bits belonging to the header and content
			int toSkip = headerLength + contentLength;
			var nextMessageBits = new List<byte[]>(messageBits.Count);
			for (int b = 0; b < messageBits.Count; b++) {
				byte[] bit = messageBits[b];
				if (toSkip <= 0) {
					// keep the remaining bits
					nextMessageBits.Add(bit);
				} else if (bit.Length <= toSkip) {
					// throw away the bit
					toSkip -= bit.Length;
				} else {
					// use parts of the bit
					int toCopy = bit.Length - toSkip;
					byte[] nextBit = new byte[toCopy];
					Array.Copy(bit, toSkip, nextBit, 0, toCopy);
					nextMessageBits.Add(nextBit);
					toSkip = 0;
				}
			}
			messageBits = nextMessageBits;
			// make the message available
			availableMessages.AddLast(new HttpMessage(HttpHeader.Parse(messageHeader), messageContent));
			// TODO add content
			missingContentLength = 0;
			messageContent = new byte[0];
			messageHeader = string.Empty;
		}
		private void BecomeOpen() {
			if (State != WebChannelState.Initializing) throw new InvalidOperationException();
			// claim received messages
			for (int i = 0; i < snoopedBitNo; i++) {
				// the source is supposed to drop the bit that we have read earlier
				source.EmitMessage();
			}
			// set state accordingly
			snoopedBitNo = 0;
			State = WebChannelState.Open;
		}
		public void Close(bool closeElementary=true) {
			if (closeElementary) {
				source.Close();
			}
			State = WebChannelState.Closed;
		}
		public void Flush() {
			source.Flush();
		}

	}
}
