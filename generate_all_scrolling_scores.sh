#!/bin/zsh

set -e

files=('songs/v-blast.s3m' 'songs/v-drac.s3m')

for file in $files; do
    echo $file
    ./generate_scrolling_score.sh $file;
done