mindaffectBCI
=============

This repository contains the [unity](unity.com) SDK code for the Brain Computer Interface (BCI) developed by the company [Mindaffect](https://mindaffect.nl).

File Structure
--------------
This repository is organized roughly as follows:

 - `Assets\FPS` - contains the assests for this unity project.  Important parts within this are:
   - Scripts - contains the C# scripts which actually do most of the work.  Within this you have
     - Noisetag - which contains the C# interface to the mindaffect Decoder.  This is a direct copy of the [C# SDK from](github.com/mindaffect/csharpmindaffectBCI)
     - NoisetagController.cs - this script is the main unity object for managing the decoder connection.  Attach this script to an invisible object at the root of your gam hierarchy, to enable the brain-based interaction for your game.
     - NoisetagBehaviour.cs - this script contains the behaviour that allows a GameObject to be controlled by the BCI.  Attach this script to a game object to enable BCI control for that object.
   - Scenes - the scenes for this game.  In paticular, the `MainScene.unity` contains the main scene, which should be the root of games scene hierarchy.
   - Models - the 3d models

Installing mindaffectBCI
------------------------

That's easy, download this repository and launch the project with unity.

Note: It may be that when you open the project in unity there is no _MainScene_ in the Hierarchy.  If that is the case then:
 1. Delete the Camera and Directional Light
 2. In the project browser navigate to: Assests->FPS->Scenes
 3. Copy MainScene.unity into the Hiearchy
You should now have a full scene tree in the hierarch, with a MainScene, Camera, ntcontroller etc.

### Note: Unity version 2019.4 (LTS)
These projects have been tested with unity versions up to 2019.4 (LTS), and ( fps_shooter in particular ) are known to have issues with later versions.   Thus, we recommend using version _2019.4.31f (LTS)_ with these projects.

### Note: Package Conflicts
When you first import this game to unity, if the versions do not match exactly, you may see lots of compilation errors.  These are most likely caused by package version conflicts.  The most likely culprits are Text Mesh Pro and VersionControl.   To solution is to go to the package manager (Window->Package Manager) and then update at least to versions compatible with your current unity version (the GUI will tell you which have been tested stable.)


Getting Support
---------------

If you run into and issue you can either directly raise an issue on the projects [github page](https://github.com/mindaffect/unitymindaffectBCI) 

Testing the mindaffectBCI SDK
-----------------------------

This SDK provides the functionality needed to add Brain Controls to your own applications.  However, it *does not* provide the actual brain measuring hardware (i.e. EEG) or the brain-signal decoding algorithms. 

In order to allow you to develop and test your Brain Controlled applications without connecting to a real mindaffect Decoder, we provide a so called "fake recogniser".  This fake recogniser simulates the operation of the true mindaffect decoder to allow easy development and debugging.  Before starting with the example output and presentation modules.  You can download the fakerecogniser from our [github page](https://github.com/mindaffect/pymindaffectBCI/tree/master/bin)

You should start this fake recogniser by running, either ::
```
  bin/startFakeRecogniser.bat
```  
if running on windows, or  ::
```
  bin/startFakeRecogniser.sh
```
if running on linux/macOS

If successfull, running these scripts should open a terminal window which shows the messages recieved/sent from your example application.

Note: The fakerecogniser is written in [java](https://www.java.com), so you will need a JVM with version >8 for it to run.  If needed download from [here](https://www.java.com/ES/download/)

Quick BCI Test
--------------

When you have imported this project into unity, you can just run it to test the BCI. Note: to ensure display timing accuracy we strongly encourage that you run the application as a stand-alone application.  Whilst running within the unity editor seems to work, it's less reliable.


System Overview
---------------

The mindaffectBCI consists of 3 main pieces:

 - *decoder* : This piece runs on a compute module (the raspberry PI in the dev-kit), connects to the EEG amplifer and the presentation system, and runs the machine learning algorithms to decode a users intended output from the measured EEG.

 - *presentation* : This piece runs on the display (normally the developers laptop, or tablet)), connects to the decoder, and shows the user interface to the user,  with the possible flickering options to pick from.

 - *output* : This piece, normally runs on the same location as the  presentation, but may be somewhere else, and also connects to the decoder.  It listens from 'selections' from the decoder, which indicate that the decoder has decided the user want's to pick a particular option,  and makes that  selection happen -- for example by adding a letter to the current sentence, or moving a robot-arm,  or turning on or off a light.

The  detailed  system architeture of the mindaffecBCI is explained in more detail in [doc/Utopia _ Guide for Implementation of new Presentation and Output Components.pdf](https://github.com/mindaffect/unitymindaffectBCI/blob/master/doc/Utopia%20_%20Guide%20for%20Implementation%20of%20new%20Presentation%20and%20Output%20components.pdf), and is illustrated in this figure:
![doc/SystemArchitecture.png](https://github.com/mindaffect/unitymindaffectBCI/blob/master/doc/SystemArchitecture.png)



Simple *presention* module
----------------------------

To use this code in your own games, follow these steps:
  1. Copy the Scripts directory from the Assests directory of this project into your game.
  2. Create a new empty game-object in the base of your game, and attache the `NoiseController.cs` script to this object.
  3. Create the game objects you want to BCI control, and attach the `NoisetagBehaviour.cs` script to them.
  4. For each BCI controlled game-object you have, in the editor define the code you want to execute when it is selected.
  5. In your main game manager, tell the noise-tag-controller to go into the correct mode. e.g.
     For Calibration mode, with 10 calibration trials of about 4 seconds use
```
        FindObjectOfType<NoisetagController>().startCalibration(10)
```

     Or for Prediction mode, for 10 selections. use::
```
        FindObjectOfType<NoisetagController>().startPrediction(10)
```	
