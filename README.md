# S3mToMidi
Converts S3M tracker files to midi

# What
This is a C# library and application which can read ScreamTracker (.s3m) files and generate MIDI (.mid) files.

# Why
Back in the 2000's, I was really into mod songs, especially those by Jake Kaufman (virt).  I wrote some songs, but mostly listened to his, and wanted to understand more of how the songs worked harmonically.  As I could read and understand music notation better than tracker notation, I started working on a program to convert between the two.  At the same time, I wanted a side project to work on my programming skills outside of the office.

# How to use
Afer building, call `S3mToMidi` and pass the path to a .s3m file.  The application will then generate a midi file in the working directory, and you can then listen to it in Windows Media Player, or open it in a notation program like Dorico, Finale, or Sibelius to view the score and follow along.  I've found that Dorico does the best job turning these midi files into readable music notation.

## Options

### Exclude Channel

Because the "drum" tracks in the module are currently mapped to the default piano midi sound, they can be very distracting.  Use the `--exclude-channel <channel number>` command line argument to exclude a channel from the output file.

### Start Order

Some songs have intros that use tempo and speed for special effects and cannot be easily understood with notation.  Use the `--start-order <order number>` command line switch to specify which pattern in the song's pattern order to start from.  You can get this information by opening the song in a tracker and going to the `Orders` page.  Note that the order is not the same as the pattern number.  In particular this is needed for v-bogey because the time signature of the intro cannot be handled correctly by any notation programs I've found.

### Pattern

Use the `--pattern <patter number>` command line parameter to export a single pattern.  This is mostly useful when debugging changes to this program.

### Minimum volume

Use the `--minimum-volume <volume level 0-64>` command line parameter to filter out quiet notes.  Any note with a volume lower than `<volume level>` will be treated as volume `0`.  Use this to simplify the output file and also to remove most note echos. Note that this may introduce gaps in the output midi file.

## Examples
```
S3mToMidi --file mysong.s3m

# export pattern 20 from v-blast
S3mToMidi --file v-blast.s3m --pattern 20

# start v-bogey after the Top Gun intro and exclude the drum channels
S3mToMidi --file v-bogey.s3m --minimum-volume 32 --start-order 4 --exclude-channel 4 --exclude-channel 5
```

# Notes
Since the program was originally written to aid in education, it does not support all effects, and really only works with chiptunes written by virt.  I welcome contributions to support more songs or improve the existing support.

# Known issues
* The triangle channel can overpower the rest of the channels at playback.  The triangle wave is typically played at full volume, which is normally fine in a chiptune.  But because all instruments are mapped to Piano, the triangle wave's part, which is typically the bass line, can sound way too loud relative to the other parts.

[TODO](./S3mToMidi/TODO.txt)

[Bugs](./S3mToMidi/bugs.txt)
