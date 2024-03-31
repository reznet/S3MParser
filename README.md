# S3mToMidi
Converts S3M tracker files to midi

# What
This is a C# library and application which can read ScreamTracker (.s3m) files and generate MIDI (.mid) files.

# Why
Back in the 2000's, I was really into mod songs, especially those by Jake Kaufman (virt).  I wrote some songs, but mostly listened to his, and wanted to understand more of how the songs worked harmonically.  As I could read and understand music notation better than tracker notation, I started working on a program to convert between the two.  At the same time, I wanted a side project to work on my programming skills outside of the office.

# How to use
Afer building, call `S3mToMidi` and pass the path to a .s3m file.  The application will then generate a midi file in the working directory, and you can then listen to it in Windows Media Player, or open it in a notation program like Finale or Sibelius to view the score and follow along.

## Options

### Channels From Patterns

This program maps samples/instruments to MIDI channels because in many of virt's files, he dedicates each channel to a sound just like if he was writing for the NES sound chip.  Some S3M files, though, will switch between samples in a single channel.  Depending on the song, the resultant notation can be hard to read or follow.  So, the program has the `--channels-from-patterns` which alters the behavior to treat all notes in a tracker channel as if they're on the same MIDI channel.  When this option is enabled, for example, a 6 channel song will have 6 MIDI channel/tracks.  Omit the option to use the default which dedicates each sample/sound/instrument to its own MIDI channel/track.

### Exclude Channel

Because the "drum" tracks in the module are currently mapped to the default piano midi sound, they can be very distracting.  Use the `--exclude-channel <channel number>` command line argument to exclude a channel from the output file.  For virt's songs, this means using `--exclude-channel 4 --exclude-channel 5`.

### Start Order

Some songs have intros that use tempo and speed for special effects and cannot be easily understood with notation.  Use the `--start-order <order number>` command line switch to specify which pattern in the song's pattern order to start from.  You can get this information by opening the song in a tracker and going to the `Orders` page.  Note that the order is not the same as the pattern number.

### Pattern

Use the `--pattern <patter number>` command line parameter to export a single pattern.  This is mostly useful when debugging changes to this program.

## Examples
```
S3mToMidi --file mysong.s3m

# export pattern 20 from v-blast
S3mToMidi --file v-blast.s3m --pattern 20

# start v-bogey after the Top Gun intro and exclude the drum channels
S3mToMidi --file v-bogey.s3m --channels-from-patterns --start-order 4 --exclude-channel 4 --exclude-channel 5
```

# Notes
Since the program was originally written to aid in education, it does not support all effects, and really only works with chiptunes written by virt.  Some are included in the repo as reference/test files.  I welcome contributions to support more songs or improve the existing support.

# Known issues
* Some time changes are not handled correctly
* The triangle channel can overpower the rest of the channels at playback.  The triangle wave is typically played at full volume, which is normally fine in a chiptune.  But because all instruments are mapped to Piano, the triangle wave's part, which is typically the bass line, can sound way too loud relative to the other parts.
* Doubled Parts - Virt uses a chiptune technique where a waveform is triggered and then immediately followed by another waveform of a different duty cycle.  This makes the instrument sound richer than a simple sine or pulse wave, but because all instruments are mapped to Piano, causes double attacks for these parts, and causes doubled notes in the visual score, which makes it harder to reason over the parts.  It's hard to know what to do in the general case, but I've had success hardcoding the exe to either ignore or merge these doubled parts into a single part.

[TODO](./S3mToMidi/TODO.txt)

[Bugs](./S3mToMidi/bugs.txt)
