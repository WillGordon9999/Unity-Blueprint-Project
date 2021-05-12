//Source: http://wiki.unity3d.com/index.php/MouseOrbitImproved
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class CameraOrbit : MonoBehaviour
{
    public Transform target;
    public float distance = 7.0f;
    public float returnSpeed = 0.1f;
    float defaultDistance;
    public float xSpeed = 60.0f;
    public float ySpeed = 60.0f;
    public float padXSpeed = 60.0f;
    public float padYSpeed = 60.0f;
    public float altSpeed = 20.0f;    
    
    public float yMinLimit = -20f;
    public float yMaxLimit = 80f;

    public float distanceMin = .5f;
    public float distanceMax = 15f;
    public float returnThreshold = 1.5f;

    private Rigidbody rb;
    float x = 0.0f;
    float y = 0.0f;    
    [HideInInspector] public GameObject zeroAnchor;
    // Use this for initialization

    private void Awake()
    {
        target = GameObject.Find("Player").transform;        
        zeroAnchor = new GameObject("Camera Zero Anchor");
        zeroAnchor.transform.position = transform.position;        
    }


    void Start()
    {        
        Vector3 angles = transform.eulerAngles;
        x = angles.y;
        y = angles.x;        
        defaultDistance = distance;

        rb = GetComponent<Rigidbody>();

        // Make the rigid body not change rotation
        if (rb != null)
        {
            rb.freezeRotation = true;
        }
    }

    void Update()
    {

        if (Time.timeScale == 0.0f)
            return;

        if (target)
        {
            //float RSX = Input.GetAxis("RSX");
            //float RSY = Input.GetAxis("RSY");

            //x += RSX * padXSpeed * distance * 0.02f;
            //y -= RSY * padYSpeed * distance * 0.02f;

            float mouseX = Input.GetAxis("Mouse X");
            float mouseY = Input.GetAxis("Mouse Y");

            x += mouseX * xSpeed * distance * 0.02f;
            y -= mouseY * ySpeed * distance * 0.02f;
            
            y = ClampAngle(y, yMinLimit, yMaxLimit);            

            Quaternion rotation = Quaternion.Euler(y, x, 0);
            Quaternion zeroRot = Quaternion.Euler(0, x, 0);

            float start = Time.time;
            RaycastHit hit;
            if (Physics.Linecast(target.position, transform.position, out hit))
            {
                //if (hit.distance > returnThreshold && hit.collider.tag != "Enemy")
                if (hit.distance > returnThreshold)
                    distance -= hit.distance;
            }
           
            //If the camera hits something check to see if there is anything behind the camera
            else
            {
                if (distance < defaultDistance)
                {
                    //If yes go halfway between object and player
                    if (Physics.Raycast(transform.position, -transform.forward, out hit, defaultDistance))
                    {
                        distance = Mathf.Lerp(distance, hit.distance, 0.5f);
                    }
                    //If no return to default distance
                    else
                    {
                        distance = Mathf.Lerp(distance, defaultDistance, returnSpeed);
                    }
                }
            }

            Vector3 negDistance = new Vector3(0.0f, 0.0f, -distance);
            Vector3 position = rotation * negDistance + target.position;
            Vector3 zeroPos = zeroRot * negDistance + target.position;

            transform.rotation = rotation;
            transform.position = position;

            zeroAnchor.transform.rotation = zeroRot;
            zeroAnchor.transform.position = zeroPos;
            
            return;
        }
    }

    public static float ClampAngle(float angle, float min, float max)
    {
        if (angle < -360F)
            angle += 360F;
        if (angle > 360F)
            angle -= 360F;
        return Mathf.Clamp(angle, min, max);
    }

}

