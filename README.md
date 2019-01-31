# fbsdiff
Dynamically generate and apply patch files to folders using `bsdiff` and `bspatch` (http://www.daemonology.net/bsdiff/). Useful when compressing & decompressing files to apply a patch is not a realistic option.

## Generating patches with fbsdiff.exe

To generate a patch, use the following command strcture:

```
fbsdiff oldFolder newFolder patchFile
```

The generated file will be a .zip that follows the following structure:

```
=> create/
  ======> file_to_be_created.txt
  ======> another_new_file.png
=> patch.index
=> file_to_be_patched.mp4.patch
=> another_existing_file.txt.patch
```

## Applying patches with fbspatch.exe

To apply a patch, use the following command structure:

```
fbspatch inputFolder patchFile
```

The `patch.index` file generated with the patch files contains instructions to every changed file. The instruction might be *update*, *create* or *delete*. Currently, renamed files are not recognized (they will be deleted and then created again with the new name). This will be added in the next version by generating a hash for the file and checking its integrity.
