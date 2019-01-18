#!/bin/bash

create_audio () {
	say $1 -o $2.aiff
	lame -m m $2.aiff $2.mp3
	rm -f $2.aiff
}

create_audio "Binocular Suppression" "binocularSuppression"
create_audio "Strabism Detection" "strabismDetection"
create_audio "Misalignment Measurement" "misalignmentMeasurement"
create_audio "Depth Perception" "depthPerception"
create_audio "Endless Runner" "endlessRunner"
