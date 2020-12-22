using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using nl.ma.utopiaserver;
using nl.ma.utopiaserver.messages;

// Main Class to manage a utopia BCI connection and the operating phases, Calibrate, Feedback, Prediction etc.
public class SignalQualityScreen : MonoBehaviour
{
    public GameObject sigQualObject;
    private GameObject[] qualArray;
    public float[] signalQualities;
    //  private NoisetagController nt = null;
    // Start is called before the first frame update
    void Start()
    {
    }

    // N.B. can't use onVisible/onInvisible as this doens't work for canvas objects...
    public void OnEnable()
    {
        NoisetagController.Instance.modeChange("ElectrodeQuality");
    }
    public void OnDisable()
    {
        NoisetagController.Instance.modeChange("idle");
    }

    void update_nch(int nch)
    {
        // get the edges of the window
        Camera cam = FindObjectOfType<Camera>();
        // get the x/y edges of the viewport in 3-d coords.
        Vector3 topleft = cam.ViewportToWorldPoint(new Vector3(0, 1, 10));
        Vector3 botright = cam.ViewportToWorldPoint(new Vector3(1, 0, 10));
        float x = (botright.x + topleft.x) / 2f;
        float y = (botright.y + topleft.y) / 2f;
        float z = (botright.z + topleft.z) / 2f;
        float w = System.Math.Abs(botright.x - topleft.x);
        float h = System.Math.Abs(botright.y - topleft.y);
        float stepx = w / (nch + 1);
        float step = stepx;
        qualArray = new GameObject[nch];
        for (int i = 0; i < nch; i++)
        {
            // N.B. objects are positioned relative to the *CENTER* of the object.
            Vector3 newPos = new Vector3(topleft.x + (i + 1) * step, y, z);
            qualArray[i] = Instantiate(sigQualObject, newPos, Quaternion.identity, transform);
            qualArray[i].SetActive(true);
            // TODO [] : put the channel label text in front
        }
    }

    // Update is called once per frame?
    void Update()
    {
        // TODO[] : switch to using the signalQuality event?

        signalQualities = NoisetagController.Instance.getLastSignalQuality();
        if (signalQualities == null) return;
        if (qualArray == null ||
            signalQualities.Length != qualArray.Length)
        {
            // update the channel montage
            update_nch(signalQualities.Length);
        }

        for (int i = 0; i < signalQualities.Length; i++)
        {
            GameObject go = qualArray[i];
            float qual = signalQualities[i];
            // red=bad, green=good
            Color qualcolor = new Color(255 * qual, 255 * (1 - qual), 0);
            Renderer r = go.GetComponent<MeshRenderer>();
            // change the color of all material below this gameobject
            foreach (Material m in r.materials)
            {
                m.color = qualcolor;
            }
        }
    }

}
