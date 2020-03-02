using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.UIElements;
using System;
using System.Reflection;

[Serializable]
public class NodeData
{    
    public Rect rect; //Rect for display on blueprint
    public Vector2 initPos; 
    public Vector2 initDimensions;
    public bool isDefined;
    public bool isEntryPoint;
    public string entryPointName; //Name for built in monobehaviour functions
    public ConnectionPointData inPoint;
    public ConnectionPointData outPoint;
    
    //REFLECTION SPECIFIC
    public List<ParameterData> paramList;
    public string input; //the raw input - can probably be used to activate the reflection
    public string type; //The stored type from original reflection
    public string assemblyPath; //The path for the assembly
    public int index; //The index of the list found for function overloads
    public UnityEngine.Object target;
    public int ID; //The way to find the correct node to reference at runtime because serialization
    public int nextID;

    //GUI stuff - not sure if really needed here - might be useful later if I want to make UI prettier
    public GUIStyle nodeStyle;
    public GUIStyle selectStyle;
    public GUIStyle inStyle;
    public GUIStyle outStyle;

    public NodeData(Node node)
    {
        //Basics
        rect = node.rect;
        initPos = node.initPos;
        initDimensions = node.initDimensions;
        isDefined = node.isDefined;
        isEntryPoint = node.isEntryPoint;

        //Style stuff
        nodeStyle = node.style;
        selectStyle = node.selectedNodeStyle;
        inStyle = node.inPoint.style;
        outStyle = node.outPoint.style;

        //Reflection core
        input = node.input;
        type = node.type;
        index = node.index;
        assemblyPath = node.assemblyPath;
        target = node.target;
        
        if (paramList == null)
            paramList = new List<ParameterData>();

        foreach(Parameter par in node.paramList)
        {
            paramList.Add(new ParameterData(par));
        }

        ID = node.ID;
        nextID = node.nextID;

        inPoint = new ConnectionPointData(node.inPoint, this);
        outPoint = new ConnectionPointData(node.outPoint, this);
    }
        
}

[Serializable]
public class ParameterData
{   
    //I wonder if I can make a type as a string along with the assembly path, and just a byte array to store any variable
    public string name;
    public Rect rect; //display rect
    //public string type; //to be later converted to full type
    public bool boolVal;
    public int intVal;
    public int enumVal;
    public float floatVal;
    public char charVal;
    public long longVal;
    public double doubleVal;
    public string strVal;
    public Rect rectVal;
    public Color colVal;
    public Vector2 vec2Val;
    public Vector3 vec3Val;
    public Vector4 vec4Val;
    public UnityEngine.Object obj; //if applicable    

    public enum ParamType { Bool, Int, Enum, Float, Char, Long, Double, String, Rect, Color, Vec2, Vec3, Vec4, Object }

    public ParamType type;

    public ParameterData(Parameter par)
    {
        type = par.paramType;
        switch(type)
        {
            case ParamType.Bool:
                {
                    boolVal = (bool)par.arg;
                    break;
                }
            case ParamType.Int:
                {
                    intVal = (int)par.arg;
                    break;
                }
            case ParamType.Enum:
                {
                    enumVal = (int)par.arg;
                    break;
                }
            case ParamType.Float:
                {
                    floatVal = (float)par.arg;
                    break;
                }
            case ParamType.Char:
                {
                    charVal = (char)par.arg;
                    break;
                }
            case ParamType.Long:
                {
                    longVal = (long)par.arg;
                    break;
                }
            case ParamType.Double:
                {
                    doubleVal = (double)par.arg;
                    break;
                }
            case ParamType.String:
                {
                    strVal = (string)par.arg;
                    break;
                }
            case ParamType.Rect:
                {
                    float[] val = (float[])par.arg;
                    rectVal = new Rect(val[0], val[1], val[2], val[3]);
                    break;
                }
            case ParamType.Color:
                {
                    colVal = (Color)par.arg;
                    break;
                }
            case ParamType.Vec2:
                {
                    float[] val = (float[])par.arg;
                    vec2Val = new Vector2(val[0], val[1]);
                    break;
                }
            case ParamType.Vec3:
                {
                    float[] val = (float[])par.arg;
                    //Vector3 test = (Vector3)par.arg;
                    vec3Val = new Vector3(val[0], val[1], val[2]);                    
                    break;
                }
            case ParamType.Vec4:
                {
                    float[] val = (float[])par.arg;
                    vec4Val = new Vector4(val[0], val[1], val[2], val[3]);
                    break;
                }
            case ParamType.Object:
                {
                    intVal = (int)par.arg;
                    break;
                }
        }

        //DOES NOT WORK
        //float[] test = new float[2] { 0.0f, 0.0f };
        //Vector2 vec = (Vector2)test;

    }

