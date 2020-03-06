using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using nl.ma.utopia;
using UnityEngine.Events;
using UnityEngine.UI;

public class NoisetagBehaviour : MonoBehaviour
{
    public int myobjID = -1;
    public int mystate = -1;
    public bool isVisible = false;

    public UnityEvent selectedEvent;
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
    }

    // Update is called once per frame
    public void Update()
    {
        if( myobjID<0 && isVisible)
        {
            acquireNoisetagObjID();
        }
        mystate = NoisetagController.Instance.getObjState(myobjID);
        // do nothing if not enabled/visible
        if (mystate < 0) return;


        updateRendererColor();
        updateButtonColor();
    }

    public void updateRendererColor()
    {
        Renderer r = gameObject.GetComponent<MeshRenderer>();
        if (r == null) return;
        // change the color of all material below this gameobject
        foreach (Material m in r.materials)
        {
            if (mystate == 0)
            {
                m.color = Color.black;
            }
            else if (mystate == 1)
            {
                m.color = Color.white;
            }
            else if (mystate == 2)
            {
                m.color = Color.green;
            }
            else if (mystate == 3)
            {
                m.color = Color.blue;
            }
        }
    }

    public void updateButtonColor()
    {
        // change the color of all material below this gameobject
        foreach (Image m in gameObject.GetComponents<Image>())
        {
            if (mystate == 0)
            {
                m.color = Color.black;
            }
            else if (mystate == 1)
            {
                m.color = Color.white;
            }
            else if (mystate == 2)
            {
                m.color = Color.green;
            }
            else if (mystate == 3)
            {
                m.color = Color.blue;
            }
        }
    }
}
