#!/bin/zsh

set -e

path_to_me=${0:A:h}
filename=${1}
filename_no_extension=${filename:r}
schismtracker='/Volumes/Macintosh HD/Applications/Schism Tracker.app/Contents/MacOS/schismtracker'

# make sure we're running with the latest code
dotnet publish

# generate lilypond score
$path_to_me/S3mToMidi/bin/Release/net8.0/publish/S3mToMidi \
    --file "${filename}" \
    --exporter lilypond

# generate scrolling score
ly2video -i "$filename_no_extension.ly" --padding 0,0 --quality 1

# generate audio file
if [[ ! -a "$filename_no_extension.wav" ]]; then
    $schismtracker --play --diskwrite="$filename_no_extension.wav" "$filename"
fi

# merge video and audio
/Users/jeff/Downloads/ffmpeg -y -i "$filename_no_extension.avi" -i "$filename_no_extension.wav" -c:v libx264 -crf 0 -pix_fmt yuv420p -map 0:v:0 -map 1:a:0 -c:a aac_at "$filename_no_extension.mp4"
