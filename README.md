# S3MParser
Converts S3M and IT tracker files to midi

# What
This is a C# library and application which can read ScreamTracker (.s3m) files and generate MIDI (.mid) files.

# Why
Back in the 2000's, I was really into mod songs, especially those by Jake Kaufman (virt).  I wrote some songs, but mostly listened to his, and wanted to understand more of how the songs worked harmonically.  As I could read and understand music notation better than tracker notation, I started working on a program to convert between the two.  At the same time, I wanted a side project to work on my programming skills outside of the office.

# How to use
Afer building, call s3mtomidi.exe and pass the path to a .s3m file.  The application will then generate a midi file in the working directory, and you can then listen to it in Windows Media Player, or open it in a notation program like Finale or Sibelius to view the score and follow along.

# Notes
Since the program was originally written to aid in education, it does not support all effects, and really only works with chiptunes written by virt.  Some are included in the repo as reference/test files.  I welcome contributions to support more songs or improve the existing support.

# Known issues
* Drum channels are treated like pitched instruments, which produces a very distracting noise when the MIDI file is played back.  It turns out that virt typically dedicates a channel to drum parts, and so I hardcoded the exe to skip those channels.
* Some time changes are not handled correctly
* The triangle channel can overpower the rest of the channels at playback.  The triangle wave is typically played at full volume, which is normally fine in a chiptune.  But because all instruments are mapped to Piano, the triangle wave's part, which is typically the bass line, can sound way too loud relative to the other parts.

[Song Specific Notes](./S3mToMidi/notes.txt)

[TODO](./S3mToMidi/TODO.txt)

[Bugs](./S3mToMidi/bugs.txt)
