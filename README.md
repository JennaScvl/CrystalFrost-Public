# Crystal Frost Experimental
Open source Unity 2021.3.6f1 LTS based Second Life viewer using LibreMetaverse.

Crystal Frost is unity based a homebrew client for Second Life and Open Sim.

# Current status
Recent updates. as of Sept 17, 2022
Textures, sculpts, and meshes are processed by an external thread before being fed into the main thread. This has greatly improved performance.
HOWEVER, the client is eating an inordinate amount of ram to do its thing, and I highly suspect it's the way I'm creating meshes and/or feeding them into the renderer.

Back end is using a lightly modified version of libraMetaverse, that's been integrated directly into the Unity project, rather than calling on it as a library.

Currently it's set up to only log into agni rather than any Open Sim grid, but this is not a design choice, it's just me being lazy because I'm an Agni Peasant. If anyone wants to submit a working grid selection UI element to the login screen, I'll happily integrate it.

There is no working build, but the source code does work. It lets you log in and renders objects within a 32meter radius around the main camera. It also hogs memory like crazy, which is the current issue I'm trying to fix and would absolutely LOVE help from someone who's better with code than I am at fixing this.

# Joining the Project
Also please contact on SL as Berry Bunny, or on Discord as .Kallisti if you want to be added as a contributor and help out in a more direct manner.

# License.
Save for the code in the libreMetaverse folder, neither binaries nor source may be distributed in part nor in whole except with those who are in the project's team.

This project uses code licensed under the BSD license as well as proprietary code.
