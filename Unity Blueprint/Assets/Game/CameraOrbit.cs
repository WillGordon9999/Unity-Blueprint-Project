//Source: http://wiki.unity3d.com/index.php/MouseOrbitImproved
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class CameraOrbit : MonoBehaviour
{
    public Transform target;
    public float distance = 7.0f;
    public float minDistance = 1.0f;
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
    public bool useGravityChange = false;

    private Rigidbody rb;
    [Header("Debug")]
    public float x = 0.0f;
    public float y = 0.0f;
    public float yCheck = 0.0f;
    public bool yMove = false;
    Matrix4x4 matrix;

    [HideInInspector] public GameObject zeroAnchor;
    [HideInInspector] public Quaternion targetRot;
    // Use this for initialization
    GameObject parent;
    GameObject followTarget;
    Transform origTarget;

    private void Awake()
    {
        if (target == null)
            target = GameObject.Find("Player").transform;        

        zeroAnchor = new GameObject("Camera Zero Anchor");
        zeroAnchor.transform.position = transform.position;

        //parent = new GameObject("Camera Parent");
        //transform.parent = parent.transform;
        //zeroAnchor.transform.parent = parent.transform;

        //gravity = new GameObject("Camera Parent");
        //gravity.transform.position = transform.position;
        //Vector3 grav = Physics.gravity.normalized;
        //gravity.transform.rotation = Quaternion.LookRotation(-grav);
        //transform.parent = gravity.transform;
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

        if (target)
        {
            transform.position = target.TransformPoint(Vector3.back) * distance;           
        }
    }

    void LateUpdate()
    {
        //if (!enabled)
        //    return;

        if (Time.timeScale == 0.0f)
            return;
               
        OriginalUpdate();
    }

    void NewUpdateV2()
    {
        //Construct the basis vectors
        //Vector3 up = -Physics.gravity.normalized;
        //Vector3 forward = Vector3.Cross(target.transform.right, up);
        //Vector3 right = target.transform.right;

        Vector3 up = target.up;
        Vector3 forward = target.forward;
        Vector3 right = target.right;

        Vector4 rightBasis = new Vector4(right.x, right.y, right.z, 0);
        Vector4 upBasis = new Vector4(up.x, up.y, up.z, 0);
        Vector4 forwardBasis = new Vector4(forward.x, forward.y, forward.z, 0);
        Vector4 projection = new Vector4(0, 0, 0, 1);

        //Construct the matrix
        matrix = new Matrix4x4(rightBasis, upBasis, forwardBasis, projection);

        zeroAnchor.transform.position = matrix * transform.position;
        zeroAnchor.transform.rotation = matrix.rotation;
        
        //transform.position = matrix.inverse * transform.position;
        
    }

    void NewUpdate()
    {
        float mouseX = Input.GetAxis("Mouse X");
        float mouseY = Input.GetAxis("Mouse Y");

        x += mouseX * xSpeed;       
        
        //Check for change in y
        yCheck = ClampAngle(y - mouseY * ySpeed, yMinLimit, yMaxLimit);

        if (yCheck != y)         
            yMove = true;        
        else
            yMove = false;

        //Ensure y gets clamped
        y = yCheck;

        RaycastHit hit;

        //Check if there's anything in between the player and camera
        if (Physics.Linecast(target.position, transform.position, out hit))
        {            
            if (hit.distance > returnThreshold && distance > minDistance)
                distance -= hit.distance;

            if (distance < minDistance)
                distance = minDistance;
        }

        //If Clear, move back while checking behind the camera
        else
        {
            if (distance < defaultDistance)
            {
                if (Physics.Raycast(transform.position, -transform.forward, out hit, defaultDistance))                
                    distance = Mathf.Lerp(distance, hit.distance, 0.5f);
                
                //If no return to default distance
                else                
                    distance = Mathf.Lerp(distance, defaultDistance, returnSpeed);                
            }
        }

        //Look at the target
        //transform.LookAt(target, -Physics.gravity.normalized);
        //zeroAnchor.transform.LookAt(target, -Physics.gravity.normalized);
        
        Vector3 dir = (target.position - transform.position).normalized;
        transform.rotation = Quaternion.LookRotation(dir, -Physics.gravity.normalized);
        //zeroAnchor.transform.rotation = Quaternion.LookRotation(dir, -Physics.gravity.normalized);         
        
        //This can only be used because the target is an anchor
        if (mouseX != 0.0f || mouseY != 0.0f)
        {
            transform.RotateAround(target.position, -Physics.gravity.normalized, mouseX * xSpeed);            
            if (yMove) transform.RotateAround(target.position, target.right, mouseY * ySpeed);
        }

        
        //Update position        
        //Vector3 dir = (transform.position - target.position).normalized;
        dir = (transform.position - target.position).normalized;
        transform.position = target.position + dir * distance;
        //zeroAnchor.transform.position = target.position + new Vector3(dir.x, 0.0f, dir.z) * distance;               

        Vector3 cameraPos = target.InverseTransformPoint(transform.position);
        zeroAnchor.transform.position = target.TransformPoint(new Vector3(cameraPos.x, 0.0f, cameraPos.z));
        dir = (target.position - zeroAnchor.transform.position).normalized;
        zeroAnchor.transform.rotation = Quaternion.LookRotation(dir, -Physics.gravity.normalized);

        //We want to do something along these lines but it spins out of control
        //Vector3 pos = target.TransformPoint(dir * distance);
        //Vector3 zero = target.TransformPoint(new Vector3(dir.x, 0.0f, dir.z) * distance);
        //transform.position = pos;
        //zeroAnchor.transform.position = zero;

    }

    void OriginalUpdate()
    {
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

            //transform.rotation = rotation;
            //transform.position = position;
            //
            //zeroAnchor.transform.rotation = zeroRot;
            //zeroAnchor.transform.position = zeroPos;
            
            position = rotation * negDistance;
            transform.position = target.TransformPoint(position);
                      
            Vector3 dir = (target.position - transform.position).normalized;
            transform.rotation = Quaternion.LookRotation(dir, -Physics.gravity.normalized);
            
            zeroPos = target.InverseTransformPoint(transform.position);
            zeroPos = new Vector3(zeroPos.x, 0.0f, zeroPos.z);
            zeroPos = target.TransformPoint(zeroPos);
            dir = (target.position - zeroPos).normalized;
            
            zeroAnchor.transform.position = zeroPos;
            zeroAnchor.transform.rotation = Quaternion.LookRotation(dir, -Physics.gravity.normalized);
          
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

