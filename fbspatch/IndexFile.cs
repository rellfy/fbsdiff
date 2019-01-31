using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading.Tasks;

namespace fbsdiff {

	public class IndexFile {

		private IndexLine[] indexLines;

		public IndexLine[] IndexLines => this.indexLines;

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

		public IndexFile(string filePath) {
			try {
				ReadAllLines(filePath);
			} catch (Exception exception) {
				throw new Exception($"Could not read index file at {filePath}", exception);
			}
		}

		/// <summary>
		/// Read instructions on index file for patching
		/// </summary>
		/// <param name="filePath">Path for the index file</param>
		private void ReadAllLines(string filePath) {
			string[] rawLines = File.ReadAllLines(filePath);
			this.indexLines = new IndexLine[rawLines.Length];

			for (int i = 0; i < rawLines.Length; i++) {
				string[] args = rawLines[i].Split(' ');

				if (args.Length < 2)
					throw new Exception($"Line ${i + 1} is formatted incorrectly");

				string command = args[0];
				string filename = "";

				for (int j = 1; j < args.Length; j++) {
					filename += j < args.Length - 1 ? args[j] + " " : args[j];
				}

				this.indexLines[i] = new IndexLine(command, filename);
			}
		}
	}
}
