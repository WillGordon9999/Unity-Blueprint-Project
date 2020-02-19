using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Reflection;
using System.Linq;
using System.Linq.Expressions;
using System;
class RealTimeVar
{
    object obj;
    Type type;
}

public class CodeNode
{
    Func<object, object[], object> function;
    CodeNode nextPath;
    CodeNode falsePath;
    string returnVarName;
}

public class Interpreter
{
    //TO-DO: Either make these locals or in general get to delegate construction
    //Text text;
    //MethodInfo method;
    //FieldInfo field;
    //object[] passArgs;
    //object target;
    //Func<object, object[], object> newTest;
    //public List<string> includes;
    static Interpreter mInstance;

    Interpreter()
    {

    }

    public static Interpreter Instance
    {   get
        {
            if (mInstance == null)            
                mInstance = new Interpreter();                
            
            return mInstance;
        }
        private set { }
    }

    

    // Start is called before the first frame update
    void Start()
    {
        //text = GameObject.Find("InputField").transform.Find("Text").GetComponent<Text>();
        //if (text == null) print("text is null");
        
    }

    // Update is called once per frame
    void Update()
    {
        //if (newTest != null)
        //    newTest.Invoke(target, passArgs);
    }

    public void Compile()
    {        
        //string proc = text.text;
        //string[] raw = proc.Split(' ');
        //string[] args = new string[raw.Length - 2];
        //        
        //var comp = GetComponent(raw[0]);        
        //Type type = null;
        //
        //if (comp == null)
        //{
        //    //Source: https://stackoverflow.com/questions/8499593/c-sharp-how-to-check-if-namespace-class-or-method-exists-in-c
        //    type = (from assembly in AppDomain.CurrentDomain.GetAssemblies() from type2 in assembly.GetTypes() where type2.Name == raw[0] select type2).FirstOrDefault();
        //
        //    if (type == null)
        //    {
        //        print("Interpretor Error: No Component or Class defintion Found");
        //        return;
        //    }
        //}
        //
        //else
        //{
        //    type = comp.GetType();
        //    target = comp;
        //}
        //
        //print("Type is " + type.ToString());
        //
        //Array.Copy(raw, 2, args, 0, raw.Length - 2);
        //object[] finalArgs = ParseArgumentTypes(args);
        //passArgs = finalArgs;
        //
        //MethodInfo method = GetMethodMatch(type, raw[1], finalArgs);
        //
        //print(method.Name);                              
        //newTest = method.Bind();             
    }
        
    public MethodInfo[] GetFunctionDefinitions(string text)
    {
        string proc = text;
        string[] raw = proc.Split(' ');
        
        if (raw.Length <= 1)
        {
            Debug.Log("Too few arguments or not enough");
            return null;
        }

        string name = raw[1];
        //string[] args = new string[raw.Length - 2];

        //var comp = GetComponent(raw[0]);
        Type type = null;
       
        //Source: https://stackoverflow.com/questions/8499593/c-sharp-how-to-check-if-namespace-class-or-method-exists-in-c
        type = (from assembly in AppDomain.CurrentDomain.GetAssemblies() from type2 in assembly.GetTypes() where type2.Name == raw[0] select type2).FirstOrDefault();

        if (type == null)
        {
            Debug.Log("Interpreter Error: No Component or Class defintion Found");
            return null;
        }
              
        Debug.Log("Type is " + type.ToString());

        List<MethodInfo> target = new List<MethodInfo>();
        MethodInfo[] methods = type.GetMethods(); 

        foreach (MethodInfo m in methods)
        {
            if (m.Name == name)
                target.Add(m);
        }
        
        return target.ToArray();

        //Array.Copy(raw, 2, args, 0, raw.Length - 2);
        //object[] finalArgs = ParseArgumentTypes(args);
        //passArgs = finalArgs;

        //MethodInfo method = GetMethodMatch(type, raw[1], finalArgs);

        //print(method.Name);
        //newTest = method.Bind();

    }

    object[] ParseArgumentTypes(string[] args)
    {
        object[] objs = new object[args.Length];

        for (int i = 0; i < args.Length; i++)
        {
            //TO-DO: Add Keywords or find a way to get members or possible function calls, maybe try using the LINQ thing above 
            bool bVal;
            int iVal;
            float fVal;
            double dVal;            

            if (Boolean.TryParse(args[i], out bVal))
            {
                //print("Parsing Bool!");
                objs[i] = bVal;
                continue;
            }

            if (Int32.TryParse(args[i], out iVal))
            {
                //print("Parsing int");
                objs[i] = iVal;
                continue;
            }

            if (Single.TryParse(args[i], out fVal))
            {
                //print("Parsing float");
                objs[i] = fVal;
                continue;
            }

            if (Double.TryParse(args[i], out dVal))
            {
                //print("Parsing string");
                objs[i] = dVal;
                continue;
            }

            // Check the " character
            if (args[i][0] == '"')
            {
                objs[i] = args[i];
            }
        }

