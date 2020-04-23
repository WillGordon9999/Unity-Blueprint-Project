using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;
using System.Reflection;

public enum NodeType { Entry_Point, Function, Field_Get, Field_Set, Property_Get, Property_Set, Conditional };

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
    public ConnectionPoint falsePoint;

    //Reflection Specific Stuff
    public string input;
    public string type; //the type.ToString() of the class containing the method/member
    public string assemblyPath;
    public int index; //The index of this overloaded function definition when GetMethods is called

    public Action<Node> OnRemoveNode;

    public MethodInfo currentMethod;    
    public FieldInfo fieldVar;
    public PropertyInfo propertyVar;
    public ConstructorInfo constructorMethod;

    public InterpreterData metaData = null;

    public List<Parameter> paramList;

    public NodeType nodeType;

    //Operations Core    
    public object actualTarget;        
    public string varName;
    public bool isStatic;

    //Function Specific
    public Func<object, object[], object> function;
    public object[] passArgs;
    public bool isReturning;
    public Type returnType;
    public object returnObj; //The object which ideally will be accessible by next nodes    

    //None of these are being used
    public enum ReturnVarType { None, Var, Field, Property }; //Deprecated
    public ReturnVarType retType;

    public bool isSpecial = false; //Basically the target to call on is the BlueprintComponent
    
    public string returnInput;

    public Parameter returnEntry;
          
    //For setting fields or properties
    public Parameter literalField; //Use a literal value
    public Parameter varField; //Access a variable

    //Determining if you are using a literal or var is still a work in progress
    public bool isVar { get; set; } //For when the node is a field set or property set, and the the value in question is being set to a variable

    public Blueprint blueprint; //With the primary idea being this is used for variable access

    public bool isContextual = false;

    //Should these have default values?
    public int ID; //The way to find the correct node to reference at runtime because serialization
    public int nextID;
    public int prevID;
    public int falseID;

    public Node nextNode; //When all nodes are instantiated and the initial find is complete we can use this
    public Node prevNode;
    public Node falseNode; //For conditionals

    string initField = "Init Field";
           
    public Node(Vector2 position, float width, float height, GUIStyle nodeStyle, GUIStyle selectStyle, GUIStyle inStyle, GUIStyle outStyle, Action<ConnectionPoint> inAction, Action<ConnectionPoint> outAction, Action<Node> onRemove)
    {
        rect = new Rect(position.x, position.y, width, height);
        style = nodeStyle;

        inPoint = new ConnectionPoint(this, ConnectionPointType.In, inStyle, inAction);
        outPoint = new ConnectionPoint(this, ConnectionPointType.Out, outStyle, outAction);
        falsePoint = new ConnectionPoint(this, ConnectionPointType.False, outStyle, outAction);

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

        inPoint = new ConnectionPoint(data.inPoint, this, data.inStyle, inAction);
        outPoint = new ConnectionPoint(data.outPoint, this, data.outStyle, outAction);
        falsePoint = new ConnectionPoint(data.falsePoint, this, data.outStyle, outAction);

        ID = data.ID;
        nextID = data.nextID;
        prevID = data.prevID;
        falseID = data.falseID;
        isContextual = data.isContextual;

        nodeType = data.nodeType;
        isStatic = data.isStatic;
        isDefined = data.isDefined;
        isEntryPoint = data.isEntryPoint;
        isVar = data.isVar;

        isReturning = data.isReturning;
        returnInput = data.returnInput;
        
        isSpecial = data.isSpecial;
        varName = data.varName;
        retType = data.retType;
        
        if (data.literalField != null)
            literalField = new Parameter(data.literalField.GetValue(), data.literalField.GetSystemType(), data.literalField.type);

        if (data.varField != null)
            varField = new Parameter(data.varField.GetValue(), data.varField.GetSystemType(), data.varField.type);

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
        //Draw Connection Points
        inPoint.Draw();
        outPoint.Draw();

        //Draw False Point if you're a branch
        if (nodeType == NodeType.Conditional)
            falsePoint.Draw();

        //Draw the box
        GUI.Box(rect, "", style);

        //Draw accordingly to whether node has definition or not
        if (!isDefined)
        {
            GUI.SetNextControlName(initField);
            input = EditorGUI.TextField(new Rect(rect.position + initPos, initDimensions), input);
        }
        else
            EditorGUI.LabelField(new Rect(rect.position + initPos, initDimensions), input);

        //Function drawing
        if (nodeType == NodeType.Function)
        {
            if (paramList.Count > 0)
            {
                for (int i = 0; i < paramList.Count; i++)
                {
                    Vector2 pos = rect.position + initPos;
                    pos.y += (initDimensions.y * (i + 1));
                    Rect entry = new Rect(pos, initDimensions);
                    paramList[i].rect = entry;

                    //Equivalent to: if draw != null                
                    paramList[i].draw?.Invoke();

                    if (isReturning && i == paramList.Count - 1)
                    {
                        pos.y += (initDimensions.y * (i + 1));
                        entry = new Rect(pos, initDimensions);
                        returnInput = GUI.TextField(entry, returnInput);
                    }
                }
            }
        }

        //Get
        if (nodeType == NodeType.Field_Get || nodeType == NodeType.Property_Get)
        {
            Vector2 pos = rect.position + initPos;
            pos.y += initDimensions.y;
            Rect entry = new Rect(pos, initDimensions);
            returnInput = GUI.TextField(entry, returnInput);
        }

        //Set        
        if (nodeType == NodeType.Field_Set || nodeType == NodeType.Property_Set)
        {
            Vector2 pos = rect.position + initPos;
            pos.y += initDimensions.y;
            Rect entry = new Rect(pos, initDimensions);

            literalField.rect = entry;            
            literalField.draw?.Invoke();

            pos.y += initDimensions.y;
            entry.position = pos;

            varField.rect = entry;
            varField.draw?.Invoke();
        }

        //Conditional
        if (nodeType == NodeType.Conditional)
        {
            if (paramList.Count > 0)
            {                
                Vector2 pos = rect.position + initPos;
                pos.y += (initDimensions.y * 2);
                Rect entry = new Rect(pos, initDimensions);
                paramList[0].rect = entry;

                //Equivalent to: if draw != null                
                paramList[0].draw?.Invoke();                              
            }
        }
       
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
                    
                    if (!isDefined)
                        Interpreter.Instance.Compile(input, NodeEditor.current, ref metaData, this);

                    if (isDefined && isReturning)
                        Interpreter.Instance.Compile(returnInput, NodeEditor.current, ref metaData, this);

                    if (metaData == null)
                        Debug.LogWarning("Meta Data is null");

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
        }

        //Check for Special Functions
        if (e.keyCode == KeyCode.Return && GUI.GetNameOfFocusedControl() == initField)
        {
            //e.Use();
            if (!isDefined)
            {
                Interpreter.Instance.ParseKeywords(input, this);
                return true;
            }
            
        }

        return false;
    }

    void ProcessContextMenu()
    {
        GenericMenu menu = new GenericMenu();
        menu.AddItem(new GUIContent("Remove node"), false, OnClickRemoveNode);
                
        if (metaData != null)
        {
            if (metaData.selectedType == null)
            {
                if (metaData.isKeyWord)
                {
                    isEntryPoint = true;
                    return;
                }

                if (metaData.types != null)
                {
                    if (metaData.types.Length > 1)
                    {
                        for (int i = 0; i < metaData.types.Length; i++)
                        {
                            menu.AddItem(new GUIContent(metaData.types[i].ToString()), false, SelectObjectType, i);
                        }
                    }
                }
               
            }

            else
            {
                if (metaData.constructors != null)
                {
                    foreach(ConstructorInfo c in metaData.constructors)
                    {
                        if (c != null)
                        {
                            ParameterInfo[] args = c.GetParameters();
                            string final = c.Name + "(";

                            for (int i = 0; i < args.Length; i++)
                            {
                                final += " " + args[i].ParameterType.Name + " " + args[i].Name;

                                if (i < args.Length - 1)
                                    final += ", ";

                            }

                            final += " )";

                            menu.AddItem(new GUIContent(final), false, ChangeToConstructor, c);
                        }
                    }
                }

                if (metaData.methods != null)
                {
                    foreach (MethodInfo m in metaData.methods)
                    {
                        if (m != null)
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
                }
                
                if (metaData.fields != null)
                {
                    foreach (FieldInfo f in metaData.fields)
                    {                        
                        if (f != null)
                        {
                            if (metaData.access == InterpreterData.AccessType.Both)
                            {
                                string final = "Get " + f.FieldType + " " + f.Name;

                                if (!isDefined && !isReturning)
                                    menu.AddItem(new GUIContent(final), false, ChangeToField, "Get"); //This will be supported later

                                final = "Set " + f.FieldType + " " + f.Name;

                                if (!isDefined && !isReturning)
                                    menu.AddItem(new GUIContent(final), false, ChangeToField, "Set");
                               
                            }
                        }
                    }
                }
                
                if (metaData.properties != null)
                {
                    foreach (PropertyInfo p in metaData.properties)
                    {                        
                        if (p != null)
                        {

                            if (metaData.access == InterpreterData.AccessType.Both)
                            {
                                string final = "Get " + p.PropertyType + " " + p.Name;

                                if (!isDefined && !isReturning)
                                    menu.AddItem(new GUIContent(final), false, ChangeToProperty, "Get"); //This will be supported later

                                final = "Set " + p.PropertyType + " " + p.Name;

                                if (!isDefined && !isReturning)
                                    menu.AddItem(new GUIContent(final), false, ChangeToProperty, "Set");
                               
                            }

                            else if (metaData.access == InterpreterData.AccessType.Get)
                            {
                                string final = "Get " + p.PropertyType + " " + p.Name;

                                if (!isDefined && !isReturning)
                                    menu.AddItem(new GUIContent(final), false, ChangeToProperty, "Get"); //This will be supported later
                            }

                            else if (metaData.access == InterpreterData.AccessType.Set)
                            {
                                string final = "Set " + p.PropertyType + " " + p.Name;

                                if (!isDefined && !isReturning)
                                    menu.AddItem(new GUIContent(final), false, ChangeToProperty, "Set");
                              
                            }
                        }
                    }
                }
            }
        }

        menu.ShowAsContext();
    }
   
    void ChangeToField(object input)
    {
        string result = (string)input;

        if (metaData.fields != null)
        {
            //targetVar = metaData.varRef;
            varName = metaData.varName;
            this.input = metaData.input;
            type = metaData.selectedType.ToString();
            assemblyPath = metaData.selectedType.Assembly.Location;            

            if (result == "Get")
            {
                nodeType = NodeType.Field_Get;
                fieldVar = metaData.fields[0];
                isStatic = fieldVar.IsStatic;
                rect = new Rect(rect.x, rect.y, rect.width, rect.height * 2.0f);
                isDefined = true;
            }

            if (result == "Set")
            {                
                nodeType = NodeType.Field_Set;

                fieldVar = metaData.fields[0];
                isStatic = fieldVar.IsStatic;

                literalField = new Parameter(fieldVar.FieldType);
                literalField.GetFieldType();

                varField = new Parameter(typeof(string));
                varField.GetFieldType();

                //Prepare rect for new appearance
                rect = new Rect(rect.x, rect.y, rect.width, rect.height * 3.0f);
                isDefined = true;
            }           
        }       
    }

    void ChangeToProperty(object input)
    {
        string result = (string)input;

        if (metaData.properties != null)
        {
            //targetVar = metaData.varRef;
            varName = metaData.varName;
            this.input = metaData.input;
            type = metaData.selectedType.ToString();
            assemblyPath = metaData.selectedType.Assembly.Location;
            
            if (result == "Get")
            {
                nodeType = NodeType.Property_Get;
                propertyVar = metaData.properties[0];
                rect = new Rect(rect.x, rect.y, rect.width, rect.height * 2.0f);
                isDefined = true;
            }

            if (result == "Set")
            {
                nodeType = NodeType.Property_Set;

                propertyVar = metaData.properties[0];

                literalField = new Parameter(propertyVar.PropertyType);
                literalField.GetFieldType();

                varField = new Parameter(typeof(string));
                varField.GetFieldType();

                //Prepare rect for new appearance
                rect = new Rect(rect.x, rect.y, rect.width, rect.height * 3.0f);
                isDefined = true;
            }
        }
    }

    void SelectObjectType(object index)
    {
        metaData.selectedType = metaData.types[(int)index];        
        metaData.selectedAsm = metaData.selectedType.Assembly;       
        Interpreter.Instance.Compile(input, NodeEditor.current, ref metaData);
    }

    void OnClickRemoveNode()
    {
        if (OnRemoveNode != null)
        {
            OnRemoveNode.Invoke(this);
        }
    }

    public void ChangeToSpecialMethod(object method)
    {
        nodeType = NodeType.Function;          
        isSpecial = true;        

        MethodInfo info = (MethodInfo)method;

        if (info != null)
        {
            type = info.ReflectedType.ToString();
            assemblyPath = info.ReflectedType.Assembly.Location;
        }

        float initHeight = rect.height;

        ParameterInfo[] args = info.GetParameters();

        foreach (ParameterInfo p in args)
            paramList.Add(new Parameter(p.ParameterType));

        if (paramList.Count == 1)
            rect = new Rect(rect.x, rect.y, rect.width, initHeight * (paramList.Count + 1));


        else if (paramList.Count > 1)
            rect = new Rect(rect.x, rect.y, rect.width, initHeight * (paramList.Count));

        foreach (Parameter p in paramList)
        {
            p.GetFieldType();
        }

        returnType = info.ReturnType;
        currentMethod = info;

        if (returnType != typeof(void))
        {
            isReturning = true;
            retType = ReturnVarType.Var;
            rect = new Rect(rect.x, rect.y, rect.width, initHeight * (paramList.Count + 1));
            returnEntry = new Parameter(typeof(string));
        }

        else
            isReturning = false;


        isDefined = true;

    }

    public void ChangeToConditional()
    {
        nodeType = NodeType.Conditional;
        rect = new Rect(rect.x, rect.y, rect.width, rect.height * 2.0f);
        paramList.Add(new Parameter(typeof(string))); //should be a variable
        
    }

    public void ChangeToMethod(object method)
    {
        nodeType = NodeType.Function;

        MethodInfo info = (MethodInfo)method;

        if (metaData != null && !isSpecial)
        {
            //targetVar = metaData.varRef;
            varName = metaData.varName;
            input = metaData.input;
            type = metaData.selectedType.ToString();
            isStatic = info.IsStatic;
            assemblyPath = metaData.selectedType.Assembly.Location;
        }

        else
        {
            type = info.ReflectedType.ToString();
            isStatic = info.IsStatic;
            assemblyPath = info.ReflectedType.Assembly.Location;            
        }

        float initHeight = rect.height;

        ParameterInfo[] args = info.GetParameters();

        foreach(ParameterInfo p in args)                    
            paramList.Add(new Parameter(p.ParameterType));
        
        if (paramList.Count == 1)                    
            rect = new Rect(rect.x, rect.y, rect.width, initHeight * (paramList.Count + 1));
        

        else if (paramList.Count > 1)
            rect = new Rect(rect.x, rect.y, rect.width, initHeight * (paramList.Count));
        
        foreach(Parameter p in paramList)
        {
            p.GetFieldType();
        }

        //for (int i = 0; i < methodDefinitions.Length; i++)

        if (metaData != null && !isSpecial)
        {
            for (int i = 0; i < metaData.methods.Length; i++)
            {
                if (metaData.methods[i] == info)
                {
                    Debug.Log("Index found");
                    index = i;
                }
            }
        }

        returnType = info.ReturnType;
        currentMethod = info;       

        if (returnType != typeof(void))
        {
            isReturning = true;            
            rect = new Rect(rect.x, rect.y, rect.width, initHeight * (paramList.Count + 1));
            returnEntry = new Parameter(typeof(string));
            retType = ReturnVarType.Var;
        }

        else        
            isReturning = false;
        

        isDefined = true;
    }

    public void ChangeToConstructor(object method)
    {
        nodeType = NodeType.Function;

        ConstructorInfo info = (ConstructorInfo)method;

        if (metaData != null && !isSpecial)
        {
            //targetVar = metaData.varRef;
            varName = metaData.varName;
            input = metaData.input;
            type = metaData.selectedType.ToString();
            isStatic = info.IsStatic;
            assemblyPath = metaData.selectedType.Assembly.Location;
        }

        else
        {
            type = info.ReflectedType.ToString();
            isStatic = info.IsStatic;
            assemblyPath = info.ReflectedType.Assembly.Location;
        }

        float initHeight = rect.height;

        ParameterInfo[] args = info.GetParameters();

        foreach (ParameterInfo p in args)
            paramList.Add(new Parameter(p.ParameterType));

        if (paramList.Count == 1)
            rect = new Rect(rect.x, rect.y, rect.width, initHeight * (paramList.Count + 1));


        else if (paramList.Count > 1)
            rect = new Rect(rect.x, rect.y, rect.width, initHeight * (paramList.Count));

        foreach (Parameter p in paramList)
        {
            p.GetFieldType();
        }

        //for (int i = 0; i < methodDefinitions.Length; i++)

        if (metaData != null && !isSpecial)
        {
            for (int i = 0; i < metaData.methods.Length; i++)
            {
                if (metaData.methods[i] == info)
                {
                    Debug.Log("Index found");
                    index = i;
                }
            }
        }

        returnType = info.DeclaringType;
        constructorMethod = info;

        if (returnType != typeof(void))
        {
            isReturning = true;
            rect = new Rect(rect.x, rect.y, rect.width, initHeight * (paramList.Count + 1));
            returnEntry = new Parameter(typeof(string));
            retType = ReturnVarType.Var;
        }

        else
            isReturning = false;


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



