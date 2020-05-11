mindaffectBCI
=============

This repository contains the [unity](unity.com) SDK code for the Brain Computer Interface (BCI) developed by the company [Mindaffect](https://mindaffect.nl).

File Structure
--------------
This repository is organized roughly as follows:

 - `Assets ` - contains the assests for this unity project.  Important parts within this are:
   - Scripts - contains the C# scripts which actually do most of the work.  Within this you have
     - Noisetag - which contains the C# interface to the mindaffect Decoder.  This is a direct copy of the [C# SDK from](github.com/mindaffect/csharpmindaffectBCI)
     - NoisetagController.cs - this script is the main unity object for managing the decoder connection
     - NoisetagBehaviour.cs - this script contains the behaviour that allows a GameObject to be controlled by the BCI
     - GameManager.cs - this script is the main manager for the different phase of the game, e.g. switching between calibration and prediction
   - Models - the 3d models
   - doc - contains general documentation for developers on the architecture of the mindaffectBCI and the low-level networking protocols it uses.  (Whilst useful to give an overview, ideally you should not need to read this.)

Installing mindaffectBCI
------------------------

That's easy, download this repository and launch the project with unity.

Support
-------

[![Join the chat at https://gitter.im/mindaffect/unitymindaffectBCI](https://badges.gitter.im/mindaffect/unitymindaffectBCI.svg)](https://gitter.im/mindaffect/unitymindaffectBCI?utm_source=badge&utm_medium=badge&utm_campaign=pr-badge&utm_content=badge)



Getting Support
---------------

If you run into and issue you can either directly raise an issue on the projects [github page](https://github.com/mindaffect/pymindaffectBCI) or directly contact the developers on [gitter](https://gitter.im/mindaffect) -- to complain, complement, or just chat:
[![badge](https://badges.gitter.im/mindaffect/unitymindaffectBCI.svg](https://gitter.im/mindaffect/pymindaffectBCI?utm_source=badge&utm_medium=badge&utm_campaign=pr-badge&utm_content=badge)

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

The  detailed  system architeture of the mindaffecBCI is explained in more detail in [doc/Utopia _ Guide for Implementation of new Presentation and Output Components.pdf](https://github.com/mindaffect/pymindaffectBCI/blob/master/doc/Utopia%20_%20Guide%20for%20Implementation%20of%20new%20Presentation%20and%20Output%20components.pdf), and is illustrated in this figure:
![doc/SystemArchitecture.png](https://github.com/mindaffect/pymindaffectBCI/blob/master/doc/SystemArchitecture.png)



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
