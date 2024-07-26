# SCP Image Format

The following is based off of the [SCP image spec][scp-spec] with some alterations and additions based on my experience with the format.

This file contains the flux transition timings of a floppy disk

## Header

| Name | Size (bytes) | Description |
| --- | --- | ---|
| Signature | 3 | The string "SCP" |
| VersionRevision | 1 | The high-order nibble contains the Version and the lower order nibble contains the revision |
| DiskType | 1 |  Indicates what kind of disk this is an image for |
| NumberOfRevolutionsPerTrack | 1 | How many times each track was read and stored | 
| StartTrack | 1 | The number of the first track |
| EndTrack | 1 | The number of the last track |
| Flags | 1 | Flags providing extra information about the file |
| BitCellWidth | 1 | The number of bits used to record timing information |
| HeadNumber | 1 | The number of heads read |
| CaptureResolution | 1 | The time multiplier for timing information |
| Checksum | 1 | A 32-bit wrap-around addition checksum for the rest of the file, not set for read-write images |

It looks like -1 (0xFF) is the default for unknown values but there's nothing about this in the spec

### Disk Type

The following Disk types are defined. The high-order nibble is used to encode a group (company or type) and then low order nibble provides
extra information

| Disk Type | Value |
| --- | :---: |
| disk_C64 | 0x00 |
| disk_Amiga | 0x04 |
| disk_AmigaHD | 0x08 |
| disk_AtariFMSS | 0x10 |
| disk_AtariFMDS | 0x11 |
| disk_AtariFMEx | 0x12 |
| disk_AtariSTSS | 0x14 |
| disk_AtariSTDS | 0x15 |
| disk_AppleII | 0x20 |
| disk_AppleIIPro | 0x21 |
| disk_Apple400K | 0x24 |
| disk_Apple800K | 0x25 |
| disk_Apple144 | 0x26 |
| disk_PC360K | 0x30 |
| disk_PC720K | 0x31 |
| disk_PC12M | 0x32 |
| disk_PC144M | 0x33 |
| disk_TRS80SSSD | 0x40 |
| disk_TRS80SSDD | 0x41 |
| disk_TRS80DSSD | 0x42 |
| disk_TRS80DSDD | 0x43 |
| disk_TI994A | 0x50 |
| disk_D20 | 0x60 |
| disk_CPC | 0x70 |
| disk_360 | 0x80 |
| disk_12M | 0x81 |
| disk_Rrsvd1 | 0x82 |
| disk_Rsrvd2 | 0x83 |
| disk_720 | 0x84 |
| disk_144M | 0x85 |
| tape_GCR1 | 0xE0 |
| tape_GCR2 | 0xE1 |
| tape_MFM | 0xE2 |
| drive_MFM | 0xF0 |
| drive_RLL | 0xF1 |

### Flags

These flags indicate information about the image or how it was saved

| Name | Value | Description |
| --- | :---: | --- |
| IndexQueued | 0x1 | 1 to indicate reader starting at the index pulse, 0 for a random location |
| Tpi96 | 0x2 | 1 to indicate 96 TPI (80 Tracks), 0 to indicate 48 TPI (40 Tracks) |
| Rpm360 | 0x4 | 1 to indicate 360 RPM (1.2M Disks), 0 to indicate 300 RPM (others) |
| FluxNormalized | 0x8 | 1 to indicate a normalization was applied that reduces quality but saves space, 0 for full resolution |
| ReadWrite | 0x10 | 1 to indicate the image is able to be read and written, 0 to indicate read-only |
| ExtensionFooter | 0x20 | 1 to indicate an extension footer is included, 0 otherwise |
| NonFloppyMedia | 0x40 | 1 to indicate the image is for some other type of media, 0 to indicate a floppy-disk image |
| CreatedByOtherDevice | 0x80 | 1 to indicate the image was created by another tool, 0 to indicate it was created by SuperCard Pro |

### BitCellWidth

This field indicates how many bits are used to encode the timing information for the tracks. The default is 0 which indicates that each time
sample is made up of 16 bits. Other values are used to indicate the number of bits that are used to store the sample. For example, an 8
would indicate that each sample contains 8 bits.

Note that this is using bit cell in a different way than the standard as this refers to timing blocks which could actually span several bitcells

