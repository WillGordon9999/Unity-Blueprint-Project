using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveReference : MonoBehaviour
{
    public Transform cameraTarget;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (cameraTarget != null)
        {
            transform.position = cameraTarget.transform.position;
            Vector3 angles = cameraTarget.transform.rotation.eulerAngles;
            transform.rotation = Quaternion.Euler(new Vector3(0.0f, angles.y, 0.0f));
        }
    }
}
