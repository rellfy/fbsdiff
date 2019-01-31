using System;
using System.IO;

namespace fbsdiff {

	class Program {

		static void Main(string[] args) {
			if (args.Length != 3) {
				Console.WriteLine("usage: fbsdiff oldFolder newFolder patchFile");
				return;
			}

			FolderDiff diff = new FolderDiff("bsdiff.exe", args[0], args[1], args[2]);
		}
	}
}
