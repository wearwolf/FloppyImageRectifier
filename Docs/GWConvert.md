# GW convert

This page details how the GW application converts an SCP file to an HFE file. 

The command line used is

```
gw convert "<scpPath>" "<hfePath>:bitrate=500"
```

the bitrate would be 250 for 360k or 720k disk images

## gw.py

gw.py imports cli and then calls [cli.main()](#climain)

## greaseweazle/cli.py

### cli.main

Called from [gw.py](#gwpy)

This function starts by doing some command line argument checking for general options. It then tests to see if the first argument,
after removing the general options, is in the actions list.

If the action is found it then imports a module by looking for "greaseweazle.tools.\<action\>" and checking if that module has a main method. If the method
exists the program calls it.

For the convert example it calls [convert.main()][convert.main]

## greaseweazle/tools/convert.py

### convert.main

Called from [cli.main]

The main method starts by defining an epilog string and creating an [ArgumentParser][util.ArgumentParser] object by passing in
 purpose string and the epilog string (it gets shown at the end of the help string). It then adds a series of arguments to
 the parser by calling [add_argument][ArgumentParser.add_argument].

The following arguments are defined for the command

| name | short name | usage name | type | Description |
| --- | --- | --- | --- | --- |
| --diskdefs | | | | disk definitions file |
| --format | | | |disk format |
| --tracks | | TSPEC | [util.TrackSet] | which tracks to read & convert from input |
| --out-tracks | | TSPEC | [util.TrackSet] | which tracks to output (default: --tracks |
| --adjust-speed | | SPEED | [util.period] | scale track data to effective drive SPEED |
| --no-clobber | -n | | store_true | do not overwrite an existing file |
| --pll | | PLLSPEC | [track.PLL] | manual PLL parameter override |
| in_file | | | | input filename |
| out_file | | | | output filename |

After adding the arguments, the [parse_args][ArgumentParser.parse_args] method on the parser is called to parse them.

Then the out_file options are separated from the file path using [util.split_opts] and stored in args.out_file_opts

If a PLL override is specified then it is inserted at the front of the global PLL list

The function then looks up an image class object with [util.get_image_class] for the in_file and the out_file

if no format option is specified then it tries to use the format parameter from the input image class, if that isn't specified then it
uses the format parameter from the output image class

If a format is found, it tries to look up a diskdef with [codec.get_diskdef] for the format argument and the specified diskdefs file.
This gets stored in args.fmt_cls. Track information from the diskdef is copied to def_tracks

IF def_tracks isn't defined, it is set using [util.TrackSet] with 'c=0-81:h=0-1'.

out_def_tracks defaults to a copy of def_tracks

If the tracks argument is defined, def_tracks is updated based on the argument and that then gets copied into out_def_tracks

args.tracks then gets updated based on def_tracks

If the out_tracks argument is set, out_def_tracks is updated based on the argument and then copied backed into args.out_tracks

[open_input_image][convert.open_input_image] is called to load the input file based on the arguments and the in_image_class and the result gets stored in in_image

[open_output_image][convert.open_output_image] is called to open the output file based on the arguments and then out_image_class and the result gets stored in out_image

[convert][convert.convert] is called with arguments, in_image, out_image to convert the files

### convert.open_input_image

Called from [convert.main]

open_input_image() calls from_file on the passed in image class object with the in_file argument and fmt_cls

For this example that would be [scp.from_file]

### convert.open_output_image

Called from [convert.main]

open_output_image() calls to_file on the passed in image class object with the out_file argument, fmt_cls and no_clobber

For this example that would be [hfe.to_file]

The args.out_file_opts values are checked against the output image to see if the options exist as fields on the image class. The options
are then set as fields on the image

### convert.convert

Called from [convert.main]

The convert function loops through args.out_tracks and gets the cylinder and head value for each track.

It checks if the cylinder and head combination has a value stored in summary and store it in dat

Otherwise it calls [process_input_track][convert.process_input_track] with args, the result of calling 
[TrackIdentity][convert.TrackIdentity] on args.tracks, cyl and head and the in_image, the output is stored in dat

if dat is none then it gets ignored, otherwise if args.fmt_cls is set it checks if dat is an instance of Codec and stores the value in summary.

Then emit_track is called on out_image with the cylinder, head and dat

For this example that would be [hfe.emit_track]

### convert.TrackIdentity

A class that is used to describe a specific track

#### TrackIdentity.init

TrackIdentity is an object that takes a [trackset][util.trackset] along with a cylinder and a head value. 
The Cylinder and head values are set as fields on the object.

physical_cyl and physical_head are calculated by calling [ch_to_pch][trackset.ch_to_pch] on the trackset with the cylinder and head values

### convert.process_input_track

process_input_track takes the arguments specified for the current operation, a track identity and the input image as parameters. 

The cylinder and head values are separated from the track identity, an optional variable dat of the HasFlux type is also declared

It them calls get_track on the input image using the physical head and cylinder as arguments

For this example that would be [scp.get_track]

If the track is none then the method returns none

If an adjust_speed argument was provided then it checks to see if the track is an instance of Codec, if it is then it calls a master_track
function on track and stores that as track. It then checks if track is an instance of MasterTrack, if it is then it calls a flux function on
track and stores that as track. Then it calls scale with the adjust_speed argument divided by the time_per_rev from track as a parameter.

If the fmt_cls argument is none or the track is an instance of Codec, track is stored in dat and returned.

Otherwise it calls a decode_flux on the fmt_cls argument and passes it the cylinder number, the head number and the track and stores the
return value in dat. If dat is none then a warning is displayed and none is returned, otherwise it asserts that dat is an instance of Codec,
and then loops through the pll list starting at index 1, it calls a nr_missing function on the dat variable and if it returns 0, it exists the 
loop, otherwise it calls decode_flux on dat with the track and the current pll item as parameters

## greaseweazle/image/scp.py

This file contains the code for describing and processing SCP files

See [SCP][docs-scp]

It contains a list of disk types and set of flags used in the SCP header

### scp.SCPOpts

This class defines the options available for the SCP image.

* legacy_ss - use the legacy single sided format
* disktype - the disk type of the image

The property definition for disktype parses the pased in value and validates that it's in the list

### scp.SCPTrack

Defines an SCP track

* tdh - the track revolution headers
* dat - the track data
* splice - extension value

### scp.scp

Defines an SCP image and provides methods for processing them

Defines the default frequency as 40000000 (40MHz), this corresponds to the default sample rate of 25 ns, 1/25 x 10<sup>−9</sup> = 40000000)

* opts - an instance of [SCPOpts][scp.SCPOpts]
* nr_revs - (optional) number of revolutions
* to_track - a dictionary of track numbers to [SCPTrack][scp.SCPTrack] objects
* index_cued - indicates if the image is index cued

#### scp.init

Creates a blank SCP image, sets opts to a new [SCPOpts][scp.SCPOpts] object, sets nr_revs to none, sets to_track to a new dictionary and
sets index_cued to true

#### scp.side_count

Determines the number of tracks per side. SCP images number tracks consecutively and interweave tracks for both sides.

The function loops through the values from to_track and determines the side by anding the track number with 1, this is basically a check
of the least significant bit. Even numbered tracks will have a least significant bit of 0 and get counted towards side 0, odd numbered tracks
will have a least significant bit of 1 and get counted towards side 1.

#### scp.from_file

Called from [convert.open_input_image]

This function is marked as a class method wo that it can be called without an instance. 

It takes a variable representing the class, a file name and a format as parameters

The method starts by setting splices to none

It then opens the file specified by name for read binary and reads all of it into dat.

Next the function uses struct.unpack to read the header values from dat. Some of the values are stored in variables, others are ignored.

| Variable | Header value |
| --- | --- |
| sig | Signature |
| _ | VersionRevision |
| disk_type | DiskType |
| nr_revs | NumberOfRevolutionsPerTrack |
| _ | StartTrack |
| _ | EndTrack |
| flags | Flags |
| _ | BitCellWidth |
| single_sided | HeadNumber |
| _ | CaptureResolution |
| checksum | Checksum |

I'm guessing StartTrack and EndTrack are ignored because they can be determined from the track list and VersionRevision, BitCellWidth 
and CaptureResolution are ignored because they very rarely change

The function then verifies that sig equals "SCP" and verifies that the sum of the rest of the bytes in the file match the checksum

The function then reads 168 integers from dat and stores it in trk_offs. It then does a loop from 0 to 168 and tries to read the offset
from trk_offs. If there's an error reading the value it breaks out of the loop. if the value is 0 or greater than 0x2b0 (The typical end of
the track offset list) it just continues. Otherwise it divides the value by 4 and then subtracts 4. It then verifies that the result is
greater than 0. If it is then it adds the result to the end of trk_offs (I don't know what this is for)

After this it checks for the existence of an extension block and parses it (I don't think any of my images use this).

At this point the program creates a new scp image by calling the cls constructor

Then the program loops through the track offsets. The track offset is read from trk_offs, if the value is 0 the track is skipped. 
The program then gets the track header data by getting 4 + 12 x number of revolution bytes starting at the track offset. It starts
by getting the signature and track number from the header and verifying the track signature = TRK and that the track number matches
the index of the track offset. Next it removes that part of the track header from the bytes extracted. The program then does a reverse
loop through the remaining revolution headers and removes any trailing revolutions with 0 track length or data offset. If no revolutions are
left it skips to the next track. If the track is not index cued and the there are multiple revolutions, the first revolution is skipped.
The program then reads the data offset from the remaining first revolution. The program adds the length of the last revolution of the track
to the offset of the last revolution. If the offset of the first revolution and the last revolution match, the track is skipped. Finally
the data for all the revolutions is extracted from the file data and stored in a new [SCPTrack][scp.SCPTrack] object along with the remaining
data in the track header. If splices is not none (Calculated as part of the extension block) then the splice value for this track is also
stored on the SCPTrack object. Finally the track is added to to_track on the scp object.

Next the program computes the number of tracks per side and stores that in s

There are then some fixups for specific disk formats

Finally the scp object is returned

#### scp.get_track

This function returns a track from the scp image

The track number is calculated by doubling the cylinder number and adding the head number

If that track number doesn't exist in the track list, none is returned

The track is retrieved from the track list and the revolution headers and data are separated out into variables.

All of the index time values are extracted from the revolution headers and stored in a list

The function then loops through the track data bytes, The data is extracted by adding the first byte * 256 to the next byte (to get a
16 bit, big endian value). If the value is 0, 65536 is added to the val variable and the next two bytes are read, otherwise the value
is added to val and stored in flux_list and val is reset to 0. 

A [flux][flux.flux] object is created from the index list, the flux_list and the image's sample frequency

The track's splice information is added to the flux object if available

finally the flux object is returned

## greaseweazle/image/hfe.py

### hfe.to_file

Called from [convert.open_output_image]

### hfe.emit_track

## greaseweazle/track.py

### track.PLL

PLL stands for Phase-locked loop. This is the mechanism used to synchronize the clock for MFM based floppy drives.

The object defines three values, period adjust percentage (period_adj_pct), phase adjust percentage (phase_adj_pct) 
and low pass threshold (lowpass_thresh).

#### PLL.init

The init function takes a string and parses it to determine the desired parameters. 

The string should contain a set of key value pairs. The pairs are separated by a : and an = separates the key and the value. 
period sets period_adj_pct, phase sets phase_adj_pct and lowpass sets lowpass_thresh

If no value is specified, period_adj_pct defaults to 5, phase_adj_pct defaults to 60 and lowpass_thresh defaults to None

## greaseweazle/flux.py

### flux.flux

## greaseweazle/codec/codec.py

#### Codec.get_diskdef

Called from [convert.main]

## greaseweazle/tools/util.py

### util.ArgumentParser

Used by [convert.main]

This class derives from [argparse][argparse].ArgumentParser

The init function takes a formatter_class which defaults to CmdlineHelpFormatter, and *args, **kwargs which all get passed into the super
init function

**kwargs contains a dictionary of key-value pairs created from parameters passed into the function and is used to assign values to the parameters when calling the
base class init function

usage -  The string describing the program usage

epilog - Text to display after the argument help

#### ArgumentParser.add_argument

Adds a possible argument and a description for it. Argument names starting with -- are keyword arguments, other
arguments are positional

#### ArgumentParser.parse_args

parses the set of arguments passed in and generates an object with the argument keywords as fields.

### util.split_opts

Called from [convert.main]

Splits key-value pair options off of a string

It starts by splitting up the string on "::" characters. 

The first part is stored as the name and an empty dictionary is created.

The parts are then split by ":" before it attempts to split them by "="

If the "=" split is successful then it's stored as a key-value in the dictionary, else it's stored with a "Y"

### util.get_image_class

Called from [convert.main]

Looks up the image_class based on a file path

The method starts by getting the extension from the file path.

The method then looks up the lowercase version of the extension in image_types and throws an error if it isn't.

If the result is a tuple then it's stored as typename and classname, otherwise the single value is stored as typename and the lowercase
version of the name is stnored as classname.

A module defined by 'greaseweazle.image.' + classname is then loaded and a __dict__ method is called with typename as an argument

### util.period

Converts a time period in various formats into seconds

An RPM value is calculated by dividing 60 by the value

A millisecond value (ms) is divided by 1e3

A microsecond value (us) is divided by 1e6

a nanosecond value (ns) is divided by 1e9

An SCP value is divided by 40e6

### util.TrackSet

Contains a list of cylinders and heads along with options like if the heads are swapped, offset per head and the step

The constructor initializes the default values and then calls update_from_trackspec with the passed in string

Head offset = 0, step = 1, head swap = false

#### TrackSet.update_from_trackspec

Decodes the passed in string to determine the track information

The value is split by ":" and then the parts are checked.

If a part is hswap then head swap is set to true

otherwise it tries to split it on "=", the left hand side is treated as a key and the right hand side is treated as the value.

If the key is c then the value is parsed to determine a list of cylinders. THe value is a series of ranges separated by commas. The
ranges can either be a single value, a start and end value separated by a dash or a start and end value separated by a dash and followed
by a slash and a step value. A RegEx is used to parse the possible values and the result is determined by checking the matching groups.
The ranges are used as the parameters to a loop which sets the appropriate item in a Boolean array to true. After parsing all of the ranges it
runs through the Boolean list and if an item is true then that index is added to the cylinder list.

If the key is h then the value is parsed to determine a list of heads. The value is either a single value (0 or 1) or a range of
values. A RegEx is used to parse the possible values and the result is determined by checking the matching groups.
The ranges are used as the parameters to a loop which sets the appropriate item in a Boolean array to true. After parsing all of the ranges it
runs through the Boolean list and if an item is true then that index is added to the head list.

If the key is h0.off or h1.off then it is parsed to determine the head index and the value is parsed to determine the offset for
that head. The value can be positive or negative.

If the key is step then the value is parsed to determine the step amount. If the value is 1/x something then the step is saved as -x,
otherwise the value is just saved as the step value

#### TrackSet.ch_to_pch

Translates a cylinder and head value to a physical cylinder and head value based on the trackset options

if the track step is less than 0 then the cylinder is divided by the negative of the step, otherwise it is multiplied by the step

The head offset for the head value is then added to the physical cylinder value

If headswap is true then return 1 minus the head number, otherwise the head number is returned

[argparse]: https://docs.python.org/3/library/argparse.html
[docs-scp]: SCP.md
