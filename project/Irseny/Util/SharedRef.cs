using System;
using System.Diagnostics;

namespace Irseny.Util {
	public class SharedRef<T> : ISharedRef<T> {
		InternalSharedRef reference;

		public SharedRef(SharedRef<T> source) {
			if (source == null) throw new ArgumentNullException("source");
			this.reference = source.reference;
			this.reference.Retain();
		}
		public SharedRef(T reference) {
			this.reference = new InternalSharedRef(reference); // refcount is 1
		}
		~SharedRef() {
			if (!Disposed) {
				Debug.WriteLine(this.GetType().Name + ": Collecting undisposed reference.");
				Dispose();
			}
		}
		public bool Disposed {
			get { return reference == null; }
		}
		public T Reference {
			get {
				if (Disposed) {
					Debug.WriteLine(this.GetType().Name + ": Using disposed reference.");
					return default(T);
				} else {
					return reference.Reference;
				}
			}
		}
		public bool LastReference {
			get {
				if (Disposed) {
					Debug.WriteLine(this.GetType().Name + ": Using disposed reference.");
					return true;
				} else {
					return reference.ReferenceCount <= 1;
				}
			}
		}
		public void Reset(T reference) {
			if (!Disposed) {
				Dispose();
			}
			this.reference = new InternalSharedRef(reference);
		}

		public void Dispose() {
			if (!Disposed) {
				reference.Release();
				reference = null;
			}
		}

		private class InternalSharedRef : IDisposable {
			object counterSync = new object();
			object referenceSync = new object();
			int referenceCount = 1;
			T reference;
			bool disposed = false;

			public InternalSharedRef(T obj) {
				this.reference = obj;
			}
			public T Reference {
				get {
					lock (referenceSync) {
						if (disposed) {
							Debug.WriteLine(this.GetType().Name + ": Using disposed reference.");
						}
						return reference;
					}
				}
			}
			public int ReferenceCount {
				get {
					lock (counterSync) {
						return referenceCount;
					}
				}
			}
			public void Retain() {
				lock (counterSync) {
					if (disposed) {
						Debug.WriteLine(this.GetType().Name + ": Retaining a disposed reference.");
					}
					referenceCount += 1;
				}
			}
			public void Release() {
				lock (counterSync) {
					if (disposed) {
						Debug.WriteLine(this.GetType().Name + ": Releasing an already disposed reference.");
					}
					referenceCount -= 1;
				}
				if (referenceCount <= 0) {
					if (referenceCount < 0) {
						Debug.WriteLine(this.GetType().Name + ": Reference count dropped below 0.");
					}
					Dispose();
				}
			}
			public void Dispose() {
				lock (referenceSync) {
					if (disposed) {
						Debug.WriteLine(this.GetType().Name + ": Disposing an already disposed reference.");
					}
					if (reference is IDisposable) {
						((IDisposable)reference).Dispose();
					}
					reference = default(T);
					disposed = true;
				}
			}
		}
	}
	public static class SharedRef {
		public static SharedRef<T> Create<T>(T obj) {
			return new SharedRef<T>(obj);
		}
		public static SharedRef<T> Copy<T>(SharedRef<T> reference) {
			return new SharedRef<T>(reference);
		}
	}
}

