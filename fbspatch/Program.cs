using System;
using System.IO;
using fbspatch;

namespace fbsdiff {

	class Program {

		static void Main(string[] args) {
			if (args.Length != 2) {
				Console.WriteLine("usage: fbspatch inputFolder patchFile");
				return;
			}

			FolderPatch patch = new FolderPatch("bspatch.exe", args[0], args[1]);
		}
	}
}