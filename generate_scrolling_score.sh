#!/bin/zsh

set -e

path_to_me=${0:A:h}
filepath=${1}
filename=${filepath:t}
filename_no_extension=${filename:r}
output_dir=$path_to_me/output
temp_dir=$path_to_me/temp
schismtracker='/Volumes/Macintosh HD/Applications/Schism Tracker.app/Contents/MacOS/schismtracker'

mkdir -p $output_dir
mkdir -p $temp_dir

# make sure we're running with the latest code
dotnet publish

# generate lilypond score
$path_to_me/S3mToMidi/bin/Release/net8.0/publish/S3mToMidi \
    --exporter lilypond \
    --output "$temp_dir/$filename_no_extension.ly" \
    --file $@

# generate scrolling score
ly2video -i "$temp_dir/$filename_no_extension.ly" --padding 0,0 --quality 1

# generate audio file
if [[ ! -a "$temp_dir/$filename_no_extension.wav" ]]; then
    $schismtracker --play --diskwrite="$temp_dir/$filename_no_extension.wav" "$filepath"
fi

# merge video and audio
/Users/jeff/Downloads/ffmpeg -y \
    -i "$temp_dir/$filename_no_extension.avi" \
    -i "$temp_dir/$filename_no_extension.wav" \
    -c:v libx264 -crf 0 -pix_fmt yuv420p \
    -c:a aac_at \
    -map 0:v:0 -map 1:a:0 \
    "$output_dir/$filename_no_extension.mp4"
