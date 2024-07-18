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

## Timing

Timing is the main challenge when it comes to decoding a magnetic medium. Knowing when a 1 is detected is trivial because there will
always be an accompanying flux transition but determining how many 0s are between those flux transitions can be difficult especially
as no two systems might have slightly different clock rates.

The various Floppy Disk standards that use MFM for encoding specify the range of bitcell lengths for the various bit sequences.

| Sequence | Number of 0s | Percentage of Bitcell length |
| --- | --- | --- |
| 1 1 data bits with a 0 clock bit between them or 0 0 0 data bits with 1 clock bits between them | 1 | 80 - 120% |
| 1 0 data bits with a 0 clock bit between them or 0 1 data bits with a 0 clock bit between them | 2 | 130 - 165 % |
| 1 0 1 data bits with 0 clock bits between them | 3 | 185 - 225% |

We can also determine the expected bitcell length based on the standards which include the nominal bitcell length in microradians and the
speed of rotation in rotations per minute

bitcell length in seconds = ((rotation speed in RPM x 2 pi) / 60) / bitcell length in microradians x 10<sup>−6<sup> 

| Disk Type | Rotation Speed (RPM) | Rotation Speed (rad/s) | bitcell length (μrad) | bitcell length (rad) | Bitcell length (s) |
| --- | ---: |  ---: |  ---: | ---: | ---: |
| 5.25" DS-DD 360k | 300 | 31.416 | 125.7 | 0.0001257 | 4.00116 x 10<sup>−6<sup> |
| 5.25" DS-HD 1200k | 360 | 37.699 | 75.5 | 0.0000755 | 2.0027 x 10<sup>−6<sup>  |
| 3.5" DS-DD 720k | 300 | 31.416 | 125.7 | 0.0001257 | 4.00116 x 10<sup>−6<sup> |
| 3.5" DS-HD 1440k | 300 | 31.416 | 62.8 | 0.0000628 | 1.99899 x 10<sup>−6<sup> |

But this is just the expected bitcell length, the actually length encountered on a disk may very because no drive is created perfectly
or stays perfect. To handle this, floppy controllers need to try and figure out what the encoded bitcell length is. They do this with a
feedback mechanism known as Phase-locked loop or PLL. Basically the controller compares the expected bitcell length against the actual 
bitcell length it is seeing on the disk and adjusts the expected bitcell length towards the actual bitcell length by some fraction of
the difference. The idea is that eventually the two will lock on and keep the expected bitcell length consistent with what is being read


## Disk Layout



[docs-fm]: FM.md

