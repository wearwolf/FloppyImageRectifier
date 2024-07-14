# MFM Encoding

MFM, or Modified Frequency Encoding, as the name implies is a modification of the earlier [FM encoding][docs-fm]

MFM Encoding tries to improve the information density by not allowing two flux transitions to occur in sequence. THis is done by suppressing
the clock bit if it is adjacent to a 1 bit. This has the benefit of allowing twice as much information to be stored because the length
between any two flux transitions is doubled but at the cost of increasing the complexity of timing synchronization because there is no
longer a consistent clock pulse

[docs-fm]: Docs/FM.md

