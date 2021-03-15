using System.Collections;
using System.Collections.Generic;
using UnityEngine;
//using Rewired;
public class Movement : MonoBehaviour
{
    //Rewired.Player player;
    public float moveSpeed = 10.0f;
    public float rotSpeed = 0.15f;    
    const float norm = 0.707f;
    new GameObject camera;
    
    // Start is called before the first frame update
    void Start()
    {
        //player = ReInput.players.GetPlayer(0);
        camera = GameObject.FindGameObjectWithTag("MainCamera");
    }

    // Update is called once per frame
    void Update()
    {
        //float LSX = player.GetAxis("LSX");
        //float LSY = player.GetAxis("LSY");
        float x = Input.GetAxis("Horizontal");
        float y = Input.GetAxis("Vertical");
        
        //if (LSX != 0.0f || LSY != 0.0f)
        if (x != 0.0f || y != 0.0f)
        {
            Vector3 dir = camera.transform.TransformDirection(new Vector3(x, 0.0f, y));
            transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(new Vector3(dir.x, 0, dir.z)), rotSpeed);
            transform.Translate(0, 0, moveSpeed * norm * Time.deltaTime, Space.Self);
        }


    }
}
