using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// Helps fix the jitteriness of Cinemachine cameras
/// </summary>
public class MoveTracker : MonoBehaviour
{
    public Transform target;
    public float lerpSpeed = 0.5f;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (target != null)
        {
            transform.position = Vector3.Lerp(transform.position, target.transform.position, lerpSpeed);
        }
    }
}
