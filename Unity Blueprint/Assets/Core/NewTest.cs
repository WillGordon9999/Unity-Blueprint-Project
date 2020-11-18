using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Reflection;
using System.Reflection.Emit;
using System.Linq.Expressions;
using System.Linq;
using System.CodeDom.Compiler;
using Microsoft.CSharp;
using UnityEditor;

public class NewTest : MonoBehaviour
{
    Dictionary<string, Var> variables;
    Expression expressionBody;
    object[] passArgs;

    GameObject obj1;
    GameObject obj2;
    
    //public Action action = () => obj1.SetActive(false);
    // Start is called before the first frame update
    void Compile()
    {
        //AssemblyName aName = new AssemblyName("DynamicAssemblyExample");        
        //AssemblyBuilder ab =
        //    AppDomain.CurrentDomain.DefineDynamicAssembly(
        //        aName,
        //        AssemblyBuilderAccess.RunAndSave);
        //
        //// For a single-module assembly, the module name is usually
        //// the assembly name plus an extension.
        //ModuleBuilder mb =
        //    ab.DefineDynamicModule(aName.Name, aName.Name + ".dll");
        //
        //TypeBuilder tb = mb.DefineType(
        //    "MyDynamicType",
        //     TypeAttributes.Public);
        //
        //// Add a private field of type int (Int32).
        //FieldBuilder fbNumber = tb.DefineField(
        //    "m_number",
        //    typeof(int),
        //    FieldAttributes.Private);
        //
        //// Define a constructor that takes an integer argument and
        //// stores it in the private field.
        //Type[] parameterTypes = { typeof(int) };
        //ConstructorBuilder ctor1 = tb.DefineConstructor(
        //    MethodAttributes.Public,
        //    CallingConventions.Standard,
        //    parameterTypes);
        //
        //ILGenerator ctor1IL = ctor1.GetILGenerator();
        //// For a constructor, argument zero is a reference to the new
        //// instance. Push it on the stack before calling the base
        //// class constructor. Specify the default constructor of the
        //// base class (System.Object) by passing an empty array of
        //// types (Type.EmptyTypes) to GetConstructor.
        //ctor1IL.Emit(OpCodes.Ldarg_0);
        //ctor1IL.Emit(OpCodes.Call,
        //    typeof(object).GetConstructor(Type.EmptyTypes));
        //// Push the instance on the stack before pushing the argument
        //// that is to be assigned to the private field m_number.
        //ctor1IL.Emit(OpCodes.Ldarg_0);
        //ctor1IL.Emit(OpCodes.Ldarg_1);
        //ctor1IL.Emit(OpCodes.Stfld, fbNumber);
        //ctor1IL.Emit(OpCodes.Ret);
        //
        //// Define a default constructor that supplies a default value
        //// for the private field. For parameter types, pass the empty
        //// array of types or pass null.
        //ConstructorBuilder ctor0 = tb.DefineConstructor(
        //    MethodAttributes.Public,
        //    CallingConventions.Standard,
        //    Type.EmptyTypes);
        //
        //ILGenerator ctor0IL = ctor0.GetILGenerator();
        //// For a constructor, argument zero is a reference to the new
        //// instance. Push it on the stack before pushing the default
        //// value on the stack, then call constructor ctor1.
        //ctor0IL.Emit(OpCodes.Ldarg_0);
        //ctor0IL.Emit(OpCodes.Ldc_I4_S, 42);
        //ctor0IL.Emit(OpCodes.Call, ctor1);
        //ctor0IL.Emit(OpCodes.Ret);
        //
        //// Define a property named Number that gets and sets the private
        //// field.
        ////
        //// The last argument of DefineProperty is null, because the
        //// property has no parameters. (If you don't specify null, you must
        //// specify an array of Type objects. For a parameterless property,
        //// use the built-in array with no elements: Type.EmptyTypes)
        //PropertyBuilder pbNumber = tb.DefineProperty(
        //    "Number",
        //    PropertyAttributes.HasDefault,
        //    typeof(int),
        //    null);
        //
        //// The property "set" and property "get" methods require a special
        //// set of attributes.
        //MethodAttributes getSetAttr = MethodAttributes.Public |
        //    MethodAttributes.SpecialName | MethodAttributes.HideBySig;
        //
        //// Define the "get" accessor method for Number. The method returns
        //// an integer and has no arguments. (Note that null could be
        //// used instead of Types.EmptyTypes)
        //MethodBuilder mbNumberGetAccessor = tb.DefineMethod(
        //    "get_Number",
        //    getSetAttr,
        //    typeof(int),
        //    Type.EmptyTypes);
        //
        //ILGenerator numberGetIL = mbNumberGetAccessor.GetILGenerator();
        //// For an instance property, argument zero is the instance. Load the
        //// instance, then load the private field and return, leaving the
        //// field value on the stack.
        //numberGetIL.Emit(OpCodes.Ldarg_0);
        //numberGetIL.Emit(OpCodes.Ldfld, fbNumber);
        //numberGetIL.Emit(OpCodes.Ret);
        //
        //// Define the "set" accessor method for Number, which has no return
        //// type and takes one argument of type int (Int32).
        //MethodBuilder mbNumberSetAccessor = tb.DefineMethod(
        //    "set_Number",
        //    getSetAttr,
        //    null,
        //    new Type[] { typeof(int) });
        //
        //ILGenerator numberSetIL = mbNumberSetAccessor.GetILGenerator();
        //// Load the instance and then the numeric argument, then store the
        //// argument in the field.
        //numberSetIL.Emit(OpCodes.Ldarg_0);
        //numberSetIL.Emit(OpCodes.Ldarg_1);
        //numberSetIL.Emit(OpCodes.Stfld, fbNumber);
        //numberSetIL.Emit(OpCodes.Ret);
        //
        //// Last, map the "get" and "set" accessor methods to the
        //// PropertyBuilder. The property is now complete.
        //pbNumber.SetGetMethod(mbNumberGetAccessor);
        //pbNumber.SetSetMethod(mbNumberSetAccessor);
        //
        //// Define a method that accepts an integer argument and returns
        //// the product of that integer and the private field m_number. This
        //// time, the array of parameter types is created on the fly.
        //MethodBuilder meth = tb.DefineMethod(
        //    "MyMethod",
        //    MethodAttributes.Public,
        //    typeof(int),
        //    new Type[] { typeof(int) });
        //
        //ILGenerator methIL = meth.GetILGenerator();
        //// To retrieve the private instance field, load the instance it
        //// belongs to (argument zero). After loading the field, load the
        //// argument one and then multiply. Return from the method with
        //// the return value (the product of the two numbers) on the
        //// execution stack.
        //methIL.Emit(OpCodes.Ldarg_0);
        //methIL.Emit(OpCodes.Ldfld, fbNumber);
        //methIL.Emit(OpCodes.Ldarg_1);
        //methIL.Emit(OpCodes.Mul);
        //methIL.Emit(OpCodes.Ret);
        //
        //// Finish the type.
        //Type t = tb.CreateType();
        //
        //// The following line saves the single-module assembly. This
        //// requires AssemblyBuilderAccess to include Save. You can now
        //// type "ildasm MyDynamicAsm.dll" at the command prompt, and
        //// examine the assembly. You can also write a program that has
        //// a reference to the assembly, and use the MyDynamicType type.
        ////
        //ab.Save(aName.Name + ".dll");
        //
        //// Because AssemblyBuilderAccess includes Run, the code can be
        //// executed immediately. Start by getting reflection objects for
        //// the method and the property.
        //MethodInfo mi = t.GetMethod("MyMethod");
        //PropertyInfo pi = t.GetProperty("Number");
        //
        //// Create an instance of MyDynamicType using the default
        //// constructor.
        //object o1 = Activator.CreateInstance(t);
        //
        //// Display the value of the property, then change it to 127 and
        //// display it again. Use null to indicate that the property
        //// has no index.
        //Debug.Log($"o1.Number: {pi.GetValue(o1, null)}" );
        //pi.SetValue(o1, 127, null);
        //Console.WriteLine($"o1.Number: {pi.GetValue(o1, null)}");
        //
        //// Call MyMethod, passing 22, and display the return value, 22
        //// times 127. Arguments must be passed as an array, even when
        //// there is only one.
        //object[] arguments = { 22 };
        //Debug.Log($"o1.MyMethod(22): {mi.Invoke(o1, arguments)}");
        //
        //// Create an instance of MyDynamicType using the constructor
        //// that specifies m_Number. The constructor is identified by
        //// matching the types in the argument array. In this case,
        //// the argument array is created on the fly. Display the
        //// property value.
        //object o2 = Activator.CreateInstance(t,
        //    new object[] { 5280 });
        //Debug.Log($"o2.Number: {pi.GetValue(o2, null)}");

        //END EXAMPLE --------------------------------------------------------------------

        var code =
            @"public class Test
            {
               public static void GetGameObjectName (UnityEngine.Object obj) { UnityEngine.Debug.Log( "" Test Debug "" + obj.name); }
            }
            ";


        CSharpCodeProvider compiler = new CSharpCodeProvider(); 

        CompilerParameters parameters = new CompilerParameters();

        parameters.GenerateExecutable = false;
        parameters.GenerateInMemory = false;
        parameters.ReferencedAssemblies.Add(typeof(string).Assembly.Location);
        parameters.ReferencedAssemblies.Add(typeof(GameObject).Assembly.Location);
        parameters.ReferencedAssemblies.Add(typeof(Debug).Assembly.Location);        
        parameters.OutputAssembly = "WillAssembly2.dll";

        CompilerResults result = compiler.CompileAssemblyFromSource(parameters, code);
        
        if (result.Errors.Count > 0)
        {
            Debug.Log($"Errors building {code} to  {result.PathToAssembly}");

            foreach(CompilerError error in result.Errors)
            {
                Debug.Log($"  {error.ToString()}  ");
            }
        }

        Type type = result.CompiledAssembly.GetType("Test");

        MethodInfo method = type.GetMethod("GetGameObjectName");

        method.Invoke(null, new object[] { gameObject });

        //ParameterExpression arg1 = Expression.Parameter(typeof(float), "num1");
        //ParameterExpression arg2 = Expression.Parameter(typeof(float), "num2");
        //
        //Expression add = Expression.Add(arg1, arg2);        
        ////Expression add = Expression.Add(Expression.Constant(5), Expression.Constant(5));        
        //Expression<Func<float, float, float>> func = Expression.Lambda<Func<float, float, float>>(add, new ParameterExpression[] { arg1, arg2 });
        //
        //AssemblyName asmName = new AssemblyName("WillAssembly");
        //
        //AssemblyBuilder asmBuilder = AppDomain.CurrentDomain.DefineDynamicAssembly(asmName, AssemblyBuilderAccess.RunAndSave);
        //
        //ModuleBuilder moduleBuilder = asmBuilder.DefineDynamicModule(asmName.Name, asmName.Name + ".dll");
        //
        //TypeBuilder typeBuilder = moduleBuilder.DefineType("WillType", TypeAttributes.Public);
        //
        //FieldBuilder fieldBuilder = typeBuilder.DefineField("willField", typeof(float), FieldAttributes.Public);
        //
        ////MethodBuilder methodBuilder = typeBuilder.DefineMethod("Add", MethodAttributes.Public, typeof(float), new Type[] { typeof(float), typeof(float) });
        ////MethodBuilder methodBuilder = typeBuilder.DefineMethod("Add", MethodAttributes.Static, typeof(float), new Type[] { typeof(float), typeof(float) });
        //
        //MethodBuilder methodBuilder = typeBuilder.DefineMethod("Print", MethodAttributes.Static, typeof(void), new[] { typeof(string) });
        //
        //Expression<Action<string>> test = (string text) => print(text);
        //
        //test.CompileToMethod(methodBuilder);
        ////MethodBuilder newMethod = typeBuilder.DefineMethod("Add", MethodAttributes.Public);        
        ////func.CompileToMethod(methodBuilder);
        ////func.CompileToMethod(newMethod);
        //
        ////ILGenerator generator = methodBuilder.GetILGenerator();
        ////
        ////generator.Emit(OpCodes.Ldarg_0);
        ////generator.Emit(OpCodes.Ldarg_1);                
        ////generator.Emit(OpCodes.Ret);        
        //
        //typeBuilder.CreateType();
        //
        //asmBuilder.Save(asmName.Name + ".dll");
        //
        //Assembly final = Assembly.LoadFile(asmName.Name + ".dll");
        //Type willType = final.GetType("WillType");        
        //
        //MethodInfo info = willType.GetMethod("Add");
        //MethodInfo[] methods = willType.GetMethods();
        //
        //object willObj = Activator.CreateInstance(willType);
        //
        ////float result = (float)info.Invoke(willObj, new object[] { 5.0f, 5.0f });
        //float result = (float)info.Invoke(null, new object[] { 5.0f, 5.0f });

        //Type[] types;
        //Assembly[] asms;
        //Interpreter.Instance.FindType("MyDynamicType", out types, out asms);

        //Assembly myAsm = Assembly.LoadFile("DynamicAssemblyExample.dll");
        //
        //if (myAsm != null)
        //{
        //    Type type = myAsm.GetType("MyDynamicType");
        //
        //}

    }

