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
using System.Net;
using Irseny.Core.Util;
using System.Threading;
using Irseny.Core.Tracking;
using Irseny.Core.Tracking.HeadTracking;

namespace Irseny.Main.Webface.LiveWire {
	public class TrackerRequestHandler : StandardRequestHandler {
		LiveWireServer server;

		public TrackerRequestHandler(LiveWireServer server) : base() {
			if (server == null) throw new ArgumentNullException("server");
			this.server = server;
		}
		public override HttpStatusCode Respond(JsonString subject, JsonString response) {
			if (subject == null) throw new ArgumentNullException("subject");
			if (response == null) throw new ArgumentNullException("response");


			bool isGetRequest;
			string type = JsonString.ParseString(subject.GetTerminal("type", "error"), "error");
			if (type.Equals("get")) {
				isGetRequest = true;
			} else if (type.Equals("post")) {
				isGetRequest = false;
			} else {
				return HttpStatusCode.BadRequest;
			}
			int[] positions = ReadPosition(subject);
			if (positions == null) {
				return HttpStatusCode.BadRequest;
			}
			var responseSubject = JsonString.CreateDict();
			{
				responseSubject.AddTerminal("type", JsonString.StringifyString("post"));
				responseSubject.AddTerminal("topic", JsonString.StringifyString("tracker"));
			}
			response.AddJsonString("subject", responseSubject);

			if (isGetRequest) {
				return RespondGet(subject, responseSubject, positions);
			} else {
				HttpStatusCode status = RespondPost(subject, responseSubject, positions);
				// not only send the result to one recipient but create an update for all clients
				if (status == HttpStatusCode.OK) {
					var update = new JsonString(response);
					update.RemoveTerminal("requestId");
					update.AddTerminal("target", "\"all\"");
					server.PostUpdate(update);
				}
				return status;
			}
		}

		private HttpStatusCode RespondGet(JsonString requestSubject, JsonString responseSubject, int[] positions) {
			object dataSync = new object();
			JsonString data;
			lock (dataSync) { // TODO lock required?
				data = JsonString.CreateDict();
			}
			var readySignal = new ManualResetEvent(false);
			TrackingSystem.Instance.Invoke((object sender, EventArgs args) => {
				var system = (TrackingSystem)sender;
				foreach (int pos in positions) {
					var entry = JsonString.CreateDict();
					IPoseTracker tracker = system.GetTracker(pos);
					if (tracker == null) {
						entry.AddTerminal("inuse", "false");
					} else if (tracker is P3CapTracker) {
						var capTracker = (P3CapTracker)tracker;
						EquipmentSettings settings = capTracker.GetSettings();
						entry.AddTerminal("inuse", "true");
						entry.AddJsonString("settings", EquipmentSettings.ToJson(settings));
					}
					data.AddJsonString(JsonString.Stringify(pos), entry);
				}
				readySignal.Set();
			});
			readySignal.WaitOne();
			lock (dataSync) {
				responseSubject.AddJsonString("data", data);
			}
			return HttpStatusCode.OK;
		}
		private HttpStatusCode RespondPost(JsonString requestSubject, JsonString responseSubject, int[] positions) {
			if (positions.Length > 1) {
				return HttpStatusCode.BadRequest;
			}
			return HttpStatusCode.InternalServerError;
		}
	}
}

