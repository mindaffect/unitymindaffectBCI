using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ntstart : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        NoisetagController.Instance.startCalibration(100);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
