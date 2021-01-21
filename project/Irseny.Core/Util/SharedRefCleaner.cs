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
using System.Diagnostics;
using System.Collections.Generic;

namespace Irseny.Core.Util {
	public class SharedRefCleaner {
		List<ISharedRef> usedRefs;
		int stepIndex;
		int referenceLimit;
		/// <summary>
		/// Creates an empty instance of this class.
		/// </summary>
		public SharedRefCleaner(int referenceLimit) {
			if (referenceLimit < 0) throw new ArgumentOutOfRangeException("referenceLimit");
			this.usedRefs = new List<ISharedRef>(referenceLimit);
			this.stepIndex = 0;
			this.referenceLimit = referenceLimit;
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
					if (usedRefs.Count > referenceLimit) { // occasional warning when cleanup operation finished
						Debug.WriteLine(this.GetType().Name + ": Many captured images still in use: " + usedRefs.Count);
					}
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
			int lastIndex = usedRefs.Count - 1;
			if (lastIndex < usedRefs.Count) {
				usedRefs[index] = usedRefs[lastIndex];
			}
			usedRefs.RemoveAt(lastIndex);

		}

	}
}