### HeadNumber

This field indicates which heads were used when reading the disk

| Name | Value | Description |
| --- | :---: | --- |
| Both | 0x00 | Both sides of the disk were read |
| Side0 | 0x01 | Only side 0 (bottom) was read, even tracks only |
| Side1 | 0x02 | Only side 1 (top) was read, odd tracks only |

### CaptureResolution

This field indicates the amount of time that each value in the track timing information represents. The default is 0 which indicates that
the timing information represents 25 ns increments. Values other than 0 represent additional 25 ns increments. So 1 represents 25 ns + 25 ns x 1 = 50 ns,
2 represents 25 ns + 25 ns x 2 = 75ns etc.

## Track Header Offsets

After the header is a series of 32-bit integer values indicating the location of each tracks header in the file

The value is in little endian

The track offsets are always stored sequentially starting with 0 and going up to a maximum of 167

Valid entries are based on the StartTrack and EndTrack values in the header

Empty tracks have a value of 0x00000000

Even tracks are from side 0 and odd tracks are from side 1. If only one side is read then tracks from other side should all be null. 

## Tracks

The tracks start the location specified by each offset and start with a track header.

| Name | Size (bytes) | Description |
| --- | --- | ---|
| Signature | 3 | The string "TRK" |
| TRACK NUMBER | 1 | The number of the track |

This is followed by a series of revolution entries, the number of entries is based on the NumberOfRevolutionsPerTrack value in the file header

| Name | Size (bytes) | Description |
| --- | --- | ---|
| INDEX TIME | 4 | Timing information for a full revolution in 25ns increments |
| TRACK LENGTH | 4 | The number of time entries for the track |
| DATA OFFSET | 4 | The offset to the time entries for this track relative to the start of the track header |

The value is in little endian

At the specified Data Offset for each revolution will be a a series of values. The size of these values depends on the BitCellWidth value
in the header but is typically 16 bits.

These values record the number of time increments between flux transitions. A value of 0 indicates that the time overflowed and that the value
65536 should be added to the subsequent time entry. The time entries are in Big Endian.

The actual time of each entry is determined by the CaptureResolution value in the file header

Time between flux transitions in seconds = time entry x (CaptureResolution + 1) x 25 / 10<sup>-9</sup> s/ns

## Footer

If the ExtensionFooter flag is set then the end of the file will contain extra information

The last 48 bytes of the file contain offsets to strings and other data. These strings are placed between the end of the last track and the footer

| Name | Size (bytes) | Description |
| --- | --- | ---|
| OFFSET TO DRIVE MANUFACTURER STRING | 4 | The offset to the Drive Manufacturer string |
| OFFSET TO DRIVE MODEL STRING | 4 | The offset to the Drive Model string |
| OFFSET TO DRIVE SERIAL NUMBER STRING | 4 | The offset to Drive Serial Number string |
| OFFSET TO CREATOR STRING | 4 | The offset to the Creator string |
| OFFSET TO APPLICATION NAME STRING | 4 | The offset to the application name string |
| OFFSET TO COMMENTS | 4 | The offset to the comments string |
| UTC TIME/DATE OF IMAGE CREATION | 8 | The time the image was created in seconds since the UNIX epoch |
| UTC TIME/DATE OF IMAGE MODIFICATION | 8 | The time the image was modified in seconds since the UNIX epoch |
| VERSION/SUBVERSION OF APPLICATION THAT CREATED IMAGE | 1 | The high-order nibble contains the Version of the application and the lower order nibble contains the sub-version |
| VERSION/SUBVERSION OF SUPERCARD PRO HARDWARE | 1 | The high-order nibble contains the Version of the SCP Hardware and the lower order nibble contains the sub-version |
| VERSION/SUBVERSION OF SUPERCARD PRO FIRMWARE | 1 | The high-order nibble contains the Version of the SCP Firmware and the lower order nibble contains the sub-version |
| VERSION/SUBVERSION OF THIS IMAGE FORMAT | 1 | The high-order nibble contains the Version of the Image Format and the lower order nibble contains the sub-version |
| Signature | 4 | The string 'FPCS' |

[scp-spec]: https://www.cbmstuff.com/downloads/scp/scp_image_specs.txt