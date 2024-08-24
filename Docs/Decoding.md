# Decoding

## Flux Decoding

SCP files store a series of time entries that indicate when a flux transition has occurred. Flux transitions always indicate a binary 1.
The problem with decoding magnetic data is determining how many 0s exist between the 1s. 

The various floppy disk specs specify the expected bitcell length and the expected ranges for sets of 0s. (See [MFM Timing][docs-mfm-timing])


[docs-mfm-timing]: MFM.md#Timing
