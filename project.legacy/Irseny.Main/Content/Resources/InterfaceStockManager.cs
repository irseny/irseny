using System;
using System.IO;

namespace Irseny.Content.Resources {
	public class InterfaceStockManager : ContentManager<Mycena.IInterfaceStock> {
		public InterfaceStockManager() {
		}
		public override void Load(ContentManagerSettings settings) {
			{
				var config = new Mycena.StockAccessConfig();
				var stock = new Mycena.InterfaceStock(config);
				string[] fileNames = new string[] {
					"ArrowDirectionH.png",
					"ArrowDirectionV.png",
					"ArrowRotationBottom.png",
					"ArrowRotationLeft.png",
					"ArrowRotationRight.png",
					"ArrowRotationTop.png",
					"CapSide.png",
					"CapTop.png",
					"CapFront.png",
					"VideoStop.png",
					"Icon.png"
				};
				for (int i = 0; i < fileNames.Length; i++) {
					stock.RegisterPixbuf(Path.Combine(settings.ResourceDirectory, fileNames[i]));
				}
				SetEntry("Main", stock);
			}
			Loaded = true;
		}
		public override void Reload() {

		}
		public override void Unload() {
			GetEntry("Main").Dispose();
			ClearEntries();
		}
	}
}