    public Type GetSystemType()
    {
        switch (type)
        {
            case ParamType.Bool:                
                return typeof(bool);
                
            case ParamType.Int:
                return typeof(int);                

            case ParamType.Enum:
                return typeof(System.Enum);
                
            case ParamType.Float:
                return typeof(float);                

            case ParamType.Char:
                return typeof(char);
                
            case ParamType.Long:
                return typeof(long);

            case ParamType.Double:
                return typeof(double);

            case ParamType.String:
                return typeof(string);

            case ParamType.Rect:
                return typeof(Rect);

            case ParamType.Color:
                return typeof(Color);

            case ParamType.Vec2:
                return typeof(Vector2);

            case ParamType.Vec3:
                return typeof(Vector3);

            case ParamType.Vec4:
                return typeof(Vector4);

            case ParamType.Object:
                return typeof(UnityEngine.Object);

        }
        return null;
    }

    public object GetValue()
    {
        switch(type)
        {
            case ParamType.Bool:                
                return boolVal;
                
            case ParamType.Int:                
                return intVal;

            case ParamType.Enum:                
                return enumVal;
            
            case ParamType.Float:                
                return floatVal;

            case ParamType.Char:                
                return charVal;

            case ParamType.Long:
                return longVal;

            case ParamType.Double:
                return doubleVal;

            case ParamType.String:
                return strVal;

            case ParamType.Rect:
                return rectVal;

            case ParamType.Color:
                return colVal;

            case ParamType.Vec2:
                return vec2Val;

            case ParamType.Vec3:
                return vec3Val;

            case ParamType.Vec4:
                return vec4Val;

            case ParamType.Object:
                return obj;

        }
        return null;
    }
}


public class Node
{
    public Rect rect;
    Rect fieldRect;
    public Vector2 initPos;
    public Vector2 initDimensions;
    public string title;
    public bool isDragged;
    public bool isSelected;
    public bool isDefined;
    public bool isEntryPoint;

    public GUIStyle style;
    public GUIStyle defaultNodeStyle;
    public GUIStyle selectedNodeStyle;

    public ConnectionPoint inPoint;
    public ConnectionPoint outPoint;

    //Reflection Specific Stuff
    public string input;
    public string type; //the type.ToString() of the class containing the method/member
    public string assemblyPath;
    public int index; //The index of this overloaded function definition when GetMethods is called

    public Action<Node> OnRemoveNode;

    MethodInfo[] methodDefinitions;
    public MethodInfo currentMethod;
    
    public List<Parameter> paramList;

    public BlueprintData blueprint; //not currently used

    public Func<object, object[], object> function;
    public object[] passArgs;
    public UnityEngine.Object target;
    public object actualTarget;
    public Type returnType;
    public Component comp; //not currently used

    public int ID; //The way to find the correct node to reference at runtime because serialization
    public int nextID;

    public Node nextNode; //When all nodes are instantiated and the initial find is complete we can use this

    //not used
    public string TypeJSON;

       
    public Node(Vector2 position, float width, float height, GUIStyle nodeStyle, GUIStyle selectStyle, GUIStyle inStyle, GUIStyle outStyle, Action<ConnectionPoint> inAction, Action<ConnectionPoint> outAction, Action<Node> onRemove)
    {
        rect = new Rect(position.x, position.y, width, height);
        style = nodeStyle;
        inPoint = new ConnectionPoint(this, ConnectionPointType.In, inStyle, inAction);
        outPoint = new ConnectionPoint(this, ConnectionPointType.Out, outStyle, outAction);
        defaultNodeStyle = nodeStyle;
        selectedNodeStyle = selectStyle;
        OnRemoveNode = onRemove;
        input = "";        
        paramList = new List<Parameter>();
        isDefined = false;
        
        Vector2 lerp = new Vector2(Mathf.Lerp(rect.position.x, rect.position.x + rect.width, 0.125f), Mathf.Lerp(rect.position.y, rect.position.y + rect.height, 0.2f));
        
        //The initial relative offset
        initPos = lerp - rect.position;                
        //Initial Label/Field dimensions for parameters
        initDimensions = new Vector2(rect.width * 0.75f, rect.height * 0.45f);              
    }   
    