        return objs;
    }

    MethodInfo GetMethodMatch(Type type, string name, object[] args)
    {
        MethodInfo[] initial = type.GetMethods();
        List<MethodInfo> methods = new List<MethodInfo>();

        foreach(MethodInfo m in initial)
        {
            if (m.Name == name)
                methods.Add(m);
        }

        if (methods.Count > 0)
        {
            //print("starting parameter loop");
            foreach (MethodInfo m in methods)
            {
                bool isMatch = true;
                ParameterInfo[] parameters = m.GetParameters();

                if (parameters.Length != args.Length)
                    continue;
                else
                {
                    //print("parameter count is the same!");
                    for (int i = 0; i < parameters.Length; i++)
                        if (parameters[i].ParameterType != args[i].GetType())
                            isMatch = false;
                        //else
                        //    print("Got a matching parameter type");

                    if (isMatch)
                        return m;
                }
            }
        }
        return null;
    }
}

//Source: https://github.com/coder0xff/FastDelegate.Net/blob/master/FastDelegate.Net/FastDelegate.cs

//namespace FastDelegate.Net
public static class MethodInfoExtensions
{
    private static Func<object, object[], object> CreateForNonVoidInstanceMethod(MethodInfo method)
    {
        ParameterExpression instanceParameter = Expression.Parameter(typeof(object), "target");
        ParameterExpression argumentsParameter = Expression.Parameter(typeof(object[]), "arguments");

        MethodCallExpression call = Expression.Call(
            Expression.Convert(instanceParameter, method.DeclaringType),
            method,
            CreateParameterExpressions(method, argumentsParameter));

        Expression<Func<object, object[], object>> lambda = Expression.Lambda<Func<object, object[], object>>(
            Expression.Convert(call, typeof(object)),
            instanceParameter,
            argumentsParameter);

        return lambda.Compile();
    }

    private static Func<object[], object> CreateForNonVoidStaticMethod(MethodInfo method)
    {
        ParameterExpression argumentsParameter = Expression.Parameter(typeof(object[]), "arguments");

        MethodCallExpression call = Expression.Call(
            method,
            CreateParameterExpressions(method, argumentsParameter));

        Expression<Func<object[], object>> lambda = Expression.Lambda<Func<object[], object>>(
            Expression.Convert(call, typeof(object)),
            argumentsParameter);

        return lambda.Compile();
    }

    private static Action<object, object[]> CreateForVoidInstanceMethod(MethodInfo method)
    {
        ParameterExpression instanceParameter = Expression.Parameter(typeof(object), "target");
        ParameterExpression argumentsParameter = Expression.Parameter(typeof(object[]), "arguments");

        MethodCallExpression call = Expression.Call(
            Expression.Convert(instanceParameter, method.DeclaringType),
            method,
            CreateParameterExpressions(method, argumentsParameter));

        Expression<Action<object, object[]>> lambda = Expression.Lambda<Action<object, object[]>>(
            call,
            instanceParameter,
            argumentsParameter);

        return lambda.Compile();
    }

    private static Action<object[]> CreateForVoidStaticMethod(MethodInfo method)
    {
        ParameterExpression argumentsParameter = Expression.Parameter(typeof(object[]), "arguments");

        MethodCallExpression call = Expression.Call(
            method,
            CreateParameterExpressions(method, argumentsParameter));

        Expression<Action<object[]>> lambda = Expression.Lambda<Action<object[]>>(
            call,
            argumentsParameter);

        return lambda.Compile();
    }

    private static Expression[] CreateParameterExpressions(MethodInfo method, Expression argumentsParameter)
    {
        return method.GetParameters().Select((parameter, index) =>
            Expression.Convert(
                Expression.ArrayIndex(argumentsParameter, Expression.Constant(index)), parameter.ParameterType)).Cast<Expression>().ToArray();
    }

    public static Func<object, object[], object> Bind(this MethodInfo method)
    {
        if (method.IsStatic)
        {
            if (method.ReturnType == typeof(void))
            {
                Action<object[]> wrapped = CreateForVoidStaticMethod(method);
                return (target, parameters) => {
                    wrapped(parameters);
                    return (object)null;
                };
            }
            else
            {
                Func<object[], object> wrapped = CreateForNonVoidStaticMethod(method);
                return (target, parameters) => wrapped(parameters);
            }
        }
        if (method.ReturnType == typeof(void))
        {
            Action<object, object[]> wrapped = CreateForVoidInstanceMethod(method);
            return (target, parameters) => {
                wrapped(target, parameters);
                return (object)null;
            };
        }
        else
        {
            Func<object, object[], object> wrapped = CreateForNonVoidInstanceMethod(method);
            return wrapped;
        }
    }

