# unitymindaffectBCI
unity SDK for the MindAffect Brain Computer Interface

This repository contains 2 example unity projects showing how to use the unity game engine to present stimuli for an evoked-response Brain Computer Interface as part of a mindaffectBCI system.

The two sub-directories conatin:
 1. `minimal_presentation` : This is a very simple spelling BCI example showing how to use the unity plugin to connect to a running mindaffectBCI, how to perform a per-user system calibration, and how to use the trained model to allow the user to spell letters on an on-screen keyboard.   This is a good starting point for seeing how to integerate the BCI functionality with your own projects.
 2. `fps_shooter` : This is a more complex example showing how to integerate Brain Controls in an existing game framework.  Here the base unity tutorial game 'fps_shooter' has been modified by inclusion of the mindaffectBCI plugin to allow a new one-shot-kill *brain_shot* triggered by the BCI system.  After you have understood the minimal_presentation example this shows how to use the BCI in a more complex situation. 
