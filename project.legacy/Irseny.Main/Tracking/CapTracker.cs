using System;
namespace Irseny.Tracking {
	public abstract class CapTracker : IPoseTracker {
		readonly object inputEventSync = new object();
		readonly object detectedEventSync = new object();
		readonly object executionEventSync = new object();
		event EventHandler<PositionDetectedEventArgs> positionDetected;
		event EventHandler inputAvailable;
		event EventHandler started;
		event EventHandler stopped;
		public abstract bool Running { get; }
		public event EventHandler<PositionDetectedEventArgs> PositionDetected {
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
		public event EventHandler Started {
			add {
				lock (executionEventSync) {
					started += value;
				}
			}
			remove {
				lock (executionEventSync) {
					started -= value;
				}
			}
		}
		public event EventHandler Stopped {
			add {
				lock (executionEventSync) {
					stopped += value;
				}
			}
			remove {
				lock (executionEventSync) {
					stopped -= value;
				}
			}
		}
		protected void OnStarted(EventArgs args) {
			if (args == null) throw new ArgumentNullException("args");
			EventHandler handler;
			lock (executionEventSync) {
				handler = started;
			}
			if (handler != null) {
				handler(this, args);
			}
		}
		protected void OnStopped(EventArgs args) {
			if (args == null) throw new ArgumentNullException("args");
			EventHandler handler;
			lock (executionEventSync) {
				handler = stopped;
			}
			if (handler != null) {
				handler(this, args);
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
		protected virtual void OnPositionDetected(PositionDetectedEventArgs args) {
			if (args == null) throw new ArgumentNullException("args");
			EventHandler<PositionDetectedEventArgs> handler;
			lock (detectedEventSync) {
				handler = positionDetected;
			}
			if (handler != null) {
				handler(this, args);
			}
		}
		public virtual void Dispose() {
			lock (detectedEventSync) {
				positionDetected = null;
			}
			lock (inputEventSync) {
				inputAvailable = null;
			}
			lock (executionEventSync) {
				started = null;
				stopped = null;
			}
		}
		public abstract bool Center();
		public abstract bool ApplySettings(TrackerSettings settings);
		public abstract bool Start(TrackerSettings settings);
		public abstract bool Step();
		public abstract bool Stop();

	}
}
