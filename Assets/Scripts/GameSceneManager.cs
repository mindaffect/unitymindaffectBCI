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

    // Start is called before the first frame update
    void Start()
    {
        NoisetagController nt = FindObjectOfType<NoisetagController>();
        // return to main menu when noise tag squence is done...
        nt.sequenceCompleteEvent.AddListener(GoMainMenu);
        nt.connectedEvent.AddListener(GoMainMenu);
        menuObject.SetActive(false);
        calibrationObject.SetActive(false);
        predictionObject.SetActive(false);
        signalQualityObject.SetActive(false);
        connectingObject.SetActive(false);
        // set the initial screen based on our connected state
        if (nt.isConnected())
        {
            GoMainMenu();
        } else {
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
    }

    public void GoConnecting()
    {
        setActiveObject(connectingObject);
    }

    public void GoCalibration()
    {
        setActiveObject(calibrationObject);
        // set all the scene child objects active?
        NoisetagController nt = FindObjectOfType<NoisetagController>();
        // make sure the flicker objects have active noisetag ids
        // N.B. this is only needed because we call startCalibration before the objects
        //      have been made visible and so get an ID themselves.
        //      If you activate the object first then this is *NOT* needed.
        nt.acquireObjIDs(activeObject.GetComponentsInChildren<NoisetagBehaviour>());
        // N.B. make sure the calibration objects are active before calling this
        //      otherwise can't do the cueing as don't know how many outputs there are.
        nt.startCalibration(10);
    }

    public void GoPrediction()
    {
        // N.B. 
        setActiveObject(predictionObject);
        NoisetagController nt = FindObjectOfType<NoisetagController>();
        // set all the scene child objects active?
        // make sure the flicker objects have active noisetag ids
        // N.B. this is only needed because we call startCalibration before the objects
        //      have been made visible and so get an ID themselves.
        //      If you activate the object first then this is *NOT* needed.
        //nt.acquireObjIDs(activeObject.GetComponentsInChildren<NoisetagBehaviour>());

        nt.startPrediction(10);
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