    public static Type LambdaType(this MethodInfo method)
    {
        if (method.ReturnType == typeof(void))
        {
            Type actionGenericType;
            switch (method.GetParameters().Length)
            {
                case 0:
                    return typeof(Action);
                case 1:
                    actionGenericType = typeof(Action<>);
                    break;
                case 2:
                    actionGenericType = typeof(Action<,>);
                    break;
                case 3:
                    actionGenericType = typeof(Action<,,>);
                    break;
                case 4:
                    actionGenericType = typeof(Action<,,,>);
                    break;
#if NET_FX_4 //See #define NET_FX_4 as the head of this file
                    case 5:
                        actionGenericType = typeof(Action<,,,,>);
                        break;
                    case 6:
                        actionGenericType = typeof(Action<,,,,,>);
                        break;
                    case 7:
                        actionGenericType = typeof(Action<,,,,,,>);
                        break;
                    case 8:
                        actionGenericType = typeof(Action<,,,,,,,>);
                        break;
                    case 9:
                        actionGenericType = typeof(Action<,,,,,,,,>);
                        break;
                    case 10:
                        actionGenericType = typeof(Action<,,,,,,,,,>);
                        break;
                    case 11:
                        actionGenericType = typeof(Action<,,,,,,,,,,>);
                        break;
                    case 12:
                        actionGenericType = typeof(Action<,,,,,,,,,,,>);
                        break;
                    case 13:
                        actionGenericType = typeof(Action<,,,,,,,,,,,,>);
                        break;
                    case 14:
                        actionGenericType = typeof(Action<,,,,,,,,,,,,,>);
                        break;
                    case 15:
                        actionGenericType = typeof(Action<,,,,,,,,,,,,,,>);
                        break;
                    case 16:
                        actionGenericType = typeof(Action<,,,,,,,,,,,,,,,>);
                        break;
#endif
                default:
                    throw new NotSupportedException("Lambdas may only have up to 16 parameters.");
            }
            return actionGenericType.MakeGenericType(method.GetParameters().Select(_ => _.ParameterType).ToArray());
        }
        Type functionGenericType;
        switch (method.GetParameters().Length)
        {
            case 0:
                return typeof(Func<>);
            case 1:
                functionGenericType = typeof(Func<,>);
                break;
            case 2:
                functionGenericType = typeof(Func<,,>);
                break;
            case 3:
                functionGenericType = typeof(Func<,,,>);
                break;
            case 4:
                functionGenericType = typeof(Func<,,,,>);
                break;
#if NET_FX_4 //See #define NET_FX_4 as the head of this file
                case 5:
                    funcGenericType = typeof(Func<,,,,,>);
                    break;
                case 6:
                    funcGenericType = typeof(Func<,,,,,,>);
                    break;
                case 7:
                    funcGenericType = typeof(Func<,,,,,,,>);
                    break;
                case 8:
                    funcGenericType = typeof(Func<,,,,,,,,>);
                    break;
                case 9:
                    funcGenericType = typeof(Func<,,,,,,,,,>);
                    break;
                case 10:
                    funcGenericType = typeof(Func<,,,,,,,,,,>);
                    break;
                case 11:
                    funcGenericType = typeof(Func<,,,,,,,,,,,>);
                    break;
                case 12:
                    funcGenericType = typeof(Func<,,,,,,,,,,,,>);
                    break;
                case 13:
                    funcGenericType = typeof(Func<,,,,,,,,,,,,,>);
                    break;
                case 14:
                    funcGenericType = typeof(Func<,,,,,,,,,,,,,,>);
                    break;
                case 15:
                    funcGenericType = typeof(Func<,,,,,,,,,,,,,,,>);
                    break;
                case 16:
                    funcGenericType = typeof(Func<,,,,,,,,,,,,,,,,>);
                    break;
#endif
            default:
                throw new NotSupportedException("Lambdas may only have up to 16 parameters.");
        }
        var parametersAndReturnType = new Type[method.GetParameters().Length + 1];
        method.GetParameters().Select(_ => _.ParameterType).ToArray().CopyTo(parametersAndReturnType, 0);
        parametersAndReturnType[parametersAndReturnType.Length - 1] = method.ReturnType;
        return functionGenericType.MakeGenericType(parametersAndReturnType);
    }
}