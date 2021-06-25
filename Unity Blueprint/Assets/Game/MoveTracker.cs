using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// Helps fix the jitteriness of Cinemachine cameras
/// </summary>
public class MoveTracker : MonoBehaviour
{
    public Transform target;    
    public bool move = true;
    public bool rotate = true;
    public bool alignWithGravity = true;
   
    // Update is called once per frame
    void Update()
    {
        if (target != null)
        {
            if (move) transform.position = target.transform.position;
            if (rotate) transform.rotation = target.transform.rotation;
            if (alignWithGravity) transform.rotation = Quaternion.LookRotation(Vector3.forward, -Physics.gravity.normalized);
        }
    }

    //private void LateUpdate()
    //{
    //    if (target != null)
    //    {
    //        if (move) transform.position = target.transform.position;
    //        if (rotate) transform.rotation = target.transform.rotation;
    //        if (alignWithGravity) transform.rotation = Quaternion.LookRotation(Vector3.forward, -Physics.gravity.normalized);
    //    }
    //}
}
