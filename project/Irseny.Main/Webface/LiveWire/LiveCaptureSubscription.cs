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
using Irseny.Core.Util;
using Irseny.Core.Sensors;
using System.Security.Cryptography;

namespace Irseny.Main.Webface.LiveWire {
	/// <summary>
	/// This class represents subscription state with a message passing interface for equipment data subscriptions.
	/// It is typically instanciated when a live client sends a capture request through <see cref="LiveWireServer"/>.
	/// The capture request is then temporarily active until cancelled.
	/// </summary>
	public class LiveCaptureSubscription : IDisposable {

		bool cancelled;
		readonly ulong subscriptionID;
		readonly object messageSync;
		Queue<JsonString> messages;

		/// <summary>
		/// Initializes a new instance of the <see cref="Irseny.Main.Webface.LiveCaptureSubscription"/> class.
		/// </summary>
		/// <param name="subscriptionID">Unique subscription identifier. Generated with <see cref="GenerateSubscriptionID"/></param>
		public LiveCaptureSubscription(ulong id) {
			subscriptionID = id;
			cancelled = false;
			messageSync = new object();
			messages = new Queue<JsonString>();
		}
		/// <summary>
		/// Gets the subscription ID.
		/// This value is usually generated in <see cref="GenerateSubscriptionID"/>
		/// and is used to check whether a particular subscription already exists.
		/// </summary>
		/// <value>The subscription I.</value>
		public ulong SubscriptionID {
			get { return subscriptionID; }
		}
		/// <summary>
		/// Indicates whether the subscription has been marked for removal.
		/// If this is set further calls to <see cref="EnqueueMessage"/> should be avoided.
		/// </summary>
		/// <value><c>true</c> if this instance is cancelled; otherwise, <c>false</c>.</value>
		public bool IsCancelled {
			get {
				lock (messageSync) {
					return cancelled;
				}
			}
		}
		/// <summary>
		/// Gets the number of available messages.
		/// </summary>
		/// <value>Message number.</value>
		public int AvailableMessageNo {
			get { 
				lock (messageSync) {
					return messages.Count;
				}
			}
		}
		/// <summary>
		/// Emits the next available message.
		/// </summary>
		/// <returns>The message.</returns>
		public JsonString EmitMessage() {
			lock (messageSync) {
				if (messages.Count == 0) {
					return null;
				}
				return messages.Dequeue();
			}
		}
		/// <summary>
		/// Peeks for the next available mesage.
		/// </summary>
		/// <returns>The next message.</returns>
		public JsonString PeekMessage() {
			lock (messageSync) {
				if (messages.Count == 0) {
					return null;
				}
				return messages.Peek();
			}
		}
		/// <summary>
		/// Adds an available message to emit.
		/// </summary>
		/// <param name="message">Message.</param>
		public void EnqueueMessage(JsonString message) {
			if (message == null) throw new ArgumentNullException("message");
			lock (messageSync) {
				messages.Enqueue(message);
			}
		}
		/// <summary>
		/// Marks the subscription for removal.
		/// </summary>
		public virtual void Cancel() {
			lock (messageSync) {
				cancelled = true;
			}
		}
		/// <summary>
		/// Excludes the subscription from further processing.
		/// There should not be further method calls made on the object
		/// after calling this method.
		/// Does not cancel the subscription. <see cref="Cancel()"/> should be called first.
		/// </summary>
		public virtual void Dispose() {
			// nothing to do
			// inheriting members can put further logic here
		}

		/// <summary>
		/// Generates a unique subscription ID that can be passed to <see cref="LiveCaptureSubscription()"/>
		/// </summary>
		/// <returns>The generated ID.</returns>
		/// <param name="connectionIndex">Client origin.</param>
		/// <param name="equpimentType">Equipment type.</param>
		/// <param name="equipmentIndex">Equipment index to observe.</param>
		public static ulong GenerateSubscriptionID(int connectionIndex, SensorType equpimentType, int equipmentIndex) {
			// concat arguments to input array
			byte[] bConnection = BitConverter.GetBytes(connectionIndex);
			byte[] bSensor = BitConverter.GetBytes((int)equpimentType);
			byte[] bIndex = BitConverter.GetBytes(equipmentIndex);
			byte[] input = new byte[bConnection.Length + bSensor.Length + bIndex.Length];
			int offset = 0;
			Array.Copy(bConnection, 0, input, offset, bConnection.Length);
			offset += bConnection.Length;
			Array.Copy(bSensor, 0, input, offset, bSensor.Length);
			offset += bSensor.Length;
			Array.Copy(bIndex, 0, input, offset, bIndex.Length);
			// comptute some hash
			byte[] hash;
			using (MD5 md5 = MD5Cng.Create()) {
				hash = md5.ComputeHash(input);
			}
			// reduce the hash to a shorter value
			ulong checkSum = 0x0;
			if (hash.Length < sizeof(ulong)) {
				// fail safe
				// generate something if the hash is somehow very short
				for (int i = 0; i < hash.Length; i++) {
					checkSum |= ((ulong)hash[i]<<(8*i));
				}
				return checkSum;
			}

			for (int i = 0; i + sizeof(ulong) <= hash.Length; i+= sizeof(ulong)) {
				checkSum ^= BitConverter.ToUInt64(hash, i);
			}

			return checkSum;
		}
	}
}

