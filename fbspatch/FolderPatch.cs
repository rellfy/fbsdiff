using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Text;
using fbsdiff;

namespace fbspatch {

	class FolderPatch {

		private string patchFile;
		private string currentPath;
		private string bspatchPath;
		private IndexFile file;

		public FolderPatch(string bspatchName, string inputDirectory, string patchFile) {
			this.currentPath = Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly().Location);
			this.patchFile = Path.Combine(this.currentPath, patchFile);
			this.bspatchPath = Path.Combine(this.currentPath, bspatchName);

			if (!this.patchFile.EndsWith(".zip"))
				this.patchFile += ".zip";

			if (!Directory.Exists(inputDirectory))
				throw new Exception("Input folder does not exist");

			Patch(inputDirectory);
			Console.ReadKey();
		}

		private void Patch(string inputDirectory) {
			string inputPath = Path.Combine(this.currentPath, inputDirectory);
			
			if (Directory.Exists(Path.Combine(this.currentPath, "patch")))
				Directory.Delete(Path.Combine(this.currentPath, "patch"), true);

			ZipFile.ExtractToDirectory(this.patchFile, this.currentPath);

			string patchPath = Path.Combine(this.currentPath, "patch");
			IndexFile indexFile = new IndexFile(Path.Combine(patchPath, "patch.index"));

			foreach (IndexFile.IndexLine line in indexFile.IndexLines) {
				string inputFilePath = Path.Combine(inputPath, line.file);
				string patchFile = Path.Combine(patchPath, line.file);

				switch (line.command) {
					case IndexFile.IndexCommand.Update:
						Bspatch(inputFilePath, inputFilePath, patchFile + ".patch");
						break;
					case IndexFile.IndexCommand.Create:
						Directory.CreateDirectory(Path.GetDirectoryName(inputFilePath));

						File.Copy(Path.Combine(patchPath, "create", line.file), inputFilePath, true);
						break;
					case IndexFile.IndexCommand.Delete:
						if (!File.Exists(inputFilePath))
							break;

						string deleteDir = inputFilePath.Substring(0, inputFilePath.Length - Path.GetFileName(inputFilePath).Length);
						File.Delete(inputFilePath);

						if (Directory.GetFileSystemEntries(deleteDir).Length == 0)
							Directory.Delete(deleteDir, true);

						break;
				}
			}

			//File.Delete(this.patchFile);
			Directory.Delete(patchPath, true);
		}

		private string[] GetFolderFiles(string folder) {
			string[] files = Directory.GetFiles(folder, "*.*", SearchOption.AllDirectories);
			string[] filesRelative = new string[files.Length];

			for (int i = 0; i < files.Length; i++) {
				int folderLength = files[i].StartsWith(folder + "\\") ? folder.Length + 1 : folder.Length;
				string path = files[i].Substring(folderLength, files[i].Length - folderLength);
				filesRelative[i] = path;
			}

			return filesRelative;
		}

		private bool FindInArray(string[] array, string text) {
			for (int i = 0; i < array.Length; i++) {
				if (array[i] == text)
					return true;
			}

			return false;
		}

		private IndexFile.IndexLine[] GetIndexLines(string[] folder1Files, string[] folder2Files) {
			List<IndexFile.IndexLine> lines = new List<IndexFile.IndexLine>();

			// Find created/patched files
			for (int i = 0; i < folder2Files.Length; i++) {
				bool found = FindInArray(folder1Files, folder2Files[i]);

				if (!found) {
					lines.Add(new IndexFile.IndexLine(IndexFile.IndexCommand.Create, folder2Files[i]));
				} else {
					lines.Add(new IndexFile.IndexLine(IndexFile.IndexCommand.Update, folder2Files[i]));
				}
			}

			// Find deleted files
			for (int i = 0; i < folder1Files.Length; i++) {
				bool found = FindInArray(folder2Files, folder1Files[i]);

				if (!found)
					lines.Add(new IndexFile.IndexLine(IndexFile.IndexCommand.Delete, folder1Files[i]));
			}

			return lines.ToArray();
		}

		/// <summary>
		/// Runs bspatch.exe and patches individual files
		/// </summary>
		public void Bspatch(string oldPath, string newPath, string patchPath) {
			ProcessStartInfo startInfo = new ProcessStartInfo();

			Console.WriteLine($"Patching from {oldPath} to {newPath} with patchfile {patchPath}");

			startInfo.CreateNoWindow = false;
			startInfo.UseShellExecute = false;
			startInfo.WindowStyle = ProcessWindowStyle.Hidden;
			startInfo.FileName = this.bspatchPath;
			startInfo.Verb = "runas";
			startInfo.RedirectStandardOutput = true;
			startInfo.Arguments = $"\"{oldPath}\" \"{newPath}\" \"{patchPath}\"";

			using (Process exeProcess = Process.Start(startInfo)) {
				exeProcess.WaitForExit();
			}
		}
	}
}
