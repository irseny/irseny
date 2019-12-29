using System;
namespace Mycena {
	public class StockAccessConfig {
		public StockAccessConfig() {
			this.CutDirectoryPath = true;
			this.CutFileExtension = true;
		}
		public StockAccessConfig(StockAccessConfig source) {
			if (source == null) throw new ArgumentNullException("source");
			this.CutDirectoryPath = source.CutDirectoryPath;
			this.CutFileExtension = source.CutFileExtension;
		}
		public bool CutDirectoryPath { get; set; }
		public bool CutFileExtension { get; set; }
	}
}
