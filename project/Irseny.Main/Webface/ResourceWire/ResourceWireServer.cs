using System;
using System.IO;
using System.Collections.Generic;

namespace Irseny.Main.Webface.Services {
	public class ResourceWireServer {
		public ResourceWireServer() {
		}
		/// <summary>
		/// Provides the resource specified by name.
		/// </summary>
		/// <returns>The resource. Empty if not available.</returns>
		/// <param name="name">Resource name.</param>
		public byte[] ProvideResource(string name) {
			if (name == null) throw new ArgumentNullException("name");
			if (name.StartsWith("/")) {
				name = name.Substring(1);
			}
			if (name.Length == 0) {
				name = "index.html";
			}
			string filePath;
			if (name.StartsWith("lib/")) {
				string prefix = Path.GetFullPath("../");
				filePath = Path.Combine(prefix, name);
			} else if (name.StartsWith("icon/")) {
				string prefix = Path.GetFullPath("../resources/icons/");
				filePath = Path.Combine(prefix, name.Substring(5));
			} else {
				string prefix = Path.GetFullPath("../project/Irseny.Webfront/");
				filePath = Path.Combine(prefix, name);
			}

			
			if (!File.Exists(filePath)) {
				Console.WriteLine("cannot provide " + name + " as " + filePath);
				return new byte[0];
			}
			return ProvideSource(filePath);
		}
		private byte[] ProvideSource(string filePath) {

			var bits = new List<byte[]>();
			try { 
				using (var stream = File.OpenRead(filePath)) {
					int bytesRead;
					do { 
						byte[] bit = new byte[1024];
						bytesRead = stream.Read(bit, 0, bit.Length);
						if (bytesRead == bit.Length) {
							bits.Add(bit);
						} else {
							return Concat(bits, bit, bytesRead);
						}

					} while (bytesRead == 1024);
				}
			} catch (FileNotFoundException) {
				// nothing can be done
			} catch (IOException) {
			}
			return new byte[0];
		}
		private byte[] Concat(List<byte[]> bits, byte[] lastBit, int lastBitLength) {
			int totalLength = lastBitLength;
			foreach (byte[] bit in bits) {
				totalLength += bit.Length;
			}
			byte[] result = new byte[totalLength];
			int resultLength = 0;
			foreach (byte[] bit in bits) {
				Array.Copy(bit, 0, result, resultLength, bit.Length);
				resultLength += bit.Length;
			}
			Array.Copy(lastBit, 0, result, resultLength, lastBitLength);
			return result;
		}
	}
}
