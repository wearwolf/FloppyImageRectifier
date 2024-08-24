## Overview

This is a tool that I wrote to convert (Or rectify) floppy images to different formats. I wanted to image my collection of floppies and
I wasn't entirely satisfied with the tools that I was finding. They did the job but not in the exact way that I wanted them to.

Note that this tool is designed for use with PC floppy types as that is what I am currently working with

## How to use (WIP)

The tool takes one or more paths and a type identifier. 

```
FloppyImageRectifier.exe -scp <Path-to-SCP-File> -hfe <Path-to-HFE-File> -img <Path-to-IMG-File> -type <Disk-Type-Identifier [-output <Path-to-Output-File>] [-rotationFixups]
```

\<Path-to-SCP-File> - A path to an SCP file, will always be an input file if provided, may be an output file

\<Path-to-HFE-File> - A path to an HFE file, may be input or output depending on other options provided

\<Path-to-IMG-file> - A path to an IMG file, may be input or output depending on other options provided

\<Disk-Type-Identifier> - Defines the type of floppy disk

\<Path-to-Output-File> - Optional, if specified then some program output will also be written to the specified file

-rotationFixups - Indicates if rotational fix-ups should be applied when converting the track (Only relevant when converting scp files to hfe files)

The operation performed depends on the arguments specified

| SCP Path | HFE Path | IMG Path | Operation | Type Required |
|:--------:|:--------:|:--------:| --------- |:-------------:|
|    x     |          |          | Displays Information about the SCP file |      No       |
|          |    x     |          | Displays Information about the HFE file |      No       |
|          |          |     x    | Displays Information about the IMG file |      No       |
|    x     |    x     |     x    | Update SCP file, convert SCP to HFE and IMG |      Yes      |
|    x     |    x     |          | Update SCP file, convert SCP to HFE |      Yes      |
|    x     |          |     x    | Update SCP file, convert SCP to IMG |      Yes      |
|          |    x     |     x    | Convert HFE to IMG |      Yes      |


### Disk Type Identifiers

| Identifier | Disk Type | Sectors | Tracks | Encoding | Total Size |
| ---------- | --------------------------------- | ------ | -- | --- | ----------- |
| PC_MFM_525_360  | 5.25" Double-sided Double Density | 8 or 9 | 40 | MFM | 320k or 360 | 
| PC_MFM_525_1200 | 5.25" Double-sided High Density | 15 | 80 | MFM | 1200k |
| PC_MFM_35_720   | 3.5" Double-sided Double Density | 9 | 80 | MFM | 720k |
| PC_MFM_35_1440 | 3.5" Double-sided High Density | 18 | 80 | MFM | 1440k |
| PC_MFM_35_1680 | 3.5" Distribution Media Format | 21 | 80 | MFM | 1680k |

## SCP File processing

SCP files contain a header and then a set of tracks containing timing information. The time recorded is the amount
of time between flux transitions. A flux transition is when the polarity of the magnetic substance changes.

Floppy disks encode a 1 as a flux transition and a 0 as a lack of a flux transition. The number of 0s encoded by
a given amount of time is based on the disk and encoding.

### Display Information

Providing only the -scp argument will cause the program to decode and display the contents of the SCP header along with the track values
(This can be piped to a file to be reviewed later)

An error occurs if the file doesn't exist

### Converting

If the -scp argument is provided along with any of the other file arguments, the program will updated the SCP file to ensure that the 
Disk Type and Flags are set correctly. This update will be done in place and no other parts of the file will be touched.

An error occurs if the file doesn't exist

The -hfe and/or -img argument(s) specify which file types the scp file should be converted to and where to save them

The user will be asked if they want to overwrite the files if they already exist

## HFE File processing

HFE Files contain a header and then a set of tracks containing the raw binary data from the disk. This includes the data, clock bits, checksums, identifier blocks, and gaps.

### Display Information

Providing only the -hfe argument will cause the program to decode and display the contents of the HFE header along with the track values
(This can be piped to a file to be reviewed later)

An error occurs if the file doesn't exist

### Converting

The -img argument specifies where to save the img file

The user will be asked if they want to overwrite the file if it already exist

## IMG File processing

IMG or IMA files contain only the actual data contents of a disk. 

### Display Information

Providing only the -img argument will cause the program to display the contents of the img file spit into 512 byte sectors along
with the ASCII representation of the bytes of the file

An error occurs if the file doesn't exist

## Acknowledgements

This tool is based on the information found in the various flexible disk cartridge ECMA standards

* [ECMA-70] - Withdrawn standard for 5.25" 40 Track MFM (360K) disks
* [ECMA-99] - Standard for 5.25" 80 Track MFM (1.2M) disks
* [ECMA-100] - Standard for 3.5" 80 Track MFM (720k) disks
* [ECMA-125] - Standard for 3.5" 80 track MFM (1440k) disks

The specification for the various file formats

* [SCP][scp-spec]
* [HFE][hfe-spec]

and the following repositories for other floppy conversion tools

* [greaseweazle][github-gw]
* [HxCFloppyEmulator][github-hxc]

[docs-mfm]: Docs/MFM.md
[ECMA-70]: https://ecma-international.org/publications-and-standards/standards/ecma-70/
[ECMA-99]: https://ecma-international.org/publications-and-standards/standards/ecma-99/
[ECMA-100]: https://ecma-international.org/publications-and-standards/standards/ecma-100/
[ECMA-125]: https://ecma-international.org/publications-and-standards/standards/ecma-125/
[scp-spec]: https://www.cbmstuff.com/downloads/scp/scp_image_specs.txt
[hfe-spec]: https://hxc2001.com/download/floppy_drive_emulator/SDCard_HxC_Floppy_Emulator_HFE_file_format.pdf
[github-gw]: https://github.com/keirf/greaseweazle
[github-hxc]: https://github.com/jfdelnero/HxCFloppyEmulator