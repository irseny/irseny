using System;
using System.Collections.Generic;
namespace Irseny.Util {
	public class SharedRefCleaner {
		List<ISharedRef> usedRefs;
		int stepIndex;
		/// <summary>
		/// Creates an empty instance of this class.
		/// </summary>
		public SharedRefCleaner() {
			usedRefs = new List<ISharedRef>(32);
			stepIndex = 0;
		}
		/// <summary>
		/// Adds a reference to be disposed by this instance. Disposes the reference if it has no customers.
		/// </summary>
		/// <param name="reference">Reference to be disposed.</param>
		public void AddReference(ISharedRef reference) {
			if (reference == null) throw new ArgumentNullException("reference");
			if (reference.LastReference) {
				reference.Dispose();
			} else {
				usedRefs.Add(reference);
			}
		}
		/// <summary>
		/// Performs disposing cleanup steps on the previously added references.
		/// </summary>
		/// <param name="iterations">Number of elements to process.</param>
		public void CleanUpStep(int iterations = 1) {
			if (usedRefs.Count == 0) {
				return;
			}
			while (iterations-- > 0) {
				if (stepIndex >= usedRefs.Count) {
					stepIndex = 0;
				}
				if (stepIndex < usedRefs.Count) {
					if (usedRefs[stepIndex].LastReference) {
						OverwriteClean(stepIndex);
					}
					stepIndex += 1;
				}
			}
		}
		/// <summary>
		/// Disposes all previously added references that have no more customers.
		/// </summary>
		public void CleanUpAll() {
			for (int i = 0; i < usedRefs.Count; i++) {
				if (usedRefs[i].LastReference) {
					OverwriteClean(i);
				}
			}
		}
		/// <summary>
		/// Forces the disposal of all previously added references.
		/// </summary>
		public void DisposeAll() {
			for (int i = 0; i < usedRefs.Count; i++) {
				usedRefs[i].Dispose();
			}
			usedRefs.Clear();
			stepIndex = 0;
		}
		/// <summary>
		/// Disposes the reference at the given index and shortens the reference collection.
		/// </summary>
		/// <param name="index">Index.</param>
		private void OverwriteClean(int index) {
			usedRefs[index].Dispose();
			int lastIndex = usedRefs.Count;
			if (lastIndex < usedRefs.Count) {
				usedRefs[index] = usedRefs[lastIndex];
			}
			usedRefs.RemoveAt(lastIndex);

		}

	}
}