    public Node(NodeData data, Action<ConnectionPoint> inAction, Action<ConnectionPoint> outAction, Action<Node> removeNode)
    {
        rect = data.rect;
        style = data.nodeStyle;
        initPos = data.initPos;
        initDimensions = data.initDimensions;
        input = data.input;
        type = data.type;
        index = data.index;
        OnRemoveNode = removeNode;
        assemblyPath = data.assemblyPath;
        inPoint = new ConnectionPoint(data.inPoint, data.inStyle, inAction);
        outPoint = new ConnectionPoint(data.outPoint, data.outStyle, outAction);
        ID = data.ID;
        nextID = data.nextID;

        isDefined = data.isDefined;
        isEntryPoint = data.isEntryPoint;
        
        if (isDefined)
        {
            if (paramList == null)
            {
                paramList = new List<Parameter>();
            }

            foreach(ParameterData par in data.paramList)
            {
                //TO-DO: Add loading functionality                
                paramList.Add(new Parameter(par.GetValue(), par.GetSystemType(), par.type, par.name));
            }
        }

    }

    public void Drag(Vector2 delta)
    {
        rect.position += delta;
    }
   
    public void Draw()
    {      
        inPoint.Draw();
        outPoint.Draw();
              
        GUI.Box(rect, title, style);

        if (!isDefined)
            input = EditorGUI.TextField(new Rect(rect.position + initPos, initDimensions), input);
        else
            EditorGUI.LabelField(new Rect(rect.position + initPos, initDimensions), input);

        if (paramList.Count > 0)
        {            
            for (int i = 0; i < paramList.Count; i++)
            {               
                Vector2 pos = rect.position + initPos;
                pos.y += (initDimensions.y * (i + 1));                
                Rect entry = new Rect(pos, initDimensions);
                paramList[i].rect = entry;
                if (paramList[i].draw != null)
                    paramList[i].draw();                          
            }
        }

        //if (blueprint.type != null)
        //{
        //    Debug.Log($"blueprint type " + blueprint.type);
        //}
        
    }

    public bool ProcessEvents(Event e)
    {
        switch(e.type)
        {
            case EventType.MouseDown:
                if (e.button == 0)
                {
                    if (rect.Contains(e.mousePosition))
                    {
                        isDragged = true;
                        GUI.changed = true;
                        isSelected = true;                        
                        style = selectedNodeStyle;
                    }

                    else
                    {
                        GUI.changed = true;
                        isSelected = false;
                        style = defaultNodeStyle;
                    }
                }
                
                if (e.button == 1 && rect.Contains(e.mousePosition))
                {                    
                    e.Use();
                    Interpreter.Instance.ParseKeywords(input, this);
                    methodDefinitions = Interpreter.Instance.GetFunctionDefinitions(input, out type , out assemblyPath);
                    ProcessContextMenu();
                }
                break;

            case EventType.MouseDrag:
                if (e.button == 0 && isDragged && isSelected)
                {
                    Drag(e.delta);
                    e.Use();
                    return true;
                }
                break;

            case EventType.KeyDown:
                if (e.keyCode == KeyCode.Return)
                {
                    e.Use();
                    Interpreter.Instance.ParseKeywords(input, this);
                    Debug.Log("In return");
                }

                break;
        }

        return false;
    }

