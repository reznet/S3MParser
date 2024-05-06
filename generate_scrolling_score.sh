#!/bin/zsh

path_to_me=${0:A:h}
filename=${1}
filename_no_extension=${filename:r}

# make sure we're running with the latest code
dotnet publish

# generate lilypond score
$path_to_me/S3mToMidi/bin/Release/net8.0/publish/S3mToMidi \
    --file "${filename}" \
    --exclude-channel 4 \
    --exclude-channel 5 \
    --exporter lilypond

# generate scrolling score
ly2video -i "$filename_no_extension.ly" --padding 0,0

# convert to mp4 for final cut pro
ffmpeg -i "$filename_no_extension.avi" -c:v copy -c:a copy "$filename_no_extension.mp4"