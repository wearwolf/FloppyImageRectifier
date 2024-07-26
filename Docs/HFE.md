# SCP Image Format

The following is based off of the [HFE file format spec][hfe-spec] with some alterations and additions based on my experience with the format.

This file contains the decoded binary information of a floppy disk

## Header

| Name | Size (bytes) | Description |
| --- | --- | ---|
| Header Signature | 8 | The string "HXCPICFE" |
| formatrevision | 1 | The revision of the format, 0 is expected |
| number_of_track | 1 | The number of tracks in the file |
| number_of_side | 1 | The number of valid sides in the file | 
| track_encoding | 1 | Indicates how tracks are encoded |
| bitRate | 2 | Bitrate of the image in Kbit/s |
| floppyRPM | 2 |Rotation speed in rotations per minute |
| floppyinterfacemode | 1 | The type of floppy disk image |
| dnu | 1 | Reserved |
| track_list_offset | 2 | File offset to the track offset look-up table in 512 byte blocks |
| single_step | 1 | Indicates if this is a single step (0xFF) or double step (0x00) image |
| track0s0_altencoding | 1 | Indicates if an alternative track encoding is used for track 0 side 0 |
| track0s0_encoding | 1 | The alternative track encoding for track 0 side 0 if used, 0xFF if unused |
| track0s1_altencoding | 1 | Indicates if an alternative track encoding is used for track 0 side 1 |
| track0s1_encoding | 1 | The alternative track encoding for track 0 side 1 if used, 0xFF if unused |

Unused bytes in the first 512 byte block should be set to 0xFF

It looks like 0xFF is used for false values and 0x00 for true

Note that single_step seems to do this backwards, this is likely done so that single-step (0xFF) is the default value

multi-byte values are in little endian format

### Track Encoding

The following encoding types are used.

| Encoding | Value |
| --- | :---: |
| ISOIBM_MFM_ENCODING | 0x00 |
| AMIGA_MFM_ENCODING | 0x01 |
| ISOIBM_FM_ENCODING | 0x02 |
| EMU_FM_ENCODING | 0x03 |
| UNKNOWN_ENCODING | 0xFF |

### Floppy Interface Type

The following floppy interface types are used.

| Disk Type | Value |
| --- | :---: |
| IBMPC_DD_FLOPPYMODE | 0x00 |
| IBMPC_HD_FLOPPYMODE | 0x01 |
| ATARIST_DD_FLOPPYMODE | 0x02 |
| ATARIST_HD_FLOPPYMODE | 0x03 |
| AMIGA_DD_FLOPPYMODE | 0x04 |
| AMIGA_HD_FLOPPYMODE | 0x05 |
| CPC_DD_FLOPPYMODE | 0x06 |
| GENERIC_SHUGGART_DD_FLOPPYMODE | 0x07 |
| IBMPC_ED_FLOPPYMODE | 0x08 |
| MSX2_DD_FLOPPYMODE | 0x09 |
| C64_DD_FLOPPYMODE | 0x0A |
| EMU_SHUGART_FLOPPYMODE | 0x0B |
| S950_DD_FLOPPYMODE | 0x0C |
| S950_HD_FLOPPYMODE | 0x0D |
| DISABLE_FLOPPYMODE | 0xFE |

## Track Offsets

Starting at the 512 byte block specified by the track_list_offset header value is a series of entries indicating the location and length
of track data

| Name | Size (bytes) | Description |
| --- | --- | ---|
| offset | 2 | File offset to the start of track data in 512 byte blocks |
| track_len | 2 | The total number of bytes of track data |

multi-byte values are in little endian format

## Track Data

Track data starts at the offset specified by the track offset structure

The data is broken up into 512 byte blocks. The first half of the block contains data from side 0 and the second half contains data for
side 1. If there is less than 512 bytes of data in a block then the beginning of the first half will be used to store the remaining data for
side 0 and the beginning of the second half will be used to store the remaining data for side 1

Note that the bytes are stored backwards. The least significant bit should be read first, as if it was the most significant bit.

[hfe-spec]: https://hxc2001.com/download/floppy_drive_emulator/SDCard_HxC_Floppy_Emulator_HFE_file_format.pdf