## Overview

This is a tool that I wrote to convert (Or rectify) floppy images to different formats. I wanted to image my collection of floppies and
I wasn't entirely satisfied with the tools that I was finding. They did the job but not in the exact way that I wanted them to.

Note that this tool is designed for use with PC floppy types as that is what I am currently working with

## How to use (WIP)

The tool takes one or more paths and a type identifier. 

```
FloppyImageRectifier.exe -scp <Path-to-SCP-File> -hfe <Path-to-HFE-File> -img <Path-to-IMG-File> -type <Disk-Type-Identifier [-output <Path-to-Output-File>]>
```

\<Path-to-SCP-File> - A path to an SCP file, will always be an input file if provided, may be an output file

\<Path-to-HFE-File> - A path to an HFE file, may be input or output depending on other options provided

\<Path-to-IMG-file> - A path to an IMG file, may be input or output depending on other options provided

\<Disk-Type-Identifier> - Defines the type of floppy disk

\<Path-to-Output-File> - Optional, if specified then some program output will also be written to the specified file

The operation performed depends on the arguments specified

| SCP Path | HFE Path | IMG Path | Operation | Type Required |
|:--------:|:--------:|:--------:| --------- |:-------------:|
|    x     |          |          | Displays Information about the SCP file |      No       |
|          |    x     |          | Displays Information about the HFE file |      No       |
|          |          |     x    | Displays Information about the IMG file |      No       |
|    x     |    x     |     x    | Update Disk type in SCP file, convert SCP to HFE and IMG |      Yes      |
|    x     |    x     |          | Update Disk type in SCP file, convert SCP to HFE |      Yes      |
|    x     |          |     x    | Update Disk type in SCP file, convert SCP to IMG |      Yes      |
|          |    x     |     x    | Convert HFE to IMG (May fail, see below) |      Yes      |


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

The Disk Type and the Flags are updated because reading raw images with the Greaseweazle doesn't seem to have an option to set them.
It's possible that I am just missing it but since I am writing this application anyways, might as well update them here.

It's possible that they are set correctly if you path a format to the Greaseweazle program when reading a disk but I don't want to do that
because it seems to limit the raw information it gets from the disk.

An error occurs if the file doesn't exist

#### SCP header updates

The Disk Type and Flag fields of the SCP header are updated based on the value of the Disk Type Identifier argument

| Identifier | SCP Disk Type | SCP TPI 96 Flag (0x2) | SCP RPM 360 Flag (0x4) |
| ---------- | ------------- | --------------------- | ---------------------- |
| PC_MFM_525_360  | disk_PC360K (0x30) | Clear | Clear |
| PC_MFM_525_1200 | disk_PC12M (0x32) | Set | Set |
| PC_MFM_35_720   | disk_PC720K (0x31) | Set | Clear |
| PC_MFM_35_1440 | disk_PC144M (0x33) | Set | Clear |
| PC_MFM_35_1680 | disk_PC144M (0x33) | Set | Clear |

## HFE File processing

HFE Files contain a header and then a set of tracks containing the raw binary data from the disk. This includes the data, clock bits, checksums, identifier blocks, and gaps.

### Display Information

Providing only the -hfe argument will cause the program to decode and display the contents of the HFE header along with the track values
(This can be piped to a file to be reviewed later)

An error occurs if the file doesn't exist

### Converting from SCP

If both the -scp and -hfe arguments are provided then the program will convert the SCP file to an HFE file. This is done by
taking the flux transition timing information from the SCP file and converting it to binary data

See [MFM page][docs-mfm] for details (To Do)

HFE files have two problems, in my opinion

First, track length is specified in number of bytes. This is a problem because the number of bits read from a disk isn't guaranteed
to be an even number of bytes in length. This means that some portion of the last byte might be garbage and the software reading the
file has no idea what portion that is.

Secondly, the two sides of a track are interwoven. This means that both sides of a track must have the same length. I don't know how much
of a problem this is but it makes writing the file more confusing. As far as I know, disk are read one side at a time so I don't even see
why this would be a benefit

To handle these issue when converting to an HFE image the program first decodes the contents of the disk and then uses that to ensure that
the end of the track doesn't occur in the middle of a sector. This is done by rotating the track before writing it to the file.

The user is asked if they want to replace the file if it already exists.

## IMG File processing

IMG or IMA files contain only the actual data contents of a disk. 

### Display Information

Providing only the -img argument will cause the program to display the contents of the img file spit into 512 byte sectors along
with the ASCII representation of the bytes of the file

An error occurs if the file doesn't exist

### Converting from SCP

If both the -scp and -img arguments are provided then the program will convert the SCP file to an IMG file. This is done by
taking the flux transition timing information from the SCP file and converting it to binary data and then decoding it to find the data
portion

See [MFM page][docs-mfm] for details (To Do)

Note that the IMG file is always converted from the SCP file even if the -hfe argument is given. This is because the contents of the
disk are used to determine if the HFE track needs to be rotated. So the information needed to generate the IMG file is already available
before the HFE file is generated.

### Converting from HFE

If only the -hfe and -img arguments are provided then the program will try to convert the HFE file to an IMG file. This is done by reading
the binary data from the HFE file and decoding the it to find the data portion.

See [MFM page][docs-mfm] for details

Note that this may fail for the reasons described above. If HFE track ends in the middle of a sector then the program will try to continue
from the start of the track but due to the discontinuity the sector may be invalid

## Acknowledgments

This tool is based on the information in the various flexible disk cartridge ECMA standards

* [ECMA-70] - Withdrawn standard for 5.25" 40 Track MFM (360K) disks
* [ECMA-99] - Standard for 5.25" 80 Track MFM (1.2M) disks
* [ECMA-100] - Standard for 3.5" 80 Track MFM (720k) disks
* [ECMA-125] - Standard for 3.5" 80 track MFM (1440k) disks

The specification for the various file formats

* [SCP][scp-spec]
* [HFE][hfe-spec]

and the following repositories

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