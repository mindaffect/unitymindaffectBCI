# unitymindaffectBCI
unity SDK for the MindAffect Brain Computer Interface

This repository contains 2 example unity projects showing how to use the unity game engine to present stimuli for an evoked-response Brain Computer Interface as part of a mindaffectBCI system.

The two sub-directories conatin:
 1. `minimal_presentation` : This is a very simple spelling BCI example showing how to use the unity plugin to connect to a running mindaffectBCI, how to perform a per-user system calibration, and how to use the trained model to allow the user to spell letters on an on-screen keyboard.   This is a good starting point for seeing how to integerate the BCI functionality with your own projects.
 2. `fps_shooter` : This is a more complex example showing how to integerate Brain Controls in an existing game framework.  Here the base unity tutorial game 'fps_shooter' has been modified by inclusion of the mindaffectBCI plugin to allow a new one-shot-kill *brain_shot* triggered by the BCI system.  After you have understood the minimal_presentation example this shows how to use the BCI in a more complex situation. 

# Note: Unity version 2019.4 (LTS)
These projects have been tested with unity versions up to 2019.4 (LTS), and ( fps_shooter in particular ) are known to have issues with later versions.   Thus, we recommend using version _2019.4.31f (LTS)_ with these projects.

# Note: Package Conflicts
When you first import this game to unity, if the versions do not match exactly, you may see lots of compilation errors.  These are most likely caused by package version conflicts.  The most likely culprits are Text Mesh Pro and VersionControl.   To solution is to go to the package manager (Window->Package Manager) and then update at least to versions compatible with your current unity version (the GUI will tell you which have been tested stable.)

# Note:
This repository only provides the unity specific parts of the system, to allow unity to provide the presentation part of a BCI.   To get a complete BCI working you will also need to install and setup the full mindaffectBCI system, which you can get from another github repository :

https://github.com/mindaffect/pymindaffectBCI

# For developers
I would suggest
 1. starting with the `pymindaffectBCI` system and getting that running first -- it already includes some simple BCI examples.   
 2. Then when you are comfortable running that, you can try integration with unity, using the minimal_presenation example, and modify this example, e.g. adding new keyboards, or buttons etc., to get familar how to add/remove BCI functionality.
 3. Then look at the fps_shooter example to see how to integerate the BCI in a more complex game, before moving on to design your own games.
