using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Reflection;

public class NewTest : MonoBehaviour
{
    int test = 4;
    int test2 = 5;
    Var v1;
    Var v2;

    // Start is called before the first frame update
    void Start()
    {
        //v1 = new Var(test, test.GetType());
        //v2 = new Var(test2, test2.GetType());
        //
        //bool result = HelperFunctions.NotEquals(v1, v2);
        //
        //print($"result is {result}");
        //Type type;
        //type = GetComponent("Rigidbody").GetType();
        //
        //print($"type test is {type.ToString()}");        

        //Material mat = new Material(GetComponent<Renderer>().material);
        //
        //ConstructorInfo info = mat.GetType().GetConstructor(new Type[] { typeof(Material) });
        //
        //Func<object, object[], object> matConst = Interpreter.Instance.CreateConstructor(info);
        //
        //Material newMat = (Material)matConst.Invoke(null, new object[] { mat });
        //
        //newMat.color = Color.green;

    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
