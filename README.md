# Together, Tacit

Created as a part of a Spring 2022 Penn State Capstone project.
https://sites.psu.edu/lfshowcasesp22/2022/04/28/together-tacit-vr-glove/

## Description

The Together, Tacit project is a program designed to allow low-vision or blind invididuals to collaborate with sighted individuals on works of art. It uses an Oculus Quest to create a virtual work environment, utilizing haptic feedback and custom VR controllers to locate and manipulate a virtual sculpture.

## Getting Started

### Dependencies

* A VR-compatable computer (with Bluetooth compatability)
* Oculus Quest 2 (with compatable Oculus Link Cable)
* Windows 10
* At least 1 USB Type-C port and/or 1 USB Type-B port for Oculus Link
* At least 2 USB Type-B ports for custom VR controllers

### Installing

* The program can be run on any Windows 10 computer set up with Oculus Link, and does not require the custom controllers to function.
* For use with custom haptic controllers, see the details laid out inside the Software Instruction Manual.

## Authors

Noah Black - contact at noahblack0425@gmail.com

## Version History

* Beta 0.3v
    * Added a rectangular starting block, with more fine detail 0.1v
    * Added a reset button
    * Began work on haptic feedback from scuplture
    * Began work on enabling/disabling sculpting using Oculus controllers

* Beta 0.2v
    * New Plugin: Ardity
          * Used to make communication between Arduino software and Unity possible

* Beta 0.1v
    * Initialized project files based the "VR-Button" public files (see Acknowledgements below)
    * Added a simple block of removable cubes

* Release 1.0v
    * Full release of the Together, Tacit project, including the following options:
          * Virtual sculpting environment (2m x 2m x 2m)
          * Smooth sculpting, rendered in real time
          * Tools to carve, add, and interact with virtual sculptures
          * Options and customizations for play
          * .fbx file export for any sculptures made

## Acknowledgments

* [dwilches](https://github.com/dwilches/Ardity), for their work on the Ardity plugin
* [C-Through](https://github.com/C-Through/VR-Button), for their online references on the XR plugin and the initial template.
* [KellanHiggins](https://github.com/KellanHiggins/UnityFBXExporter), for their work on the UnityFBXExporter plugin.
* [Scrawk](https://github.com/Scrawk/Marching-Cubes), for their work on a version of Marching Cubes in Unity (used within this project).
