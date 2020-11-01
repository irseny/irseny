using System;
using System.Collections.Generic;

namespace Irseny.Main.Webface {
	/// <summary>
	/// A web channel represents a single connection that operates according to a specific protocol.
	/// Initially the channel goes through a connection setup phase.
	/// Afterwards the channel failed initialization or is ready for message exchange until it is closed.
	/// </summary>
	public interface IWebChannel {

		/// <summary>
		/// Gets the state of operation this object.
		/// </summary>
		/// <value>The state of operation.</value>
		WebChannelState State { get; }
		/// <summary>
		/// Gets the number of available messages.
		/// </summary>
		/// <value>The available message no.</value>
		int AvailableMessageNo { get; }
		/// <summary>
		/// Continues processing setup, receiving and sending messages.
		/// </summary>
		void Process();
		/// <summary>
		/// Closes this channel.
		/// </summary>
		/// <param name="closeElementary">Whether or not to close elementary channels and connections.</param>
		void Close(bool closeElementary=true);
		/// <summary>
		/// Used to finish writing internal buffers.
		/// </summary>
		void Flush();
	}
	public interface IWebChannel<T> : IWebChannel {

		/// <summary>
		/// Dequeues and returns the next available message.
		/// </summary>
		/// <returns>The message.</returns>
		T EmitMessage();
		/// <summary>
		/// Returns an available message without dequeuing it.
		/// </summary>
		/// <returns>The message.</returns>
		/// <param name="depth">Depth.</param>
		T SnoopMessage(int depth=0);
		/// <summary>
		/// Enqueues a message to be sent.
		/// </summary>
		/// <param name="message">Message to send.</param>
		void SendMessage(T message);

	}
}
