using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CoroutineTest : MonoBehaviour
{
    Renderer rend;
    public Color targetColor = Color.red;
    string currentState = "";
    // Start is called before the first frame update
    void Start()
    {
        rend = GetComponent<Renderer>();
        //StartCoroutine("ColorChange");
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.F))
        {            
            StartCoroutine("FlyUp");
        }
    }

    void ChangeState(string newState)
    {
        StopCoroutine(currentState);
        StartCoroutine(newState);
    }

    IEnumerator FlyUp()
    {
        currentState = "FlyUp";
        GetComponent<Rigidbody>().useGravity = false;

        while (true)
        {
            transform.Translate(0.0f, 1.0f * Time.deltaTime, 0.0f);

            if (transform.position.y >= 5.0f)
                ChangeState("Spin");

            yield return null;
        }
    }

    IEnumerator Spin()
    {
        print("In spin coroutine");
        float timer = 0.0f;

        while (timer < 5.0f)
        {
            timer += Time.deltaTime;
            transform.Rotate(0.0f, 0.0f, 45.0f);
            yield return null;
        }

        GetComponent<Rigidbody>().useGravity = true;
    }

    IEnumerator ColorChange()
    {
        print("Beginning of Coroutine");

        while (rend.material.color != targetColor)
        {
            rend.material.color = Color.Lerp(rend.material.color, targetColor, 0.1f);            
            yield return null;
        }
    }

}
