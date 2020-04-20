using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameSceneManager : MonoBehaviour
{

    public GameObject menuObject;
    public GameObject calibrationObject;
    public GameObject predictionObject;
    public GameObject connectingObject;
    public GameObject signalQualityObject;
    public GameObject activeObject = null;
    public int nCalibrationTrials=10;
    public int nPredictionTrials=10;

    // Start is called before the first frame update
    void Start()
    {
        NoisetagController nt = NoisetagController.Instance;
        // N.B. add these listeners from the GUI
        //nt.sequenceCompleteEvent.AddListener(GoMainMenu);
        //nt.connectedEvent.AddListener(GoMainMenu);
        //nt.startPrediction(1); // test starting prediction early + button selection

        menuObject.SetActive(false);
        calibrationObject.SetActive(false);
        predictionObject.SetActive(false);
        signalQualityObject.SetActive(false);
        connectingObject.SetActive(false);
        // set the initial screen based on our connected state
        if (nt.isConnected())
        {
            GoMainMenu();
        }
        else
        {
            GoConnecting();
        }
    }
    // Update is called once per frame
    void Update()
    {
    }

    void setActiveObject(GameObject go)
    {
        if (activeObject != null) activeObject.SetActive(false);
        activeObject = go;
        activeObject.SetActive(true);
    }

    public void GoMainMenu()
    {
        setActiveObject(menuObject);
        NoisetagController.Instance.stopFlicker();
    }

    public void GoConnecting()
    {
        setActiveObject(connectingObject);
    }

    public void GoCalibration()
    {
        setActiveObject(calibrationObject);
        // make sure the flicker objects have active noisetag ids
        // N.B. this is only needed because we call startCalibration before the objects
        //      have been made visible and so get an ID themselves.
        //      If you activate the object first then this is *NOT* needed.
        NoisetagController nt = NoisetagController.Instance;
        nt.acquireObjIDs(activeObject.GetComponentsInChildren<NoisetagBehaviour>());
        // N.B. make sure the calibration objects are active before calling this
        //      otherwise can't do the cueing as don't know how many outputs there are.
        nt.startCalibration(nCalibrationTrials);
    }

    public void GoPrediction()
    {
        // N.B. 
        setActiveObject(predictionObject);
        // set all the scene child objects active?
        // make sure the flicker objects have active noisetag ids
        // N.B. this is only needed because we call startCalibration before the objects
        //      have been made visible and so get an ID themselves.
        //      If you activate the object first then this is *NOT* needed.
        //nt.acquireObjIDs(activeObject.GetComponentsInChildren<NoisetagBehaviour>());

        NoisetagController.Instance.startPrediction(nPredictionTrials);
    }

    public void GoSignalQuality()
    {
        Debug.Log("GoSignalQuality!!!");
        setActiveObject(signalQualityObject);
    }

    public void GoQuit()
    {
        Application.Quit();
    }

}
