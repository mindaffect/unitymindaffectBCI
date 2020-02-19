using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using nl.ma.utopia;
using UnityEngine.Events;

public class NoisetagBehaviour : MonoBehaviour
{
    private NoisetagController nt = null;
    public int myobjID = -1;
    public int mystate = -1;

    public UnityEvent selectedEvent;
    // TODO: add an event for when the state is updated? rather than direct object change here?
    // Start is called before the first frame update
    void Start()
    {
        if (nt == null)
        {
            nt = FindObjectOfType<NoisetagController>();
        }
    }

    private void OnBecameVisible()
    {
        // acquire objID when visible
        if (myobjID < 0)
        {
            myobjID = nt.acquireObjID(this);
            Debug.Log("Acquired objID: " + myobjID);
        }
        mystate = -1;
    }

    public void OnBecameInvisible()
    {
        // release the objID
        if (myobjID > 0)
        {
            nt.releaseObjID(myobjID);
            Debug.Log("Released objID: " + myobjID);
            myobjID = -1;
        }
        mystate = -1;
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
    void Update()
    {
        mystate = nt.getObjState(myobjID);
        // do nothing if not enabled/visible
        if (mystate < 0) return;

        Renderer r = gameObject.GetComponent<MeshRenderer>();
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
}
