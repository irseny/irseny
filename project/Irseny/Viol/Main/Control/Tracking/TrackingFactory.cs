using System;

namespace Irseny.Viol.Main.Control.Tracking {
	public class TrackingFactory : InterfaceFactory {
		readonly int index;
		public TrackingFactory(int index) : base() {
			this.index = index;
		}
		protected override bool CreateInternal() {
			var factory = Mycena.InterfaceFactory.CreateFromFile(Content.ContentMaster.Instance.Resources.InterfaceDefinitions.GetEntry("TrackingControl"));
			Container = factory.CreateWidget("box_Root");
			return true;
		}
		protected override bool ConnectInternal() {
			var btnTrack = Container.GetWidget<Gtk.ToggleButton>("btn_Track");
			btnTrack.Clicked += delegate {
				if (btnTrack.Active) {
					StartTracking();
				} else {
					StopTracking();
				}
			};
			return true;
		}
		protected override bool DisconnectInternal() {
			StopTracking();
			return true;
		}
		protected override bool DestroyInternal() {
			Container.Dispose();
			return true;
		}
		private bool StartTracking() {
			int trackerId = Listing.EquipmentMaster.Instance.HeadTracker.GetEquipment(index, -1);
			if (trackerId < 0) {
				var tracker = new Tracap.Basic3PointCapTracker();
				trackerId = Tracap.DetectionSystem.Instance.StartDetector(tracker);
				Listing.EquipmentMaster.Instance.HeadTracker.Update(index, Listing.EquipmentState.Active, trackerId);
				Irseny.Log.LogManager.Instance.Log(Irseny.Log.LogMessage.CreateMessage(this, "Head tracker {0} started", index));
				// TODO: connect to video capture
				return true;
			} else {
				Irseny.Log.LogManager.Instance.Log(Irseny.Log.LogMessage.CreateWarning(this, "Unable to start head tracker {0}: Already running", index));
				return false;
			}
		}
		private bool StopTracking() {
			int trackerId = Listing.EquipmentMaster.Instance.HeadTracker.GetEquipment(index, -1);
			if (trackerId > -1) {
				Listing.EquipmentMaster.Instance.HeadTracker.Update(index, Listing.EquipmentState.Passive, -1);
				Tracap.DetectionSystem.Instance.StopDetector(trackerId); // auto disconnect from video capture in dispose
				Irseny.Log.LogManager.Instance.Log(Irseny.Log.LogMessage.CreateMessage(this, "Head tracker {0} stopped", index));
				return true;
			} else {
				Irseny.Log.LogManager.Instance.Log(Irseny.Log.LogMessage.CreateWarning(this, "Unable to stop head tracker {0}: Not running", index));
				return false;
			}
		}
	}
}
