using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using nl.ma.utopia;
using nl.ma.utopiaserver.messages;
using UnityEngine.Events;

/**
 * Unity Controller for interfacing with the MindAffect brain decoder
 * box.  Basically this is just a unity specific wrapper round the
 * general .Net NoiseTag.cs brain decoder box.  It adds some
 * functionality to allow individual objects to dynamically register
 * and deregister themselvees as under bci control, and some callback degelates for important events in managing the noise tags, such as when a sequence finishes.   Finrally, (and most importantly) it tracks the *actual* frame render times with high precision so we get a good time-lock for the brain response -- which is important for the BCI performance.
 */

/* typed events */
[System.Serializable] public class NewMessagesEventType : UnityEvent<List<UtopiaMessage>> { };
[System.Serializable] public class NewPredictionEventType : UnityEvent<PredictedTargetProb> { };
[System.Serializable] public class SelectionEventType : UnityEvent<int> { };
[System.Serializable] public class SignalQualityEventType : UnityEvent<float[]> { };

public class NoisetagController : MonoBehaviour
{
    // make an event systemfor relevant noise tag events..
    public UnityEvent connectedEvent;
    public UnityEvent lostConnectionEvent;
    public UnityEvent sequenceCompleteEvent;
    public UnityEvent newTargetEvent;
    public NewMessagesEventType newMessagesEvent;
    public NewPredictionEventType newPredictionEvent;
    public SelectionEventType selectionEvent;
    public SignalQualityEventType signalQualityEvent;

    public string decoderAddress = null;
    public bool isRunning = false;
    private bool wasRunning = false;
    public Noisetag nt;
    public int nframe;
    static public int ISI = 60;
    public long lastframetime;
    public bool target_only = false;
    public bool connect_loop_running = false;
    public long last_connected_time = 0;
    public long lostConnectionTimeout_ms = 5000; // register disconnection if trying for 5s
    public bool frametime_loop_running = false;
    // singlenton pattern....
    public static NoisetagController instance = null;
    public TextAsset codebook = null;
    public StimulusState stimulusState = null;
    private NoisetagBehaviour[] registeredobjIDs = null;
    // TODO: make the set of objIDs we use a configuration option -- for use with other pres systems.
    private int[] objIDs = null;
    // number of video frames per noisetag codebit
    public float targetFrameRate = 60.0f; 
    public int FRAMESPERCODEBIT = -1;

    public float selectionThreshold = .1f;
    public int prediction_frames = ISI*10;
    public int calibration_frames= (int)(ISI*4.2);


    // singlenton field
    private static NoisetagController _instance;

    // singlenton accessor field
    public static NoisetagController Instance
    {
        get { return _instance; }
    }

