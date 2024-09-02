# Decoding

## Flux Decoding

SCP files store a series of time entries that indicate when a flux transition has occurred. Flux transitions always indicate a binary 1.
The problem with decoding magnetic data is determining how many 0s exist between the 1s. 

The various floppy disk specs specify the expected bitcell length and the expected ranges for sets of 0s. (See [MFM Timing][docs-mfm-timing])

The first step is to normalize the time entries. SCP files typically store the time entries as 16-bit unsigned integers. If the time value
exceeds what can be stored in a single 16-bit value then that entry is set to zero and 65536 is added to the next value. This continues until a
non-zero value is found. The time entries are also multiplied by the tick resolution to get the actual time in seconds.

Decoding is done by dividing each normalized time entry by the expected bitcell length in seconds. The result is then compared to a
series of ranges to determine how many 0s should be recorded. The ranges are based on the expected ranges from the spec but altered to
be continues.

| Percentage of Bitcell length | Result |
| --- | --- |
| < 80% | Ignored |
| 80% - 130% | One 0 bit |
| 130% - 185% | Two 0 bits |
| 185% - 225% | Three 0 bits |

Percentages higher than that are calculated using a formula

Number of 0 bits = (time / 2) - 1

The number of 0s goes up by one for each half-bitcell beyond the first one so this should be a good approximation. A properly MFM
encoded disk will never have flux transitions more than 225% of a bitcell apart so these time entries shouldn't represent valid data.
They are still likely to occur on the disk because gaps may not have been encoded properly or may of had their values change over time.

For the normal 0 bit strings the expected bitcell length is updating based on the value that was just processed. The time entry is divided
by the average number of bitcells expected (From the spec, not the ranges used for detection) based on the number of 0s detected to get the
current bitcell length.

| 0 Count | Average Bitcell length |
| --- | --- |
| One 0 bit | 100% |
| Two 0 bits | 147.5% |
| Three 0 bits | 205% |

The difference between the calculated and expected bitcell lengths multiplied by a constant is added to the expected bitcell length to
get the new expected bitcell length. This value is then checked against the min and max bitcell lengths which are 70% and 130% of the 
nominal bitcell length respectively.

The constant controls how quickly the expected bitcell length changes. We want bitcell length to change so that we can get more accurate
reads of the disk but we don't want temporary changes in bitcell length to throw off the whole process.

If rotation fix-ups are enabled then the program tries to transfer bits between successive rotations to ensure that each rotation is perfectly
circular. This is done by testing to see where the first quarter of a track rotation repeats. The program tests offsets between -20 and +20 of the
expected endpoint. If a match is found, that number of bits will be transferred to or from the current revolution to the next revolution.

After all the revolutions for a track are decoded the program tries to pick the best one.

First the program tries to group the revolutions based on how close they are two each other. Each revolution is assigned a group number starting
with 1 and increasing by 1 for each subsequent revolution. Next the program loops through each of the revolution group, tests to see if there is
a revolution with that group number and if there is, compares it against each other revolution with a higher group number. If the revolutions match
then the other revolution is assigned the group number being test.

Comparison is done by going through the bit lists for both revolutions are comparing the number of 0s that appear between each set of 1s. This is done
to try and prevent a single difference throwing the entire test off. The number of 1s in each revolution should be fairly consistent and assuming the
two revolutions aren't misaligned, the number of 0s should generally line up. The number of matches is divided by the number of sets to give a
match percentage. Two revolutions are said to be the same if they have more than 99% matches.

Next the program groups the revolutions by their group numbers and then tries to find the best group and the best revolution in that group. The processes
for both is very similar but is performed at two different levels

For groups

* First the groups are compared to find the one with the largest count
* Then the program finds all the groups with a matching count
  * If There is only one group with that count, it gets used
  * else the program checks if any of those groups contain revolutions where all the sectors are valid
    * If only one group contains revolutions where all the sectors are valid, it gets used
    * else if multiple groups contains revolutions where all the sectors are valid, the program checks if any have fix-ups applied
      * If only one group contains revolutions that have had fix-ups applied, it gets used
      * if multiple groups contain revolutions that have had fix-ups applied, the one with the highest group number is used
      * if no groups contain revolutions that have had fix-ups applied, the valid group with the highest group number is used
    * if no groups contains revolutions where all the sectors are valid, the program checks if any have fix-ups applied
      * If only one group contains revolutions that have had fix-ups applied, it gets used
      * if multiple groups contain revolutions that have had fix-ups applied, the one with the highest group number is used
      * if no groups contain revolutions that have had fix-ups applied, the large group with the highest group number is used

For revolutions

* if the group contains only one revolution, it gets used
* else the program checks if any of the revolutions have sectors that are all marked as valid
  * if there is only one revolutions where all sectors are valid, it gets used
  * if there are multiple revolutions where all sectors are valid, the program checks if any have fix-ups applied
    * if only one revolution has fix-ups applied, it gets used
    * if multiple revolutions have fix-ups applied, the one with the highest revolution number gets used
    * if no revolutions have fix-ups applied, the valid one with the highest revolution number gets used
  * if there are no revolutions where all sectors are valid, the program checks if any have fix-ups applied
    * if only one revolution has fix-ups applied, it gets used
    * if multiple revolutions have fix-ups applied, the one with the highest revolution number gets used
    * if no revolutions have fix-ups applied, the one with the highest revolution number gets used
 
 The selected revolution is the one used to generate the HFE file and IMG file for that track/side combination

### Data Decoding

In a lot of ways decoding the data from the bit stream is a lot easier than decoding the flux data because it's all digital.

The main trick is determining which bit is a data bit and which bit is a clock bit. This is accomplished by finding the Identifier Mark
sequence at the start of each Sector Identifier and Data block (See [MFM Disk Layout][docs-mfm-diskLayout]). Some of the bytes in the
Identifier Mark are missing clock pulses. This creates a sequence that shouldn't appear anywhere else in the disk and can be used to orient
the reader.

From this point on it's just a matter of reading the bits and ignoring the clock bits, which will be every other bit. 


[docs-mfm-timing]: MFM.md#Timing
[docs-mfm-diskLayout]: MFM.md#Disk-Layout