    void ProcessContextMenu()
    {
        GenericMenu menu = new GenericMenu();
        menu.AddItem(new GUIContent("Remove node"), false, OnClickRemoveNode);
        
        if (methodDefinitions != null)
        {
            foreach (MethodInfo m in methodDefinitions)
            {
                ParameterInfo[] args = m.GetParameters();
                string final = m.Name + "(";
               
                for (int i = 0; i < args.Length; i++)
                {
                    final += " " + args[i].ParameterType.Name + " " + args[i].Name;

                    if (i < args.Length - 1)
                        final += ", ";

                }

                final += " )";
                
                menu.AddItem(new GUIContent(final), false, ChangeToMethod, m);
            }
        }

        //else
        //    Debug.Log("Method Definitions is null");

        menu.ShowAsContext();
    }

    void OnClickRemoveNode()
    {
        if (OnRemoveNode != null)
        {
            OnRemoveNode.Invoke(this);
        }
    }

    void ChangeToMethod(object method)
    {
        MethodInfo info = (MethodInfo)method;

        ParameterInfo[] args = info.GetParameters();

        foreach(ParameterInfo p in args)                    
            paramList.Add(new Parameter(p.ParameterType));
        
        if (paramList.Count == 1)                    
            rect = new Rect(rect.x, rect.y, rect.width, rect.height * (paramList.Count + 1));
        

        else if (paramList.Count > 1)
            rect = new Rect(rect.x, rect.y, rect.width, rect.height * (paramList.Count));
        
        foreach(Parameter p in paramList)
        {
            p.GetFieldType();
        }

        for (int i = 0; i < methodDefinitions.Length; i++)
        {
            if (methodDefinitions[i] == info)
            {
                Debug.Log("Index found");
                index = i;
            }
        }

        returnType = info.ReturnType;
        currentMethod = info;
        //Debug.Log($"Function pointer is {currentMethod.MethodHandle.GetFunctionPointer()}");
        //Debug.Log($"Function metadata token is {currentMethod.MetadataToken}");
        //function = currentMethod.Bind();
        
                
        isDefined = true;
    }
}


/*  Old testing stuff
        //blueprint.ptr = currentMethod.MetadataToken;
        //MethodBody body = function.GetMethodInfo().GetMethodBody();
        //byte[] compiled = body.GetILAsByteArray();
        //string str = "";
        //
        //foreach(byte b in compiled)
        //{
        //    str += (char)b;
        //}
        //Debug.Log("compiled lambda is: " + str);
        //TypeJSON = currentMethod.DeclaringType.ToString() + " " + currentMethod.Name;
        //Debug.Log(TypeJSON);
        //blueprint.CompileTest = str;
        //
        //blueprint.typeTest = currentMethod.ToString();
        //blueprint.type = currentMethod.ReturnType;
        //
        ////float test = 4.0f;
        ////SerializedProperty property = (SerializedProperty)test;
        //blueprint.enumTest = SerializedPropertyType.Color;

        //Debug.Log($"Method body {}")

        //Module.ResolveMethod((Int32)blueprint.ptr);
        //Delegate del;
        //Type someType = typeof(int);
        //Module mod = Assembly.GetAssembly(someType).ManifestModule;
        //MethodBase methodTest = mod.ResolveMethod(blueprint.ptr);        
        //Debug.Log($"Method test {methodTest.ToString()} {methodTest.DeclaringType}");
        //Debug.Log($"current method compare {currentMethod.Name} {currentMethod.ToString()}"); 
*/


public class Parameter
{
    public string name;
    public object arg;
    public Type type;
    public Rect rect;
    public Action draw;
    public ParameterData.ParamType paramType;

    public Parameter() { }


    public Parameter(Type objType)
    {
        type = objType;
    }

    public Parameter(object obj, Type objType, ParameterData.ParamType parType, string label)
    {
        arg = obj;
        type = objType;
        name = label;
        paramType = parType;
    }

    T GetType<T>() where T : new()
    {
        Type type = typeof(T);

        if (arg == null)
            arg = new T();

        return (T)arg;
    }

