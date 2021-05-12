using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemySense : MonoBehaviour
{
    [Header("Vision")]
    public bool visionEnabled = true;
    public bool playerVisible = false;
    public float visionAngle = 0.2f;
    public float maxVisionRange = 20.0f;
    public float timeUntilAlert = 1.0f;
    bool alertTimeStarted = false;

    [Header("Hearing")]
    public bool hearingEnabled = true;
    public bool playerAudible = false;
    public float maxHearingRange = 25.0f;
    public float minHearingVolume = 3.0f;

    GameObject player;
    public Vector3 lastSeenPos;
    public Vector3 lastSeenDir;
    public Vector3 lastHeardPos;
    
    void Start()
    {
        player = Player.Instance.gameObject;
    }
    
    public bool CheckVision()
    {
        if (visionEnabled)
        {
            Vector3 distance = (player.transform.position - transform.position);

            if (distance.magnitude <= maxVisionRange)
            {
                if (Vector3.Dot(transform.TransformDirection(Vector3.forward), distance.normalized) >= visionAngle)
                {
                    RaycastHit hit;

                    if (Physics.Linecast(transform.position, player.transform.position, out hit))
                    {
                        if (hit.collider != null)
                        {
                            if (hit.collider.gameObject == player)
                            {                                
                                lastSeenPos = player.transform.position;
                                lastSeenDir = player.transform.TransformDirection(Vector3.forward);
                                playerVisible = true;                                
                                return true;
                            }

                            playerVisible = false;
                            return false;
                        }

                        print("Vision linecast is null");
                        playerVisible = false;

                    }

                    else
                    {
                        print("Vision linecast is null");
                        playerVisible = false;
                    }

                }

                playerVisible = false;
            }

            playerVisible = false;
        }

        return false;
    }
    
    public bool CheckHearing(Vector3 pos, float volume)
    {
        if (hearingEnabled)
        {
            if (volume >= minHearingVolume)
            {
                if (Vector3.Distance(transform.position, pos) <= maxHearingRange)
                {
                    playerAudible = true;
                    return true;
                }
            }
        }

        return false;
    }

}
