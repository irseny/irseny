using System;
using System.Collections.Generic;

namespace Irseny.Capture.Video {
	public class CaptureSettings {
		Dictionary<CaptureProperty, double> fProps = new Dictionary<CaptureProperty, double>(16);
		Dictionary<CaptureProperty, int> iProps = new Dictionary<CaptureProperty, int>(16);

		public CaptureSettings() {
		}

		public CaptureSettings(CaptureSettings source) {
			if (source == null) throw new ArgumentNullException("source");
			// leave everything at auto detect
			// iProps.Add(CaptureProperty.CameraId, 0);
		}
		public int GetInteger(CaptureProperty prop, int fallback) {
			int result;
			if (!iProps.TryGetValue(prop, out result)) {
				return fallback;
			}
			return result;
		}
		public void SetInteger(CaptureProperty prop, int value) {
			iProps[prop] = value;
		}
		public double GetDecimal(CaptureProperty prop, double fallback) {
			double result;
			if (!fProps.TryGetValue(prop, out result)) {
				return fallback;
			}
			return result;
		}
		public void SetDecimal(CaptureProperty prop, double value) {
			fProps[prop] = value;
		}
		public void SetAuto(CaptureProperty prop) {
			if (fProps.ContainsKey(prop)) {
				fProps.Remove(prop);
			}
			if (iProps.ContainsKey(prop)) {
				iProps.Remove(prop);
			}
		}
		public bool IsAuto(CaptureProperty prop) {
			return !fProps.ContainsKey(prop) && !iProps.ContainsKey(prop);
		}
	}
}
