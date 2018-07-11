using System;
namespace Irseny.Tracap {
	public abstract class CapTracker : ICapTracker {
		readonly object inputEventSync = new object();
		readonly object detectedEventSync = new object();
		readonly object executionSync = new object();
		event EventHandler<EventArgs> positionDetected;
		event EventHandler inputAvailable;
		public CapTracker() {
		}
		~CapTracker() {
			Dispose();
		}
		public event EventHandler<EventArgs> PositionDetected {
			add {
				lock (detectedEventSync) {
					positionDetected += value;
				}
			}
			remove {
				lock (detectedEventSync) {
					positionDetected -= value;
				}
			}
		}
		public event EventHandler InputAvailable {
			add {
				lock (inputEventSync) {
					inputAvailable += value;
				}
			}
			remove {
				lock (inputEventSync) {
					inputAvailable -= value;
				}
			}
		}
		protected virtual void OnInputAvailable(EventArgs args) {
			if (args == null) throw new ArgumentNullException("args");
			EventHandler handler;
			lock (inputEventSync) {
				handler = inputAvailable;
			}
			if (handler != null) {
				handler(this, args);
			}
		}
		protected virtual void OnPositionDetected(EventArgs args) {
			if (args == null) throw new ArgumentNullException("args");
			EventHandler<EventArgs> handler;
			lock (detectedEventSync) {
				handler = positionDetected;
			}
			if (handler != null) {
				handler(this, args);
			}
		}

		public abstract bool Start();
		public abstract bool Step();
		public abstract bool Stop();
		public abstract void Dispose();
	}
}
