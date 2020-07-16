using System;
namespace Irseny.Main.Webface {

	public interface IWebJunction {
		/// <summary>
		/// Adds a channel to begin handshake. The given channel gets occupied by this instance.
		/// Typically called from a lower level junction 
		/// when the connection is supposed to operate at a higher level protocol
		/// with a newly created channel object.
		/// </summary>
		/// <param name="channel">Channel to add.</param>
		void AddChannelPrototype(IWebChannel channel);
		/// <summary>
		/// Signals the rejection of a channel prototype.
		/// This method is typically called by a higher level junction 
		/// when the handshake for a higher level protocol has failed.
		/// </summary>
		/// <param name="channel">Channel prototoype that failed connecting.</param>
		void RejectChannelPrototype(IWebChannel channel);
		/// <summary>
		/// Signals the acceptance of a channel prototype.
		/// This method is typically called by a higher level junction
		/// when the handshake for a higher level protocol succeeds.
		/// </summary>
		/// <param name="channel">Channel prototype that succeeded connecting.</param>
		void AcceptChannelPrototype(IWebChannel channel);
		/// <summary>
		/// Singnals the discontinuation of a channel.
		/// This method is typically called when a connection ends at a higher level protocol.
		/// </summary>
		/// <param name="channel">Channel to close. Must have been added as a prototype earlier.</param>
		void CloseChannel(IWebChannel channel);
		/// <summary>
		/// Continues channel processing.
		/// </summary>
		void Process();
		/// <summary>
		/// Adds a junction to the junction tree.
		/// </summary>
		/// <param name="junction">Junction to add.</param>
		/// <param name="pathStart">Index in the path to start at.</param>
		/// <param name="path">Path of junctions to traverse before adding.</param>
		void AddJunction(IWebJunction junction, string[] path, int pathStart=0);
		/// <summary>
		/// Gets a junction from the junction tree.
		/// </summary>
		/// <returns>The junction specified by the given path.</returns>
		/// <param name="pathStart">Index in the path to start at.</param>
		/// <param name="path">Path of junctions to traverse before returning.</param>
		IWebJunction GetJunction(string[] path, int pathStart=0);
	}
}
