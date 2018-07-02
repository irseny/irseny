using System;
using System.Threading;
using System.Collections.Generic;

namespace Irseny.Tracap {
	public abstract class HeadDetector : IHeadDetector {

		protected readonly object outputEventSync = new object();
		protected readonly object inputSync = new object();
		readonly object runSync = new object();
		ManualResetEvent stepSignal = new ManualResetEvent(false);
		volatile bool running = false;
		Thread thread = null;
		Queue<Util.SharedRef<Emgu.CV.Mat>> pendingInput = new Queue<Util.SharedRef<Emgu.CV.Mat>>();
		protected event EventHandler<ImageEventArgs> inputProcessed;
		protected event EventHandler<EventArgs> positionDetected;

		public HeadDetector() {
		}
		public event EventHandler<ImageEventArgs> InputProcessed {
			add {
				lock (outputEventSync) {
					inputProcessed += value;
				}
			}
			remove {
				lock (outputEventSync) {
					inputProcessed -= value;
				}
			}
		}
		public event EventHandler<EventArgs> PositionDetected {
			add {
				lock (outputEventSync) {
					positionDetected += value;
				}
			}
			remove {
				lock (outputEventSync) {
					positionDetected -= value;
				}
			}
		}
		protected void OnInputProcessed(ImageEventArgs args) {
			if (args == null) throw new ArgumentNullException("args");
			EventHandler<ImageEventArgs> handler;
			lock (outputEventSync) {
				handler = inputProcessed;
			}
			if (handler != null) {
				handler(this, args);
			}
		}
		protected void OnPositionDetected(EventArgs args) {
			if (args == null) throw new ArgumentNullException("args");
			EventHandler<EventArgs> handler;
			lock (outputEventSync) {
				handler = positionDetected;
			}
			if (handler != null) {
				handler(this, args);
			}
		}
		protected abstract void Step(Emgu.CV.Mat image);

		private void Run() {
			running = true;
			while (running) {
				stepSignal.WaitOne();
				stepSignal.Reset();
				ProcessPending();
			}
		}
		private void ProcessPending() {
			Queue<Util.SharedRef<Emgu.CV.Mat>> input;
			lock (inputSync) {
				input = pendingInput;
				pendingInput = new Queue<Util.SharedRef<Emgu.CV.Mat>>();
			}
			foreach (var image in input) {
				Step(image.Reference);
				image.Dispose();
			}
		}
		public virtual void Dispose() {
			lock (inputSync) {
				foreach (var image in pendingInput) {
					image.Dispose();
				}
				pendingInput.Clear();
			}
		}

		public bool Start() {
			lock (runSync) {
				if (!running) {
					running = true;
					thread = new Thread(Run);
					thread.Start();
					return true;
				}
			}
			return false;
		}
		public bool Stop() {
			lock (runSync) {
				if (running) {
					running = false;
					stepSignal.Set();
					thread.Join(); // TODO: move processing to detection system thread
					thread = null;
					return true;
				}
			}
			return false;
		}
		public void QueueInput(Util.SharedRef<Emgu.CV.Mat> image) {
			if (image == null) throw new ArgumentNullException("image");
			lock (inputSync) {
				pendingInput.Enqueue(Util.SharedRef.Copy(image));
			}
			stepSignal.Set();
		}
	}


}

