# Encoding

## SCP file updates

The SCP file is updated so that the Disk Type and flags in the header match the specified disk type. The file is only updated if the
stored values don't match what they should be based on the disk type

### SCP header updates

The Disk Type and Flag fields of the SCP header are updated based on the value of the Disk Type Identifier argument

| Identifier | SCP Disk Type | SCP TPI 96 Flag (0x2) | SCP RPM 360 Flag (0x4) |
| ---------- | ------------- | --------------------- | ---------------------- |
| PC_MFM_525_360  | disk_PC360K (0x30) | Clear | Clear |
| PC_MFM_525_1200 | disk_PC12M (0x32) | Set | Set |
| PC_MFM_35_720   | disk_PC720K (0x31) | Set | Clear |
| PC_MFM_35_1440 | disk_PC144M (0x33) | Set | Clear |
| PC_MFM_35_1680 | disk_PC144M (0x33) | Set | Clear |

### SCP Footer updates

The modification time in the footer is also updated to indicate that the file has been changed. This also requires updating the checksum
because it covers the footer (It doesn't cover the header so the disk type and flag changes don't impact the checksum)

The checksum is simply an addition of all the bytes in the file beyond the header so to update the checksum the bytes from the original
modification date are subtracted and the bytes of the new date are added.

## Encoding HFE files

### Header

Most of the information in the HFE is fairly straightforward. The number of tracks is based on the number of tracks decoded.
The number of sides is determined by checking if any track contains side 0 entries and if any track contains side 1 entries.
If both sides are present then sides is set to 2, otherwise it is set to 1.

The track offset list location is set to 1 to indicate that it starts at the second 512 byte block

The RPM, bitrate and interface type are set based on the type of disk selected

| Disk Type | Rotation Speed (RPM) | Bitrate (Kb/s) | Interface Mode |
| --- | ---: |  ---: |  ---: | ---: | ---: |
| 5.25" DS-DD 360k | 300 | 250 | IBM PC Double Density |
| 5.25" DS-HD 1200k | 360 | 500 | IBM PC High Density  |
| 3.5" DS-DD 720k | 300 | 250 | IBM PC Double Density |
| 3.5" DS-HD 1440k | 300 | 500 | IBM PC High Density |

The Interface Mode is based off of creating test images with HxC tools. The Double and High density interface modes might line up
with the 250 Kb/s and 500 Kilobits/s bitrates respectively.

The bitrate can also be calculated using the rotation speed in radians/s
and bitcell length in radians.

bitrate = rotation speed in radians per second / bitcell length in radians

or the number of bitcells per second

| Disk Type | Rotation Speed (rad/s) | bitcell length (rad) | bitrate (bits/s) |
| --- | ---: |  ---: |  ---: | ---: | ---: |
| 5.25" DS-DD 360k | 31.416 | 0.0001257 | 249927.8165 |
| 5.25" DS-HD 1200k | 37.699 | 0.0000755 | 499325.9847 |
| 3.5" DS-DD 720k | 31.416 | 0.0001257 | 249927.8165 |
| 3.5" DS-HD 1440k | 31.416 | 0.0000628 | 500253.6073 |

### Track Offsets

The number of tracks is used to calculate the number of 512 byte blocks needed to store track offset information. The Offset for each
track is then calculated by using that value plus 1, for the header, and a running accumulation of the number of blocks needed to store
each track

The track length is calculated by finding the maximum number of bytes required between both sides of the track and rounding up.

### Track Data

Track data is extended to an even 256 bytes by adding 01s to the end of the decoded track data. This fills up the end of the track with 0 data bits

The HxC tools seem to repeat the start of the track in the remainder of the last block. In my testing this is problematic because it could
start reading part of a sector identifier and then read that same sector identifier again when it gets to the end of the block and restarts
at the beginning of the track. Writing 0 bits might artificially extend the end of the track but prevents information from being repeated.

The bytes are reversed because HFE files read bytes starting with the least significant bit

If rotation fixups are enabled then the track is also rotated to try and prevent the end of the track appearing in the middle of an identifier
or data block as that could cause issues reading the disk. 

Because HFE files specify the length of a track in bytes, and also some tools don't seem to check that value, there is likely to
be a discontinuity at the end of each track as it's not clear where in the last byte, or block if the actual length is ignored, the
track should wrap around and start from the beginning. Rotating the track ensures that this discontinuity is inside of a gap and not
in the middle of an identifier or data block where it could cause issues.

If a side of a track doesn't exist then all 0s will be written for that section of the track.

### Encoding IMG files

IMG files are very simple and only contain data.

The data blocks from each sector are written out in order for each track. The sectors from Side 0 are written, if they exists,
and the sectors from side 1 are written, if they exist. The sectors are always written in assenting order based on sector number.