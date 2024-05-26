#!/bin/zsh

set -e

path_to_me=${0:A:h}
filename=${1}
filename_no_extension=${filename:r}

# make sure we're running with the latest code
dotnet publish

# generate lilypond score
$path_to_me/S3mToMidi/bin/Release/net8.0/publish/S3mToMidi \
    --exporter lilypond \
    --file $filename

lilypond -o $filename_no_extension $filename_no_extension.ly && open $filename_no_extension.pdf