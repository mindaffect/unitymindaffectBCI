using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using nl.ma.utopia;
using UnityEngine.Events;
using UnityEngine.UI;

[System.Serializable]
public class UnityEventGameObject : UnityEvent<GameObject> { }

public class NoisetagBehaviour : MonoBehaviour
{
    public int myobjID = -1;
    public int mystate = -1;
    public float myprob = -1;
    public bool isVisible = false;
    public Color flicker_color;
    public bool live_predictions = true;

    public UnityEvent selectedEvent;
    public UnityEventGameObject selectedObjectEvent;
    // Start is called before the first frame update
    void Start()
    {
        // TODO[] search for the color children of this object to be NT changed when started
        // to save findobjbytype stuff...
    }

    public void OnEnable()
    {
        isVisible = true;
    }

    public void OnBecameVisible()
    {
        isVisible = true;
    }

    public void acquireNoisetagObjID()
    {
        // acquire objID when visible
        if (myobjID < 0)
        {
            myobjID = NoisetagController.Instance.acquireObjID(this);
            Debug.Log("Acquired objID: " + myobjID);
        }
        mystate = -1;
    }

    public void OnDisable()
    {
        releaseNoisetagObjID();
    }

    public void OnBecameInvisible()
    {
        releaseNoisetagObjID();
    }

    public void releaseNoisetagObjID()
    {
        // release the objID
        if (myobjID > 0)
        {
            NoisetagController.Instance.releaseObjID(myobjID);
            Debug.Log("Released objID: " + myobjID);
            myobjID = -1;
        }
        mystate = -1;
        isVisible = false;
    }

    public void OnSelection()
    {
        // method called when this object is selected by the BCI
        
        Debug.Log("-------------- Selected: " + myobjID + "---------------------");
        // invoke our selection handler
        Debug.Log("Invoking:" + selectedEvent.ToString());
        selectedEvent.Invoke();
        selectedObjectEvent.Invoke(this.gameObject);
    }

    public void OnPrediction(float Perr)
    {
        // method called when this object is predicted by the BCI
        this.myprob = 1-Perr;
    }

    public void OnNewTarget()
    {
        this.mystate = -1;
        this.myprob = -1;
    }


    // Update is called once per frame
    public void Update()
    {
        if( myobjID<0 && isVisible)
        {
            acquireNoisetagObjID();
        }
        mystate = NoisetagController.Instance.getObjState(myobjID);
        flicker_color = getFlickerColor(mystate, myprob);
        // do nothing if not enabled/visible
        if (mystate < 0) return;


        updateRendererColor();
        updateButtonColor();
    }

    public Color getFlickerColor(int mystate=0, float myprob=-1)
    {
        // map state to color
        Color col = Color.black;
        if (mystate == 0)
        {
            col = Color.black;
        }
        else if (mystate == 1)
        {
            col = Color.white;
        }
        else if (mystate == 2)
        {
            col = Color.green;
        }
        else if (mystate == 3)
        {
            col = Color.blue;
        }

        // blend in the prediction confidence
        if ( live_predictions && myprob > .6f)
        { 
            col = Color.Lerp(col, Color.blue, myprob * .4f);
        }
        return col;
    }

    public void updateRendererColor()
    {
        Renderer r = gameObject.GetComponent<MeshRenderer>();
        if (r == null) return;

        // change the color of all material below this gameobject
        foreach (Material m in r.materials)
        {
            m.color = this.flicker_color;
        }
    }

    public void updateButtonColor()
    {
        // change the color of all material below this gameobject
        foreach (Image m in gameObject.GetComponents<Image>())
        {
            m.color = this.flicker_color;
        }
    }
}
