## What is this?

The EyeSkills Framework is designed to ease the development of apps related to the visual system. In particular, we are interested in Amblyopia and Strabismus.

In addition to the core framework, it includes some example scenarios showing how it can be used.

Take a look at [EyeSkills.org](www.eyeskills.org) for more info!

## What will I need to make this work?

The framework is written in Unity (which you can get here).  We recommend that you install Unity version 2018.2.9f1.

You’ll also need to have (or develop) some familiarity with C# and the Unity API.

On the hardware side : this has been tested with google cardboard style viewers (we quite like the Hamswan 3D VR Viewer) and Samsung/LG and Sony smartphones. Pretty much any VR compatible phone should work (which basically means it needs an integrated gyro).

Finding affordable and reliable blue-tooth controllers has been a challenge. We currently recommend just using a cheap bluetooth keyboard.

## How do I use it

Some initial API documentation is here :

https://api.eyeskills.org

This only shows public methods - for now you'll have to dig into the code a little.  More tutorials showing how the parts fit together will be coming soon.

## Why have we done this?

We’ve done this so that the community interested in eye disorders can more easily explore their own visual system and experiment with the potential of new devices.

## What do we ask of you?

The code is licensed under the GNU AGPL v3.  In plain text, if you release changes based on, or add to, this code, you must share those changes with the community.  By the community, for the community.

## How was this possible?

This started off (and still is) an amateur project.  We are very grateful for the support of Social Impact Lab in Leipzig, and the equally wonderful PrototypeFund for helping us keep the project moving.

## GOTCHAS

Be careful that you have assigned permissions for the webcam to the app. If this isn't done, it will crash without warning why trying to access the Hyperreal/VirtuallyReal realignment scene.
