using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading.Tasks;

namespace fbsdiff {

	class IndexFile {

		public enum IndexCommand {
			Update,
			Create,
			Delete
		}

		public struct IndexLine {
			public IndexCommand command;
			public string file;

			public IndexLine(string command, string file) {
				this.command = (IndexCommand)Enum.Parse(typeof(IndexCommand), command, true);
				this.file = file;
			}

			public IndexLine(IndexCommand command, string file) {
				this.command = command;
				this.file = file;
			}

			public new string ToString() {
				return $"{this.command.ToString().ToLower()} {this.file}";
			}
		}

		public static void Write(string filePath, IndexLine[] lines) {
			string data = "";

			for (int i = 0; i < lines.Length; i++) {
				data += lines[i].ToString();

				if (i < lines.Length - 1)
					data += "\n";
			}

			File.WriteAllText(filePath, data);
		}
	}
}
