# FM Encoding

FM, or Frequency Encoding, is a method of storing information on magnetic media

Information is stored using changes in the polarity of a magnetic substance, these are known as flux transitions.
A flux transition encodes a binary 1 while a lack of a flux transition encodes a 0.

FM Encoding inserts a flux transition as a clock bit between each data bit. This allows for easy timing synchronization but limits the 
amount of information that can be stored because there is a physical limit for how close two flux transitions can be to
each other.

The space between one clock pulse and the next is known as a bitcell. The flux transitions, if one is needed, for each data bit should
occur in the center of the bitcell. The flux transitions for the clock occur on the boundaries of the bitcell.

## Example

For an example lets consider the byte 11010010. To encode this we simply insert a 1 clock bit between each data bit

| Data   |   |   1   |   |   1   |   |   0   |   |   1   |   |   0   |   |   0   |   |   1   |   |   0   |   |
| ---    | :---: | :---: | :---: | :---: | :---: | :---: | :---: | :---: | :---: | :---: | :---: | :---: | :---: | :---: | :---: | :---: | :---: |
| Clock  | 1 |       | 1 |       | 1 |       | 1 |       | 1 |       | 1 |       | 1 |       | 1 |       | 1 |
| Result | 1 | **1** | 1 | **1** | 1 | **0** | 1 | **1** | 1 | **0** | 1 | **0** | 1 | **1** | 1 | **0** | 1 |

The following diagram shows how this data would be recorded on a magnetic medium

![A diagram showing how the byte 11010010 is stored with FM Encoding](FM_Data.svg)

The bitcells are represented by the alternating light and dark grey backgrounds. The values of the data bits are along the top and the
value of the clock bits are allow the bottom. The blue line going along the diagram represents the polarity of the magnetic substance.
It has two states, near the top and near the bottom. A flux transition is represented by the line going from one position to the other
and then continuing on from there. 

From left to right we have

* Clock bit, 1, polarity changes
* Data bit, 1, polarity changes
* Clock bit, 1, polarity changes
* Data bit, 1, polarity changes
* Clock bit, 1, polarity changes
* Data bit, 0, no polarity change
* Clock bit, 1, polarity changes
* Data bit, 1, polarity changes
* Clock bit, 1, polarity changes
* Data bit, 0, no polarity change
* Clock bit, 1, polarity changes
* Data bit, 0, no polarity change
* Clock bit, 1, polarity changes
* Data bit, 1, polarity changes
* Clock bit, 1, polarity changes
* Data bit, 0, no polarity change
* Clock bit, 1, polarity changes

The minimum number of 0s in a row is zero and the maximum is one because of this FM Encoding is also known as Group coded recording, GCR,
(0,1) with the 0 representing the minimum number of 0s and the 1 representing the maximum number of 0s.

For FM Encoding the bitcell length must be at least twice the minimum flux transition spacing as it's possible for there to be 3 flux
transitions in the span of a bitcell.

## Disk Layout