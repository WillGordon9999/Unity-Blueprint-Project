using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.UIElements;
using System;
using System.Reflection;


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