    // Awake is called **before** any Start methods.
    // so we make sure the NoiseTag controller is ready before
    // any game objects which may want to use it!
    void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(this.gameObject);
            return;
        }
        _instance = this;
        DontDestroyOnLoad(this.gameObject); // keep the controller arround...

        // Switch to 640 x 480 full-screen at 60 hz, and put
        // VSYNC on, so we a) run fast, b) are time-accurate.
        Screen.SetResolution(Screen.width, Screen.height, FullScreenMode.ExclusiveFullScreen);
        // FORCE!! Sync framerate to monitors refresh rate
        // get the current display rate, and set the vSyncCount to get as close as possible
        // for the code bits
        QualitySettings.vSyncCount = 1;
        if (FRAMESPERCODEBIT <= 0) // set framesperbit to be close to target framerate
        {
            int refreshRate = Screen.currentResolution.refreshRate;
            float framesperbit = refreshRate / QualitySettings.vSyncCount / targetFrameRate;
            FRAMESPERCODEBIT = (int) Math.Round(framesperbit + .1);
        }

        nt = new Noisetag(new System.IO.StringReader(codebook.text), null, null);

        // setup the event handlers when the connection is up.
        // debug message handler : prints all new messages
        nt.stopFlicker(); // reset state
        nt.addMessageHandler(newMessageHandler);
        nt.addSelectionHandler(selectionHandler);
        nt.addPredictionHandler(newPredictionHandler);
        nt.addSignalQualityHandler(signalQualityHandler);
        nt.addNewTargetHandler(newTargetHandler);
        nt.setFramesPerBit(FRAMESPERCODEBIT);

        // init the info on the set of objIDs we are managing..
        if (objIDs == null)
        {
            objIDs = new int[127];
            for (int i = 0; i < objIDs.Length; i++)
            {
                objIDs[i] = i + 1; // N.B. objid > 0 
            }
        }
        if (registeredobjIDs == null)
        {
            registeredobjIDs = new NoisetagBehaviour[objIDs.Length];
        }

        nframe = 0;
        isRunning = false;

        // magic co-routine to record accurately the time the last frame was drawn
        StartCoroutine(recordFrameTime());

        // magic co-routine to make and maintain the decoder connection
        last_connected_time = nt.getTimeStamp();
        StartCoroutine(KeepTryingToConnect());
    }

    // Keep trying to connect until successful
    public IEnumerator KeepTryingToConnect()
    {
        // ensure is singleton
        if (connect_loop_running)
        {
            Debug.Log("Connection management loop already running!");
            yield break;
        }
        // run forever, so auto-reconnect if connection is dropped
        while (true)
        {
            connect_loop_running = true;
            if (!nt.isConnected())
            {
                tryToConnect(100);
            } else
            {
                last_connected_time = nt.getTimeStamp();
            }
            // check again in .5s
            yield return new WaitForSeconds(.5f);
        }
        connect_loop_running = false;
    }

    public void tryToConnect(int timeout_ms)
    {
        // TODO [x] : This is evil as it blocks the main graphics thread
        //     should refactor nt into a co-routine? to not block in this way...
        Debug.Log("Trying to connect to : " + decoderAddress);
        // TODO [] : Add a user query if the connection doesn't work for too long.
        nt.connect(decoderAddress, -1, timeout_ms);
        if (nt.isConnected())
        {
            this.decoderAddress = nt.getHostPort();
            Debug.Log("Connected to " + this.decoderAddress);
            nt.modeChange("idle");
            if ( connectedEvent != null ) connectedEvent.Invoke();
        } else if (  nt.getTimeStamp() > last_connected_time + lostConnectionTimeout_ms)
        {
            if (lostConnectionEvent != null) lostConnectionEvent.Invoke();
        }
    }

    public bool isConnected()
    {
        if (this.nt != null)
        {
            return nt.isConnected();
        }
        return false;
    }

    // Modify the decoder address we are trying to connect to.
    public void setDecoderAddress(string newaddress)
    {
        if (isConnected())
        {
            Debug.Log("Warning: already connected....");
        }
        decoderAddress = newaddress;
        last_connected_time = nt.getTimeStamp(); // restart the connection timeout clock
        StartCoroutine(KeepTryingToConnect());
    }

    public void modeChange(string newmode)
    {
        this.nt.modeChange(newmode);
    }
    public float[] getLastSignalQuality()
    {
        return nt.getLastSignalQuality();
    }

    public void startExpt(int nCal = 10)
    {
        startExpt(nCal, 10, selectionThreshold);
    }
    public void startExpt(int nCal, int nPred)
    {
        startExpt(nCal, nPred, selectionThreshold);
    }
    public void startExpt(int nCal, int nPred, float selectionThreshold)
    {
        nt.startExpt(nCal, nPred, selectionThreshold);
    }

    public void startCalibration(int nTrials = 10)
    {
        target_only = false;
        nt.startCalibration(nTrials, null, calibration_frames);
    }
    public void startSimpleCalibration(int nTrials = 10)
    {
        target_only = true;
        nt.startCalibration(nTrials);
    }

    public void startPrediction()
    {
        startPrediction(10);
    }
    public void startPrediction(int nTrials)
    {
        startPrediction(nTrials, false);
    }
    public void startCuedPrediction(int nTrials=10)
    {
        target_only = false;
        nt.startPrediction(nTrials, null, true, selectionThreshold, prediction_frames);
    }
    public void startPrediction(int nTrials, bool cuedPrediction)
    {
        target_only = false;
        nt.startPrediction(nTrials,null,cuedPrediction, selectionThreshold, prediction_frames);
    }
    public void startFlickerWithSelection(float duration=10)
    {
        startFlickerWithSelection(duration,selectionThreshold);
    }
    public void startFlickerWithSelection(float duration, float selectionThreshold, int tgtidx = -1)
    {
        nt.startFlickerWithSelection((int)(duration / ISI), tgtidx, true, selectionThreshold);
    }
    public void stopFlicker()
    {
        nt.stopFlicker();
    }

    // add a coroutine to record exact time-stamps for the frame rendering..
    public IEnumerator recordFrameTime()
    {
        // ensure is singlenton recording loop
        if (frametime_loop_running)
        {
            Debug.Log("Frametime recorder already running!");
            yield break;
        }
        while (true) // so we never terminate
        {
            frametime_loop_running = true;
            yield return new WaitForEndOfFrame();
            lastframetime = nt.getTimeStamp();
        }
        frametime_loop_running = false;
    }

    public void newMessageHandler(List<UtopiaMessage> msgs)
    {
        // simple message handler to test callbacks
        Debug.Log(String.Format("Got {0:d} messages:", msgs.Count));
        foreach (UtopiaMessage m in msgs)
        {
            Debug.Log(m);
        }
        // make a general unity event
        if (newMessagesEvent != null) newMessagesEvent.Invoke(msgs);
    }
    public void newPredictionHandler(PredictedTargetProb m)
    {
        int objIdx = getObjIdx(this.objIDs, m.Yest);
        if (objIdx >= 0) // one of ours
        {
            NoisetagBehaviour obj = registeredobjIDs[objIdx];
            if (obj != null) obj.OnPrediction(m.Perr);
        }
        if (newPredictionEvent != null) newPredictionEvent.Invoke(m);
    }
    public void signalQualityHandler(float[] qualities)
    {
        if (signalQualityEvent != null) signalQualityEvent.Invoke(qualities);
    }
    public void selectionHandler(int objID)
    {
        int objIdx = getObjIdx(this.objIDs, objID);
        if (objIdx >= 0) // one of ours
        {
            NoisetagBehaviour selobj = registeredobjIDs[objIdx];
            if ( selobj !=null ) selobj.OnSelection();
        }
        if (selectionEvent != null) selectionEvent.Invoke(objID);
    }
    public void newTargetHandler()
    {
        foreach ( NoisetagBehaviour obj in registeredobjIDs)
        {
            if ( obj != null )
                obj.OnNewTarget();
        }
    }


    // Update is called once per frame
    string logstr = "";

    void Update()
    {
        // don't bother if not connected to decoder...
        if (!this.nt.isConnected())
        {
            //Debug.Log("Noise-tag is not connected!");
            return;
        }

        nframe++;
        // don't update the code-stuff if in slowdown mode..
        //if (nframe % Math.Max(FRAMESPERCODEBIT,1) != 0) return; 

        wasRunning = isRunning;
        isRunning = this.nt.updateStimulusState(nframe);
        if (!isRunning)
        {
            if (wasRunning && sequenceCompleteEvent!=null) // we have just finished a nt sequence
            {
                sequenceCompleteEvent.Invoke();
            }
            return;
        }
        stimulusState = nt.getStimulusState();
        //if (stimulusState != null ) Debug.Log(stimulusState);
        if (stimulusState != null && stimulusState.target_idx >= 0 && stimulusState.target_idx < stimulusState.stimulusState.Length)
        {
            logstr += stimulusState.stimulusState[stimulusState.target_idx] > 0 ? "*" : ".";
        }
        else
        {
            logstr += "_";
        }
        if (logstr.Length > 60)
        {
            Debug.Log(logstr); logstr = "";
        }
        nt.sendStimulusState(lastframetime);
    }


    //-----------------------------------------------------------------------
    // code to manage the objIDs acquire/release
    private int findUnusedObjIdx()
    {
        for (int i = 0; i < registeredobjIDs.Length; i++)
        {
            if (!registeredobjIDs[i])
            {
                return i;
            }
        }
        return -1;
    }

    public int[] acquireObjIDs(NoisetagBehaviour[] objs)
    {
        List<int> objIDs = new List<int>();
        foreach (NoisetagBehaviour o in objs)
        {
            int id = acquireObjID(o);
            objIDs.Add(id);
        }
        return objIDs.ToArray();
    }

    internal int acquireObjID(NoisetagBehaviour gameobj)
    {
        // check if already registered
        // TODO []: correct search that the object reference also matches...
        int objID = -1;
        int objIdx = -1;
        if (gameobj.myobjID == 0)
        {
            // BODGE: [] Zero id is special and *cannot* be acquired or released!
            objID = gameobj.myobjID;
            objIdx = -1;
        }
        else if (gameobj.myobjID > 0)
        {
            objID = gameobj.myobjID;
            objIdx = getObjIdx(objIDs, gameobj.myobjID);
            if (registeredobjIDs[objIdx] == gameobj)
            {
                Debug.Log("Obj already registered as: " + gameobj.myobjID);
                return objID;
            }
        }
        else
        {
            objIdx = findUnusedObjIdx();
            objID = objIDs[objIdx];
        }
        if (objIdx >= 0)
        {
            // mark this ID as used
            registeredobjIDs[objIdx] = gameobj;
            // tell the object that it's got an ID
            gameobj.myobjID = objID;
            // the the nt-controller it's used
            updateActiveObjIDs();
        }
        return objID;
    }

    private int getObjIdx(int[] objIDs, int objID)
    {
        for (int i = 0; i < objIDs.Length; i++)
        {
            if (objIDs[i] == objID) return i;
        }
        return -1;
    }

    internal int releaseObjID(int myobjID)
    {
        if (myobjID > 0)
        {
            // search for the index of this objID
            int objIdx = getObjIdx(objIDs, myobjID);
            if (objIdx >= 0 && registeredobjIDs[objIdx] != null)
            {
                // mark as free
                registeredobjIDs[objIdx] = null;
                // update the active set
                updateActiveObjIDs();
            }
        }
        return -1;
    }

    internal int getObjState(int myobjID)
    {
        if (stimulusState != null)
        {
            if (myobjID == 0)
            {
                int targetState = stimulusState.target_idx >= 0 && stimulusState.target_idx < stimulusState.stimulusState.Length ? stimulusState.stimulusState[stimulusState.target_idx] : -1;
                // target is special -- only is on/off
                return targetState == 1 ? 1 : 0;
            }
            else
            {
                int objIdx = getObjIdx(stimulusState.objIDs, myobjID);
                if ( target_only && objIdx != stimulusState.target_idx)
                {
                    // in target only mode, only the stim with idx matching the target gets a state
                    return 0;
                }
                if (objIdx >= 0 )
                {
                    return stimulusState.stimulusState[objIdx];
                }
            }
        }
        return -1;
    }

    public int[] getActiveObjIDs()
    {
        // extract the set of objIDs current active from the bit-field
        List<int> activeObjIDs = new List<int>();
        for (int i = 0; i < registeredobjIDs.Length; i++)
        {
            if (registeredobjIDs[i] != null) activeObjIDs.Add(objIDs[i]);
        }
        return activeObjIDs.ToArray();
    }
    private void updateActiveObjIDs()
    {
        nt.setActiveObjIDs(getActiveObjIDs());
    }


}
