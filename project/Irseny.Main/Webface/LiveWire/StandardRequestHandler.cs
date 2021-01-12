using System;
using System.Net;
using Irseny.Core.Util;

namespace Irseny.Main.Webface.LiveWire {
	public abstract class StandardRequestHandler {
		protected readonly int MaxRequestWaitTime = 10000;

		protected int MinPosition { get; set; }
		protected int MaxPosition { get; set; }
		protected string PositionKey { get; set; }
		public StandardRequestHandler () {
			MinPosition = 0;
			MaxPosition = 15;
			PositionKey = "position";
		}
		/// <summary>
		/// Reads the position information from a subject given in standard format.
		/// The position information may be composed of a single index or the 'all' keyword.
		/// </summary>
		/// <returns><c>true</c>, if position was read successfully, <c>false</c> otherwise.</returns>
		/// <param name="subject">Subject string.</param>
		/// <param name="lowBound">Starting position.</param>
		/// <param name="highBound">Ending position.</param>
		protected virtual bool ReadPosition(JsonString subject, out int lowBound, out int highBound) {
			lowBound = MinPosition - 1;
			highBound = MinPosition - 1;
			string position = TextParseTools.ParseString(subject.GetTerminal(PositionKey, "all"), "all");
			if (position.Equals("all")) {
				lowBound = MinPosition;
				highBound = MaxPosition;
			} else {
				lowBound = TextParseTools.ParseInt(position, -1);
				highBound = lowBound;
			}
			return lowBound >= MinPosition && lowBound <= MaxPosition;
		}
		/// <summary>
		/// Creates a response to the request formulated in the given subject JSON string.
		/// Fails with a status code other than 200 in case of inability to respond.
		/// It is the responsibility of the caller to add required response information
		/// from the original request that is unavailable to this method.
		/// It is the responsibility of the caller to extend the given JSON string to an error indicator
		/// in case inability to respond.
		/// </summary>
		/// <param name="subject">Subject.</param>
		/// <param name="response">Response.</param>
		/// <returns>Status code which indicates success or failure</returns>
		public abstract HttpStatusCode Respond(JsonString subject, JsonString response);
	}
}

