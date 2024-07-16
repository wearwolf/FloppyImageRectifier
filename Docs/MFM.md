# MFM Encoding

MFM, or Modified Frequency Encoding, as the name implies is a modification of the earlier [FM encoding][docs-fm]

MFM Encoding tries to improve the information density by not allowing two flux transitions to occur in sequence. This is done by suppressing
the clock bit if it is adjacent to a 1 bit. This has the benefit of allowing twice as much information to be stored because the length
between any two flux transitions is doubled but at the cost of increasing the complexity of timing synchronization because there is no
longer a consistent clock pulse

The rules for clock bits then become
* A 1 data bit will have a 0 clock bit before and after it
* A series of two 0 data bits will have a 1 clock bit between them

## Example

For an example lets consider the byte 11010010. To encode this we insert a 0 clock bit around each 1 data bit and a 1 between subseqnent
0 bits.

| Data   |   |   1   |   |   1   |   |   0   |   |   1   |   |   0   |   |   0   |   |   1   |   |   0   |   |
| ---    | :---: | :---: | :---: | :---: | :---: | :---: | :---: | :---: | :---: | :---: | :---: | :---: | :---: | :---: | :---: | :---: | :---: |
| Clock  | 0 |       | 0 |       | 0 |       | 0 |       | 0 |       | 1 |       | 0 |       | 0 |       | x |
| Result | 0 | **1** | 0 | **1** | 0 | **0** | 0 | **1** | 0 | **0** | 1 | **0** | 0 | **1** | 0 | **0** | x |

We don't know what the last clock bit is because it depends on what the next data bit is. If the last data bit was a 1 then we would
know that the next clock bit would have to be a 0.

The following diagram shows how this data would be recorded on a magnetic medium

![A diagram showing how the byte 11010010 is stored with MFM Encoding](MFM_Data.svg)

The bitcells are represented by the alternating light and dark grey backgrounds. The values of the data bits are along the top and the
value of the clock bits are allow the bottom. The blue line going along the diagram represents the polarity of the magnetic substance.
It has two states, near the top and near the bottom. A flux transition is represented by the line going from one position to the other
and then continuing on from there. 

From left to right we have

* Clock bit, 0, no polarity change
* Data bit, 1, polarity changes
* Clock bit, 0, no polarity change
* Data bit, 1, polarity changes
* Clock bit, 0, no polarity change
* Data bit, 0, no polarity change
* Clock bit, 0, no polarity change
* Data bit, 1, polarity changes
* Clock bit, 0, no polarity change
* Data bit, 0, no polarity change
* Clock bit, 1, polarity changes
* Data bit, 0, no polarity change
* Clock bit, 0, no polarity change
* Data bit, 1, polarity changes
* Clock bit, 0, no polarity change
* Data bit, 0, no polarity change
* Clock bit, unknown

The minimum number of 0s in a row is one and the maximum is three because of this MFM Encoding is also known as GCR(1,3) with the 1 representing
the minimum number of 0s and the 3 representing the maximum number of 0s.

For MFM Encoding the bitcell length must be at least the minimum flux transition spacing as it's possible to have a flux transition
in the center of two adjacent bitcells or on both boundaries of a bitcell. 

There can be one and a half bitcells between flux transitions when starting or ending a series of 0 data bits and two bitcells between
flux transitions if a 0 data bit appears between two 1 data bits.

[docs-fm]: FM.md

