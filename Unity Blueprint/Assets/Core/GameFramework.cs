using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameFramework
{
    public static void Sweep(GameObject obj, Vector3 targetPos, float speed, BlueprintComponent comp)
    {
        //float cost = 20.0f * Vector3.Distance(obj.transform.position, targetPos) * speed;
        float cost = 100.0f;
        //Debug.Log("Inside sweep");

        if (cost > 500.0f)
        {
            //Debug.Log("Not enough Mana");
            comp.status = ActionStatus.Failure;
            return;
        }

        //Debug.Log("Enough Mana");
        obj.transform.position = Vector3.Lerp(obj.transform.position, targetPos, speed * Time.deltaTime);
        comp.status = ActionStatus.Success;
    }

    public static void SetABool(bool value)
    {

    }

    public static void SetInt(int value)
    {

    }

    public static void SetVector2(Vector2 value)
    {

    }

    public static Vector3 SetVector3(Vector3 value)
    {
        return value;
    }

    public static void SetVector4(Vector4 value)
    {

    }

    public static void SetRect(Rect value)
    {

    }

    public static void SetEnum(RigidbodyConstraints rbconst)
    {

    }
}
