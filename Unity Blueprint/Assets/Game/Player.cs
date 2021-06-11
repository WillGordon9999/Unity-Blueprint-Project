using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    public static Player Instance { get { return mInstance; } private set { } }
    private static Player mInstance;
    [HideInInspector] public Rigidbody rb;
    public Transform LeftFootTarget;
    public Transform RightFootTarget;
    public Transform LeftHandTarget;
    public Transform RightHandTarget;
    public Transform GroundCheck;
    public DitzelGames.FastIK.FastIKFabric LeftFootIK;
    public DitzelGames.FastIK.FastIKFabric RightFootIK;
    public DitzelGames.FastIK.FastIKFabric LeftHandIK;
    public DitzelGames.FastIK.FastIKFabric RightHandIK;

    private void Awake()
    {
        if (mInstance == null)
            mInstance = this;

        rb = GetComponent<Rigidbody>();
        SetHandIK(false);
    }  
    
    public void SetFootIK(bool val)
    {
        if (LeftFootIK != null)
            LeftFootIK.enabled = val;

        if (RightFootIK != null)
            RightFootIK.enabled = val;
    }

    public void SetHandIK(bool val)
    {
        if (RightHandIK != null)
            RightHandIK.enabled = val;

        if (LeftHandIK != null)
            LeftHandIK.enabled = val;
    }
}
