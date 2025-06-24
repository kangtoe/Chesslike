using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FaceToCam : MonoBehaviour
{
    [SerializeField]
    bool startOnly = true;

    void Start()
    {
        FaceCam();
    }

    void Update() // update works better than LateUpdate, but It should be done in LateUpdate...
    {
        if (startOnly) return;
        FaceCam();
    }

    void FaceCam()
    {
        transform.forward = Camera.main.transform.forward;
        //transform.rotation = Quaternion.LookRotation( transform.position - cam.position );
    }
}
