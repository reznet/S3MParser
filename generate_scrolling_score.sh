#!/bin/zsh

set -e

path_to_me=${0:A:h}
filename=${1}
filename_no_extension=${filename:r}

# make sure we're running with the latest code
dotnet publish

# generate lilypond score
$path_to_me/S3mToMidi/bin/Release/net8.0/publish/S3mToMidi \
    --file "${filename}" \
    --exporter lilypond

# generate scrolling score
ly2video -i "$filename_no_extension.ly" --padding 0,0 --quality 1

# generate audio file
if [[ ! -a "$filename_no_extension.mp3" ]]; then
    # ffmpeg downloaded from https://evermeet.cx/ffmpeg/ and compiled with --enable-libmodplug
    /Users/jeff/Downloads/ffmpeg -y -i "$filename" "$filename_no_extension.mp3"
fi

# merge video and audio
/Users/jeff/Downloads/ffmpeg -y -i "$filename_no_extension.avi" -i "$filename_no_extension.mp3" -c:v copy -map 0:v:0 -map 1:a:0 "$filename_no_extension.mp4"
