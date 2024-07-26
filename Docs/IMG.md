# IMG Image Format

I don't believe there is a specification for the IMG format as it only contains raw data

This file contains the binary data of a floppy disk

This format only contains data and there is no header

The sectors from the disk are written in order for each track and side to generate a completely flat image of the disk. Track 0 side 0
is written first and then track 0 side 1, size 1 track 0, side 1 track 1 etc.