    //public T1 Test<T1, T2>(T1 arg, T2 arg2)
    //{
    //    return arg;
    //}

    public static T Test<T>(T arg)
    {
        return arg;
    }
  
    private void Start()
    {
        //Debug.Log("File Path is " + Application.persistentDataPath);
        //Debug.Log($"String Test {gameObject.name}\n  {transform.position.ToString()}");
        //
        //Type type = typeof(GameObject);
        //MethodInfo method = type.GetMethod("Find");
        //ParameterInfo[] pars = method.GetParameters();
        //
        //foreach (ParameterInfo p in pars)
        //    print($"Argument is {p.ParameterType} {p.Name}");
        //
        //string test = "WILLNOTWORK";
        //
        //string[] splitTest = test.Split(' ');
        //
        //if (splitTest == null)
        //    print("Split Test is null!");
        //var methods = transform.GetType().GetMethods();
        //
        //ParameterInfo p;
        //
        //foreach(MethodInfo m in methods)
        //{
        //    if (m.Name == "GetComponent")
        //    {
        //        if (m.IsGenericMethod)
        //        {
        //            Type[] types = m.GetGenericArguments();
        //            //string final = $"{m.Name} args are ";
        //            //foreach (Type t in types)
        //            //    final += t.ToString() + ", ";
        //            //print(final);
        //            MethodInfo template = m.MakeGenericMethod(typeof(Rigidbody));
        //            print(template.MetadataToken);
        //            var args = template.GetParameters();
        //            print(args.Length);
        //        }
        //
        //        if (m.IsGenericMethodDefinition)
        //        {
        //            print($"{m.Name} is Generic Method Definition is true");
        //        }
        //    }
        //}                
                
        var method = this.GetType().GetMethod("Test");
        
        var args = method.GetGenericArguments();

        var mainPars = method.GetParameters();

        var newMethod = method.MakeGenericMethod(typeof(int));

        ParameterInfo[] newArgs = newMethod.GetParameters();

        print(newArgs.Length);

    }
    

    // Update is called once per frame
    void Update()
    {
        //if (Input.GetMouseButtonDown(1))
        //{            
        //    print("Setting context menu");
        //    GenericMenu menu = new GenericMenu();
        //
        //    menu.AddItem(new GUIContent("Option 1"), false, Test);
        //    menu.AddItem(new GUIContent("Option 2"), false, Test);
        //    menu.AddItem(new GUIContent("Option 3"), false, Test);
        //    menu.AddItem(new GUIContent("Option 4"), false, Test);
        //
        //    menu.ShowAsContext();
        //}
    }

    //void Test()
    //{
    //    print("I am the delegator");
    //}

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


//Old Expression Test

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