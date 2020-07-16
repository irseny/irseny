using System;
using System.Collections.Generic;

namespace Irseny.Main.Webface {
	public abstract class WebJunction : IWebJunction {
		protected abstract IWebJunction ParentJunction { get; }

		// channels added to this instance
		private List<IWebChannel> addedChannels;
		// channels added to children, handshake not finished, original channel and untested junctions
		private Dictionary<IWebChannel, Tuple<IWebChannel, HashSet<string>>> prototypeChannels;
		// child junctions
		private Dictionary<string, IWebJunction> junctions;

		public WebJunction() {
			addedChannels = new List<IWebChannel>();
			prototypeChannels = new Dictionary<IWebChannel, Tuple<IWebChannel, HashSet<string>>>();
			junctions = new Dictionary<string, IWebJunction>();
		}

		public void AddJunction(IWebJunction junction, string[] path, int pathStart) {
			if (junction == null) throw new ArgumentNullException("junction");
			if (path == null) throw new ArgumentNullException("path");
			if (path.Length < 1) throw new ArgumentException("path.Length");
			if (pathStart < 0) throw new ArgumentOutOfRangeException("pathStart");
			if (pathStart >= path.Length) throw new ArgumentOutOfRangeException("pathStart");
			string name = path[pathStart];
			if (pathStart < path.Length - 1) {
				// pass operation to child
				IWebJunction child;
				if (!junctions.TryGetValue(name, out child)) {
					throw new ArgumentOutOfRangeException("path");
				}
				child.AddJunction(junction, path, pathStart + 1);
			} else {
				// add junction to this instance
				if (junctions.ContainsKey(name)) {
					throw new ArgumentException("path");
				}
				//if (junction.ParentJunction != this) {
				//	throw new ArgumentException("junction.ParentJunction");
				//}
				junctions.Add(name, junction);
			}
		}
		public IWebJunction GetJunction(string[] path, int pathStart) {
			if (path == null) throw new ArgumentNullException("path");
			if (path.Length < 1) throw new ArgumentException("path");
			if (pathStart < 1) throw new ArgumentOutOfRangeException("pathStart");
			if (pathStart >= path.Length) throw new ArgumentOutOfRangeException("pathStart");
			string name = path[pathStart];
			if (pathStart < path.Length - 1) {
				// return result from child
				IWebJunction child;
				if (!junctions.TryGetValue(name, out child)) {
					throw new ArgumentOutOfRangeException("path");
				}
				return child.GetJunction(path, pathStart + 1);
			} else {
				// return child of this instance
				IWebJunction result;
				if (!junctions.TryGetValue(name, out result)) {
					throw new ArgumentOutOfRangeException("path");
				}
				return result;
			}
		}
		public void AddChannelPrototype(IWebChannel channel) {
			if (channel == null) throw new ArgumentNullException("channel");

		}
		public void RejectChannelPrototype(IWebChannel channel) {
			if (channel == null) throw new ArgumentNullException("channel");
			Tuple<IWebChannel, HashSet<string>> status;
			if (prototypeChannels.TryGetValue(channel, out status)) {
				// TODO complete
			}
			// do nothing if the given channel is unknown
		}
		public void AcceptChannelPrototype(IWebChannel channel) {
			if (channel == null) throw new ArgumentNullException("channel");
			// hand off responsibility to child junction
			Tuple<IWebChannel, HashSet<string>> status;
			if (prototypeChannels.TryGetValue(channel, out status)) {
				var source = status.Item1;
				if (addedChannels.Contains(source)) {
					// remove from instance
					addedChannels.Remove(source);
				}
				prototypeChannels.Remove(channel);
			}
			// do nothing if the given channel is unknown
		}
		public void CloseChannel(IWebChannel channel) {
			if (channel == null) throw new ArgumentNullException("channel");

			Tuple<IWebChannel, HashSet<string>> status;
			if (prototypeChannels.TryGetValue(channel, out status)) {
				// continue to remove channel from junction tree
				var source = prototypeChannels[channel].Item1;
				if (addedChannels.Contains(source)) {
					// remove from this instance
					addedChannels.Remove(source);
					// remove from parent
					if (ParentJunction != null) {
					ParentJunction.CloseChannel(source);
					}
				}
				// remove from this instance
				prototypeChannels.Remove(channel);
			}
			// if the channel is not in the prototypes, responsibility has been moved to another junction before
			// nothing to be done in this case

		}
		public void Process() {
			var acceptDiff = new List<IWebChannel>();
			var closeDiff = new List<IWebChannel>();
			var rejectDiff = new List<IWebChannel>();
			// first process all non accepted channels
			foreach (IWebChannel channel in addedChannels) {

				channel.Process();
				switch (channel.State) {
				case WebChannelState.Open:

					if (ParentJunction != null) { 
						ParentJunction.AcceptChannelPrototype(channel);
					}
					acceptDiff.Add(channel);
				break;
				case WebChannelState.Closed:
					if (ParentJunction != null) {
						ParentJunction.CloseChannel(channel);
					}
					closeDiff.Add(channel);
				break;
				case WebChannelState.SetupFailed:
					if (ParentJunction != null) {
						ParentJunction.RejectChannelPrototype(channel);
					}
					rejectDiff.Add(channel);
				break;
				default:
					// nothing to do
				break;
				}
			}
			// process diffs
			foreach (IWebChannel channel in acceptDiff) {
				addedChannels.Remove(channel);

			}
			foreach (IWebChannel channel in closeDiff) {
				addedChannels.Remove(channel);
			}
			foreach (IWebChannel channel in rejectDiff) {
				addedChannels.Remove(channel);
			}
			// process all accepted channels

		}
	}

}
