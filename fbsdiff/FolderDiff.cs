using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;

namespace fbsdiff {

	class FolderDiff {

		private string patchDirectory;
		private string currentPath;
		private string bsdiffPath;
		private IndexFile file;

		public FolderDiff(string bsdiffName, string oldFolder, string newFolder, string patchDirectory) {
			this.currentPath = Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly().Location);
			this.patchDirectory = Path.Combine(this.currentPath, patchDirectory);
			this.bsdiffPath = Path.Combine(this.currentPath, bsdiffName);

			Directory.CreateDirectory(this.patchDirectory);

			Diff(oldFolder, newFolder);
		}

		private void Diff(string oldFolder, string newFolder) {
			string[] oldFiles = GetFolderFiles(oldFolder);
			string[] newFiles = GetFolderFiles(newFolder);

			IndexFile.IndexLine[] lines = GetIndexLines(oldFiles, newFiles);
			IndexFile.Write(Path.Combine(this.patchDirectory, "patch.index"), lines);

			HandleDiff(oldFolder, newFolder, lines);
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

		private IndexFile.IndexLine[] GetIndexLines (string[] folder1Files, string[] folder2Files) {
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

		public void HandleDiff(string oldFolder, string newFolder, IndexFile.IndexLine[] lines) {
			for (int i = 0; i < lines.Length; i++) {
				string[] pathParts = lines[i].file.Replace('/', '\\').Split('\\');
				string directory = "";

				for (int j = 0; j < pathParts.Length - 1; j++) {
					directory += pathParts[j] + "\\";
				}

				string directoryPath = Path.Combine(this.patchDirectory, directory);
				string filePath = Path.Combine(this.currentPath, lines[i].file);

				string oldFile = Path.Combine(oldFolder, lines[i].file);
				string newFile = Path.Combine(newFolder, lines[i].file);

				switch (lines[i].command) {
					case IndexFile.IndexCommand.Update:
						if (directory != "" && !Directory.Exists(directoryPath))
							Directory.CreateDirectory(directoryPath);

						Bsdiff(oldFile, newFile, Path.Combine(this.patchDirectory, Path.GetFileName(filePath) + ".patch"));
						break;
					case IndexFile.IndexCommand.Create:
						string createPath = Path.Combine(this.patchDirectory, "create", directory);

						if (directory != "" && !Directory.Exists(createPath))
							Directory.CreateDirectory(createPath);

						File.Copy(newFile, Path.Combine(createPath, Path.GetFileName(newFile)), true);
						break;
					case IndexFile.IndexCommand.Delete:
						break;
				}
			}

			string zipPath = Path.Combine(this.currentPath, "patch.zip");

			if (File.Exists(zipPath))
				File.Delete(zipPath);
			
			ZipFile.CreateFromDirectory(this.patchDirectory, zipPath, CompressionLevel.Optimal, true);
			Directory.Delete(this.patchDirectory, true);
		}

		/// <summary>
		/// Runs bsdiff.exe and generated patch file
		/// </summary>
		/// <param name="filePath"></param>
		/// <param name="patchPath"></param>
		public void Bsdiff(string oldPath, string newPath, string patchPath) {
			ProcessStartInfo startInfo = new ProcessStartInfo();

			startInfo.CreateNoWindow = false;
			startInfo.UseShellExecute = false;
			startInfo.WindowStyle = ProcessWindowStyle.Hidden;
			startInfo.FileName = this.bsdiffPath;
			startInfo.Arguments = $"\"{oldPath}\" \"{newPath}\" \"{patchPath}\"";

			using (Process exeProcess = Process.Start(startInfo)) {
				exeProcess.WaitForExit();
			}
		}
	}
}