    public void GetFieldType()
    {
        if (type.IsPrimitive)
        {
            if (type == typeof(bool))
            {
                draw = delegate { arg = EditorGUI.Toggle(rect, GetType<bool>()); };
                paramType = ParameterData.ParamType.Bool;
                return;
            }

            if (type == typeof(int))
            {
                draw = delegate { arg = EditorGUI.IntField(rect, GetType<int>()); };
                paramType = ParameterData.ParamType.Int;
                return;
            }

            if (type == typeof(long))
            {
                draw = delegate { arg = EditorGUI.LongField(rect, GetType<long>()); };
                paramType = ParameterData.ParamType.Long;
                return;
            }

            if (type == typeof(float))
            {
                draw = delegate { arg = EditorGUI.FloatField(rect, GetType<float>()); };
                paramType = ParameterData.ParamType.Float;
                return;
            }

            if (type == typeof(double))
            {
                draw = delegate { arg = EditorGUI.DoubleField(rect, GetType<double>()); };
                paramType = ParameterData.ParamType.Double;
                return;
            }

            Debug.Log($"{type} is not a primitive");
        }

        else
        {
            if (type.BaseType == typeof(UnityEngine.Object))
            {
                Debug.Log($"Base type of parameter type: {type} is Unity Object");
            }

            if (type == typeof(string))
            {
                draw = delegate { arg = EditorGUI.TextField(rect, (string)System.Convert.ChangeType(arg, typeof(string))); };
                paramType = ParameterData.ParamType.String;
                return;
            }

            if (type == typeof(Vector2))
            {
                //draw = delegate { arg = EditorGUI.Vector2Field(rect, name, GetType<Vector2>()); };                    
                arg = new float[] { 0.0f, 0.0f };
                draw = delegate { EditorGUI.MultiFloatField(rect, new GUIContent[] { new GUIContent("X"), new GUIContent("Y") }, (float[])arg); };
                paramType = ParameterData.ParamType.Vec2;
                return;
            }

            if (type == typeof(Vector3))
            {
                //draw = delegate { arg = EditorGUI.Vector3Field(rect, name, GetType<Vector3>()); }; 
                arg = new float[] { 0.0f, 0.0f, 0.0f };
                draw = delegate { EditorGUI.MultiFloatField(rect, new GUIContent[] { new GUIContent("X"), new GUIContent("Y"), new GUIContent("Z") }, (float[])arg); };
                paramType = ParameterData.ParamType.Vec3;
                return;
            }

            if (type == typeof(Vector4))
            {
                //draw = delegate { arg = EditorGUI.Vector4Field(rect, name, GetType<Vector4>()); };                                        
                arg = new float[] { 0.0f, 0.0f, 0.0f, 0.0f };
                draw = delegate { EditorGUI.MultiFloatField(rect, new GUIContent[] { new GUIContent("X"), new GUIContent("Y"), new GUIContent("Z"), new GUIContent("W") }, (float[])arg); };
                paramType = ParameterData.ParamType.Vec4;
                return;
            }

            if (type == typeof(Rect))
            {
                //draw = delegate { arg = EditorGUI.RectField(rect, GetType<Rect>()); };
                arg = new float[] { 0.0f, 0.0f, 0.0f, 0.0f };
                draw = delegate { EditorGUI.MultiFloatField(rect, new GUIContent[] { new GUIContent("X"), new GUIContent("Y"), new GUIContent("W"), new GUIContent("H") }, (float[])arg); };
                paramType = ParameterData.ParamType.Rect;
                return;
            }

            if (type == typeof(Color))
            {
                draw = delegate { arg = EditorGUI.ColorField(rect, name, GetType<Color>()); };
                paramType = ParameterData.ParamType.Color;
                return;
            }

            //Found Here: https://stackoverflow.com/questions/15855881/create-instance-of-unknown-enum-with-string-value-using-reflection-in-c-sharp
            if (type.BaseType == typeof(System.Enum))
            {
                if (arg == null)
                    arg = Enum.ToObject(type, 0);

                draw = delegate { arg = EditorGUI.EnumPopup(rect, (System.Enum)System.Convert.ChangeType(arg, type)); };
                paramType = ParameterData.ParamType.Enum;

                return;
            }

            if (type == typeof(UnityEngine.Object))
            {
                draw = delegate { arg = EditorGUI.ObjectField(rect, GetType<UnityEngine.Object>(), type, true); };
                paramType = ParameterData.ParamType.Object;
                return;
            }

            Debug.Log("Type not found");

        }
    }

}
