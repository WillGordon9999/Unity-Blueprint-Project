using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Reflection;
using System.Linq.Expressions;
using System.Linq;

public class NewTest : MonoBehaviour
{
    Dictionary<string, Var> variables;
    Expression expressionBody;
    object[] passArgs;

    GameObject obj1;
    GameObject obj2;

    //public Action action = () => obj1.SetActive(false);
    // Start is called before the first frame update
    void Start()
    {
        //UnityEngine.Space space = Space.World;
        //
        //Type type = space.GetType();
        //
        //if (type.IsEnum)
        //{
        //    type.GetEnumNames();
        //}

        //int test = 5;
        //int test2 = 6;
        //
        //test %= test2;
        //
        //variables = new Dictionary<string, Var>();
        ////Set up variables
        //variables["randPos"] = new Var((object)(UnityEngine.Random.onUnitSphere * 5.0f), typeof(Vector3));
        //variables["newPos"] = new Var((object)new Vector3(0.0f, 0.0f, 0.0f), typeof(Vector3));
        //variables["transform"] = new Var((object)(Transform)null, typeof(Transform));
        //
        /////Get Component Transform
        //Node getTransformNode = new Node();
        //MethodInfo getCompMethod = this.GetType().GetMethod("GetComponent", new Type[] { typeof(string), typeof(string) });
        //
        ////Set up parameters -- would need extra expressions created for variable parameters
        //ParameterInfo[] getCompArgs = getCompMethod.GetParameters();
        //
        //getTransformNode.paramList = new List<Parameter>();
        //
        //foreach (ParameterInfo p in getCompArgs)        
        //    getTransformNode.paramList.Add(new Parameter(p.ParameterType, p.Name));
        //
        //getTransformNode.returnEntry = new Parameter(typeof(string));
        //getTransformNode.returnEntry.arg = "transform";
        //
        ////Compile the Method Call Expression - for now we will see if we can use the Lambda, if not use the raw MethodCallExpression
        ////Expression lambdaGetComp = Interpreter.Instance.NonVoidInstanceMethodExpression(getCompMethod);
        //
        ////Set up the target of the lambda aka this instance in this scenario.
        ////Expression getCompTarget = Expression.Parameter(typeof(object), "target");
        //Expression getCompTarget = Expression.Constant(this);
        //
        //List<object> argConcat = new List<object>();
        //
        ////Set up pass-In params for this function in this case it's just "Transform" and ""
        //foreach(Parameter p in getTransformNode.paramList)
        //{
        //    if (p.name == "type")
        //    {
        //        p.arg = "Transform";
        //        argConcat.Add(p.arg);
        //    }
        //    if (p.name == "other")
        //    {
        //        p.arg = "";
        //        argConcat.Add(p.arg);
        //    }
        //}
        //
        ////Prepare the final object array to pass in
        ////Expression getCompPassArgs = Expression.Parameter(typeof(object[]), "arguments");
        //Expression getCompPassArgs = Expression.Constant(argConcat.ToArray());
        //
        ////Prepare the variable to return to
        //Expression key = Expression.Constant(getTransformNode.returnEntry.arg); //I think this is normally returnInput
        //
        ////Access the Dictionary at the key
        //Expression dictAccess = Expression.Property(Expression.Constant(variables), "Item", key);
        //
        ////Access the field of the var that is the actual object
        //Expression objAccess = Expression.Field(dictAccess, typeof(Var).GetField("obj"));
        //
        ////Call the function
        ////MethodCallExpression callGetComp = (MethodCallExpression)Interpreter.Instance.NonVoidInstanceMethodExpression(getCompMethod);
        //MethodCallExpression call = Expression.Call
        //(
        //    Expression.Convert(getCompTarget, getCompMethod.DeclaringType),
        //    getCompMethod,
        //    Interpreter.Instance.CreateMethodParameters(getCompMethod, getCompPassArgs)
        //);
        //
        //
        ////Expression assignVar = Expression.Assign(dictAccess, call);
        //Expression assignVar = Expression.Assign(objAccess, call);
        //
        ////print the name to really test this
        ////MethodInfo debugLog = this.GetType().GetMethod("print");
        //
        //PropertyInfo nameProp = typeof(Transform).GetProperty("name");
        //
        //Expression convert = Expression.Convert(objAccess, nameProp.DeclaringType);
        //
        //Expression getName = Expression.Property(convert, nameProp);
        //
        //MethodInfo debugLog = typeof(MonoBehaviour).GetMethod("print");
        //
        ////Expression log = Expression.Call(debugLog, Expression.Constant($"Transform name {variables["transform"].obj}"));
        //Expression log = Expression.Call(debugLog, getName);
        //
        //Expression final = Expression.Block(assignVar, log);
        //
        //var lambda = Expression.Lambda<Action>(final, new ParameterExpression[] { });
        //
        //Action func = lambda.Compile();
        //
        //func.Invoke();
        //
        //print("made it past the call");
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public Component GetComponent(string type, string other)
    {
        if (other != "")
        {
            return (variables[other].obj as Component).GetComponent(type);
        }

        return GetComponent(type);
    }
}

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
