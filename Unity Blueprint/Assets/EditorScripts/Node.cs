using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;
using System.Reflection;
using System.Linq;
using System.Linq.Expressions;

public enum NodeType { Entry_Point, Function, Constructor, Field_Get, Field_Set, Property_Get, Property_Set, Conditional, Operation };

public class Node
{
    public Rect rect;   //Initial rect
    public Rect final;  //drawing rect
    //Rect fieldRect;
    
    public Vector2 initPos;         //Init position of node
    public Vector2 initDimensions;  //Init dimensions of node
    //public string title;
    public bool isDragged;          //UI Control
    public bool isSelected;         //UI Control
    public bool isDefined;          //Is this node a declared operation, field, property, function or constructor
    public bool isEntryPoint;       //Is this node the entry point to one of the Unity Messages or virtuals

    //Unclear if still used
    public GUIStyle style;
    public GUIStyle defaultNodeStyle;
    public GUIStyle selectedNodeStyle;

    public ConnectionPoint inPoint;
    public ConnectionPoint outPoint;
    public ConnectionPoint falsePoint;

    //Reflection Specific Stuff
    public string input;    //The Raw input from the user
    public string type;     //the type.ToString() of the class containing the method/member

    public string assemblyPath;
    public string declaringTypeAsmPath;
    public string declaringBaseTypeAsmPath;
    public string reflectedTypeAsmPath;
    public string reflectedTypeBaseAsmPath;

    public string nameSpace;
    public int index; //The index of this overloaded function definition when GetMethods is called
    public bool isVirtual = false;
    //Generic Specific
    public bool isGenericFunction; //Controls Parameter Generation in Constructors and Methods
    Type[] genericTypes;           //Cache for selecting a type in a generic function
    Assembly[] genericAsms;        //Cache for corresponding Assemblies
    Parameter genericParameterToSet;   //The reference to target generic parameter to update upon selection

    public Action<Node> OnRemoveNode;

    public MethodInfo currentMethod;    //MethodInfo reference 
    public FieldInfo fieldVar;          //FieldInfo reference
    public PropertyInfo propertyVar;    //PropertyInfo reference
    public ConstructorInfo constructorMethod;   //Constructor Reference

    public InterpreterData metaData = null;

    public List<Parameter> paramList;

    public NodeType nodeType;
     
    public List<string> passInParams; //THIS IS PURELY FOR LABELLING ENTRY POINT ARGUMENTS FOR INSTANCE OnTriggerEnter

    //Operations Core    
    //public object actualTarget;        
    public string varName; //The name of the target variable to execute member call on
    public bool isStatic;
    public string operatorStr;
    public string operatorMethodName;

    //Function Specific
    public Func<object, object[], object> function;
    public object[] passArgs;
    public bool isReturning;
    public Type returnType;
    public object returnObj; //The object which ideally will be accessible by next nodes    

    //None of these are being used
    //public enum ReturnVarType { None, Var, Field, Property }; //Deprecated
    //public ReturnVarType retType;

    public bool isSpecial = false; //Implemented in many places but not actively used in a meaningful capacity
    
    public string returnInput = ""; // The variable name to return to if the function returns

    //public Parameter returnEntry;
          
    //For setting fields or properties
    //public Parameter literalField; //Use a literal value
    //public Parameter varField; //Access a variable
    //public Parameter setMember;
    
    //Determining if you are using a literal or var is still a work in progress
    //public bool isVar { get; set; } //For when the node is a field set or property set, and the the value in question is being set to a variable

    public Blueprint blueprint; //With the primary idea being this is used for variable access

    //Game Specific 
    public bool hasCost = false; //No longer used in a meaningful capacity

    public bool isContextual = false; //Is the type the node is referencing in the previous node?

    //Should these have default values?
    public int ID; //The way to find the correct node to reference at runtime because serialization
    public int nextID;
    public int prevID;
    public int falseID;

    public Node nextNode; //When all nodes are instantiated and the initial find is complete we can use this
    public Node prevNode;
    public Node falseNode; //For conditionals

    string initField = "Init Field";

    //New GUI
    public Vector2 startPos;
    public Vector2 endPos;
           
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
        startPos = new Vector2(Mathf.Lerp(rect.position.x, rect.position.x + rect.width, 0.25f), Mathf.Lerp(rect.position.y, rect.position.y + rect.height, 0.25f));
        endPos = new Vector2(Mathf.Lerp(rect.position.x, rect.position.x + rect.width, 0.75f), Mathf.Lerp(rect.position.y, rect.position.y + rect.height, 0.75f));

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
        operatorStr = data.operatorStr;
        operatorMethodName = data.operatorMethodName;
        nameSpace = data.nameSpace;
        isGenericFunction = data.isGenericFunction;

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
        //isVar = data.isVar;

        isReturning = data.isReturning;
        returnInput = data.returnInput;

        declaringTypeAsmPath = data.declaringTypeAsmPath;
        declaringBaseTypeAsmPath = data.declaringBaseTypeAsmPath;
        reflectedTypeAsmPath = data.reflectedTypeAsmPath;
        reflectedTypeBaseAsmPath = data.reflectedTypeBaseAsmPath;

        isSpecial = data.isSpecial;
        varName = data.varName;
        //retType = data.retType;
        isVirtual = data.isVirtual;

        hasCost = data.hasCost;

        if (data.passInParams != null)
        {
            passInParams = new List<string>();

            foreach (string str in data.passInParams)
                passInParams.Add(str);
        }

        if (isReturning)
            returnType = Interpreter.Instance.LoadVarType(data.returnType, data.returnAsmPath);
               
        if (isDefined)
        {
            if (paramList == null)
            {
                paramList = new List<Parameter>();
            }

            foreach(ParameterData par in data.paramList)
            {
                //TO-DO: Add loading functionality                
                //paramList.Add(new Parameter(par.GetValue(), par.GetSystemType(), par.type, par.inputVar, par.varInput, par.shouldDraw, par.name, this, par.isGeneric, par.templateType));
                paramList.Add(new Parameter(par, this));
            }
        }

        //switch(nodeType)
        //{
        //    case NodeType.Function:
        //        currentMethod = Interpreter.Instance.LoadMethod(input, type, assemblyPath, index, isContextual);
        //        break;
        //    case NodeType.Constructor:
        //        constructorMethod = Interpreter.Instance.LoadConstructor(input, type, assemblyPath, index, isContextual);
        //        break;
        //    case NodeType.Field_Get:
        //        fieldVar = Interpreter.Instance.LoadField(input, type, assemblyPath, isContextual);
        //        break;
        //    case NodeType.Field_Set:
        //        fieldVar = Interpreter.Instance.LoadField(input, type, assemblyPath, isContextual);
        //        break;
        //    case NodeType.Property_Get:
        //        propertyVar = Interpreter.Instance.LoadProperty(input, type, assemblyPath, isContextual);
        //        break;
        //    case NodeType.Property_Set:
        //        propertyVar = Interpreter.Instance.LoadProperty(input, type, assemblyPath, isContextual);
        //        break;
        //
        //}

    }

    public void Drag(Vector2 delta)
    {
        rect.position += delta;
    }
   
    public void Draw(float zoomScale = 1.0f)
    {
        //Draw Connection Points
        inPoint.Draw(zoomScale);
        outPoint.Draw(zoomScale);

        //Draw False Point if you're a branch
        if (nodeType == NodeType.Conditional)
            falsePoint.Draw(zoomScale);

        final = new Rect(rect.position * zoomScale, new Vector2(rect.width, rect.height) * zoomScale);
        //Draw the box
        if (!Application.isPlaying)
            GUI.Box(rect, "", style);
        else
            GUI.Box(final, "");

        //Draw accordingly to whether node has definition or not
        if (!isDefined)
        {
            
            GUI.SetNextControlName(initField);
            //input = GUI.TextField(new Rect(rect.position + initPos, initDimensions), input);
            input = GUI.TextField(new Rect(final.position + initPos, initDimensions), input);

            //GUILayout.BeginArea(rect, style);
            //GUI.Box(rect, "", style);
            //GUILayout.BeginArea(new Rect(startPos, new Vector2(endPos.x - startPos.x, endPos.y - startPos.y)));
            //GUILayout.Box("", style);
            //
            //input = GUILayout.TextField(input);
            //GUILayout.EndArea();
        }
        //else
        //    //EditorGUI.LabelField(new Rect(rect.position + initPos, initDimensions), input);
        //    GUI.Label(new Rect(rect.position + initPos, initDimensions), input);

        if (nodeType == NodeType.Entry_Point && isDefined)
        {
            //
            GUI.Label(new Rect(final.position + initPos, initDimensions), input);

            if (passInParams != null)
            {
                for (int i = 0; i < passInParams.Count; i++)
                {
                    //Vector2 pos = rect.position + initPos;
                    Vector2 pos = final.position + initPos;
                    pos.y += (initDimensions.y * (i + 1));
                    Rect entry = new Rect(pos, initDimensions);
                    GUI.Label(entry, passInParams[i]);                                        
                }
            }
        }

        //Function drawing
        if (nodeType == NodeType.Function)
        {
            if (paramList.Count > 0)
            {
                //
                //GUILayout.BeginArea(new Rect(rect.position + initPos, new Vector2(rect.width - initPos.x, rect.height)));
                GUILayout.BeginArea(new Rect(final.position + initPos, new Vector2(final.width - initPos.x, final.height)));

                GUILayout.BeginVertical();
                                
                GUILayout.Label(input);

                for (int i = 0; i < paramList.Count; i++)
                {                    
                    paramList[i].draw?.Invoke(rect.width * 0.75f);

                    //if (isReturning && i == paramList.Count - 1)
                    //{
                    //    GUILayout.Label("Return to:");
                    //    //returnInput = GUILayout.TextField(returnInput, GUILayout.MaxWidth(rect.width * 0.75f));
                    //    returnInput = GUILayout.TextField(returnInput, GUILayout.MaxWidth(final.width * 0.75f));
                    //}
                                                                   
                    //Vector2 pos = rect.position + initPos;
                    //
                    //pos.y += (initDimensions.y * (i + 1));
                    //Rect entry = new Rect(pos, initDimensions);
                    //paramList[i].rect = entry;
                    //
                    ////Equivalent to: if draw != null                
                    //paramList[i].draw?.Invoke();
                    //
                    //if (isReturning && i == paramList.Count - 1)
                    //{
                    //    pos.y += (initDimensions.y * (i + 1));
                    //    entry = new Rect(pos, initDimensions);
                    //    returnInput = GUI.TextField(entry, returnInput);
                    //}
                }

                if (isReturning)
                {
                    GUILayout.Label("Return to:");
                    //returnInput = GUILayout.TextField(returnInput, GUILayout.MaxWidth(rect.width * 0.75f));
                    returnInput = GUILayout.TextField(returnInput, GUILayout.MaxWidth(final.width * 0.75f));
                }

                GUILayout.EndVertical();                
                GUILayout.EndArea();
            }

            else
            {
                GUILayout.BeginArea(new Rect(final.position + initPos, new Vector2(final.width - initPos.x, final.height)));

                GUILayout.BeginVertical();

                GUILayout.Label(input + "Call");

                GUILayout.EndVertical();
                GUILayout.EndArea();
            }
        }

        if (nodeType == NodeType.Constructor)
        {
            if (paramList.Count > 0)
            {
                //GUILayout.BeginArea(new Rect(rect.position + initPos, new Vector2(rect.width - initPos.x, rect.height)));
                GUILayout.BeginArea(new Rect(final.position + initPos, new Vector2(final.width - initPos.x, final.height)));

                GUILayout.BeginVertical();

                GUILayout.Label(input);

                for (int i = 0; i < paramList.Count; i++)
                {
                    //paramList[i].draw?.Invoke(rect.width * 0.75f);
                    paramList[i].draw?.Invoke(final.width * 0.75f);

                    //if (isReturning && i == paramList.Count - 1)
                    //{
                    //    GUILayout.Label("Return To");
                    //    //returnInput = GUILayout.TextField(returnInput, GUILayout.MaxWidth(rect.width * 0.75f));
                    //    returnInput = GUILayout.TextField(returnInput, GUILayout.MaxWidth(final.width * 0.75f));
                    //}


                    //Vector2 pos = rect.position + initPos;
                    //pos.y += (initDimensions.y * (i + 1));
                    //Rect entry = new Rect(pos, initDimensions);
                    //paramList[i].rect = entry;
                    //
                    ////Equivalent to: if draw != null                
                    //paramList[i].draw?.Invoke();
                    //
                    //if (isReturning && i == paramList.Count - 1)
                    //{
                    //    pos.y += (initDimensions.y * (i + 1));
                    //    entry = new Rect(pos, initDimensions);
                    //    returnInput = GUI.TextField(entry, returnInput);
                    //}
                }

                if (isReturning)
                {
                    GUILayout.Label("Return to:");
                    //returnInput = GUILayout.TextField(returnInput, GUILayout.MaxWidth(rect.width * 0.75f));
                    returnInput = GUILayout.TextField(returnInput, GUILayout.MaxWidth(final.width * 0.75f));
                }

                GUILayout.EndVertical();

                GUILayout.EndArea();
            }
        
            //Vector2 pos = rect.position + initPos;
            //pos.y += (initDimensions.y);
            //Rect entry = new Rect(pos, initDimensions);
            //returnInput = GUI.TextField(entry, returnInput);
        }
        
        //Get
        if (nodeType == NodeType.Field_Get || nodeType == NodeType.Property_Get)
        {
            //Vector2 pos = rect.position + initPos;
            //pos.y += initDimensions.y;
            //Rect entry = new Rect(pos, initDimensions);
            //returnInput = GUI.TextField(entry, returnInput);
            //GUILayout.BeginArea(new Rect(rect.position + initPos, new Vector2(rect.width - initPos.x, rect.height)));
            GUILayout.BeginArea(new Rect(final.position + initPos, new Vector2(final.width - initPos.x, final.height)));

            GUILayout.BeginVertical();

            GUILayout.Label(input);

            GUILayout.Label("Return to:");
            //returnInput = GUILayout.TextField(returnInput, GUILayout.MaxWidth(rect.width * 0.75f));
            returnInput = GUILayout.TextField(returnInput, GUILayout.MaxWidth(final.width * 0.75f));

            GUILayout.EndVertical();

            GUILayout.EndArea();
        }
        
        //Set        
        if (nodeType == NodeType.Field_Set || nodeType == NodeType.Property_Set)
        {
            //GUILayout.BeginArea(new Rect(rect.position + initPos, new Vector2(rect.width - initPos.x, rect.height)));            
            GUILayout.BeginArea(new Rect(final.position + initPos, new Vector2(final.width - initPos.x, final.height)));
            GUILayout.BeginVertical();

            GUILayout.Label(input);
            //paramList[0].draw?.Invoke(rect.width * 0.75f);
            paramList[0].draw?.Invoke(final.width * 0.75f);

            GUILayout.EndVertical();
            GUILayout.EndArea();

            //Vector2 pos = rect.position + initPos;
            //pos.y += initDimensions.y;
            //Rect entry = new Rect(pos, initDimensions);
            //paramList[0].rect = entry;
            //
            ////Equivalent to: if draw != null                
            //paramList[0].draw?.Invoke();

            //literalField.rect = entry;            
            //literalField.draw?.Invoke();
            //
            //pos.y += initDimensions.y;
            //entry.position = pos;
            //
            //varField.rect = entry;
            //varField.draw?.Invoke();
        }

        //Conditional
        if (nodeType == NodeType.Conditional)
        {
            if (paramList.Count > 0)
            {
                //GUILayout.BeginArea(new Rect(rect.position + initPos, new Vector2(rect.width - initPos.x, rect.height)));
                GUILayout.BeginArea(new Rect(final.position + initPos, new Vector2(final.width - initPos.x, final.height)));
                GUILayout.BeginVertical();

                GUILayout.Label(input);

                //paramList[0].draw?.Invoke(rect.width * 0.75f);
                paramList[0].draw?.Invoke(final.width * 0.75f);

                GUILayout.EndVertical();
                GUILayout.EndArea();

                //Vector2 pos = rect.position + initPos;
                //pos.y += (initDimensions.y * 2);
                //Rect entry = new Rect(pos, initDimensions);
                //paramList[0].rect = entry;
                //
                ////Equivalent to: if draw != null                
                //paramList[0].draw?.Invoke();                              
            }
        }
        
        if (nodeType == NodeType.Operation)
        {
            //GUILayout.BeginArea(new Rect(rect.position + initPos, new Vector2(rect.width - initPos.x, rect.height)));
            GUILayout.BeginArea(new Rect(final.position + initPos, new Vector2(final.width - initPos.x, final.height)));
            GUILayout.BeginVertical();

            GUILayout.Label(input);

            for (int i = 0; i < paramList.Count; i++)
            {
                //paramList[i].draw?.Invoke(rect.width * 0.75f);
                paramList[i].draw?.Invoke(final.width * 0.75f);

                if (isReturning && i == paramList.Count - 1)
                {
                    GUILayout.Label("Return to:");
                    //returnInput = GUILayout.TextField(returnInput, GUILayout.MaxWidth(rect.width * 0.75f));
                    returnInput = GUILayout.TextField(returnInput, GUILayout.MaxWidth(final.width * 0.75f));
                }
            }

            GUILayout.EndVertical();
            GUILayout.EndArea();

            //for (int i = 0; i < paramList.Count; i++)
            //{
            //    Vector2 pos = rect.position + initPos;
            //    pos.y += (initDimensions.y * (i + 1));
            //    Rect entry = new Rect(pos, initDimensions);
            //    paramList[i].rect = entry;
            //
            //    //Equivalent to: if draw != null                
            //    paramList[i].draw?.Invoke();
            //
            //    if (isReturning && i == paramList.Count - 1)
            //    {
            //        pos.y += (initDimensions.y * (i + 1));
            //        entry = new Rect(pos, initDimensions);
            //        returnInput = GUI.TextField(entry, returnInput);
            //    }
            //}
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
                    {
#if UNITY_EDITOR
                        if (!Application.isPlaying)
                            Interpreter.Instance.Compile(input, NodeEditor.current, ref metaData, this);
#endif
                        if (Application.isPlaying)
                            Interpreter.Instance.Compile(input, RealTimeEditor.Instance.current, ref metaData, this);
                    }
                                            

                    //if (isDefined && isReturning)
                    //    Interpreter.Instance.Compile(returnInput, NodeEditor.current, ref metaData, this);

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

        //Check for Special Functions - DOES NOT WORK WITH PASS IN PARAMS CHECK
        //if (e.keyCode == KeyCode.Return && GUI.GetNameOfFocusedControl() == initField)
        //{
        //    //e.Use();
        //    if (!isDefined)
        //    {
        //        //Questionable if we still want this
        //        Interpreter.Instance.ParseKeywords(input, this, ref blueprint.dataRef);
        //
        //        if (metaData != null)
        //        {
        //            if (metaData.isOperator && metaData.methods?.Length == 0)
        //            {
        //                Debug.Log("Node changing to operator node");
        //                ChangeToOperation(null);
        //            }
        //        }
        //        return true;
        //    }
        //    
        //}

        return false;
    }

    void ProcessContextMenu()
    {
#if UNITY_EDITOR
        if (!Application.isPlaying)
        {
            GenericMenu menu = new GenericMenu();
            menu.AddItem(new GUIContent("Remove node"), false, OnClickRemoveNode);

            if (isDefined)
            {
                if (nodeType == NodeType.Conditional)
                {
                    if (isContextual)
                        menu.AddItem(new GUIContent("Change to use variable"), false, ToggleContext, false);
                    else
                        menu.AddItem(new GUIContent("Change to use result of previous node"), false, ToggleContext, true);
                }

                for (int i = 0; i < paramList.Count; i++)
                {
                    if (!paramList[i].isGeneric)
                    {
                        if (!paramList[i].inputVar)
                            menu.AddItem(new GUIContent($"Change {paramList[i].name} to var"), false, ToggleParameterArg, i);
                        else
                            menu.AddItem(new GUIContent($"Change {paramList[i].name} to field"), false, ToggleParameterArg, i);
                    }

                    if (paramList[i].isGeneric)
                    {
                        menu.AddItem(new GUIContent("Search Types for " + paramList[i].name), false, SearchForType, paramList[i]);
                    }
                }

                if (genericParameterToSet != null)
                {
                    foreach(Type t in genericTypes)
                    {
                        menu.AddItem(new GUIContent($"Set {genericParameterToSet} Type to {t.ToString()}"), false, ChangeGenericParameterToSelectedType, t);
                    }
                }

                goto End;
            }

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
                        foreach (ConstructorInfo c in metaData.constructors)
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

                                if (hasCost)
                                    menu.AddItem(new GUIContent(final), false, ChangeToGameMethod, m);

                                if (!metaData.isOperator)
                                    menu.AddItem(new GUIContent(final), false, ChangeToMethod, m);
                                else
                                    menu.AddItem(new GUIContent(final), false, ChangeToOperation, m);
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

                    if (metaData.isOperator && metaData.methods.Length == 0)
                    {
                        Debug.Log("Node changing to operator node");
                        ChangeToOperation(null);
                    }
                }
            }

            End:

            menu.ShowAsContext();
        }
#endif        
        if (Application.isPlaying)
            ProcessContextMenuRealTime();
    }

    void ProcessContextMenuRealTime()
    {
        //GenericMenu menu = new GenericMenu();
        //menu.AddItem(new GUIContent("Remove node"), false, OnClickRemoveNode);
        RealTimeEditor.Instance.contextMenu.StopDraw();
        Rect menuRect = RealTimeEditor.Instance.contextMenu.rect;
        menuRect.position = new Vector2(rect.x, rect.y + rect.height);
        RealTimeEditor.Instance.contextMenu.rect = menuRect;

        Debug.Log("In Process Context Menu Real Time");
        RealTimeEditor.Instance.contextMenu.AddItem("Remove node", OnClickRemoveNode);

        if (isDefined)
        {
            if (nodeType == NodeType.Conditional)
            {
                if (isContextual)
                    RealTimeEditor.Instance.contextMenu.AddItem("Change to use variable", ToggleContext, false);
                else
                    RealTimeEditor.Instance.contextMenu.AddItem("Change to use result of previous node", ToggleContext, true);
            }

            for (int i = 0; i < paramList.Count; i++)
            {
                if (!paramList[i].isGeneric)
                {
                    if (!paramList[i].inputVar)
                        RealTimeEditor.Instance.contextMenu.AddItem($"Change {paramList[i].name} to var", ToggleParameterArg, i);
                    else
                        RealTimeEditor.Instance.contextMenu.AddItem($"Change {paramList[i].name} to field", ToggleParameterArg, i);
                }

                else
                    RealTimeEditor.Instance.contextMenu.AddItem("Search Types for " + paramList[i].name, SearchForType, paramList[i]);
            }

            if (genericParameterToSet != null)
            {
                foreach (Type t in genericTypes)
                {
                    RealTimeEditor.Instance.contextMenu.AddItem($"Set {genericParameterToSet} Type to {t.ToString()}", ChangeGenericParameterToSelectedType, t);
                }
            }

            goto End;
        }

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
                            RealTimeEditor.Instance.contextMenu.AddItem(metaData.types[i].ToString(), SelectObjectType, i);
                        }
                    }
                }

            }

            else
            {
                if (metaData.constructors != null)
                {
                    foreach (ConstructorInfo c in metaData.constructors)
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

                            RealTimeEditor.Instance.contextMenu.AddItem(final, ChangeToConstructor, c);
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

                            if (hasCost)
                                RealTimeEditor.Instance.contextMenu.AddItem(final, ChangeToGameMethod, m);

                            if (!metaData.isOperator)
                                RealTimeEditor.Instance.contextMenu.AddItem(final, ChangeToMethod, m);
                            else
                                RealTimeEditor.Instance.contextMenu.AddItem(final, ChangeToOperation, m);
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
                                    RealTimeEditor.Instance.contextMenu.AddItem(final, ChangeToField, "Get"); //This will be supported later

                                final = "Set " + f.FieldType + " " + f.Name;

                                if (!isDefined && !isReturning)
                                    RealTimeEditor.Instance.contextMenu.AddItem(final, ChangeToField, "Set");

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
                                    RealTimeEditor.Instance.contextMenu.AddItem(final, ChangeToProperty, "Get"); //This will be supported later

                                final = "Set " + p.PropertyType + " " + p.Name;

                                if (!isDefined && !isReturning)
                                    RealTimeEditor.Instance.contextMenu.AddItem(final, ChangeToProperty, "Set");

                            }

                            else if (metaData.access == InterpreterData.AccessType.Get)
                            {
                                string final = "Get " + p.PropertyType + " " + p.Name;

                                if (!isDefined && !isReturning)
                                    RealTimeEditor.Instance.contextMenu.AddItem(final, ChangeToProperty, "Get"); //This will be supported later
                            }

                            else if (metaData.access == InterpreterData.AccessType.Set)
                            {
                                string final = "Set " + p.PropertyType + " " + p.Name;

                                if (!isDefined && !isReturning)
                                    RealTimeEditor.Instance.contextMenu.AddItem(final, ChangeToProperty, "Set");

                            }
                        }
                    }
                }

                if (metaData.isOperator && metaData.methods.Length == 0)
                {
                    Debug.Log("Node changing to operator node");
                    ChangeToOperation(null);
                }
            }
        }

        End:

        RealTimeEditor.Instance.contextMenu.canDraw = true;
    }

    //TO-DO
    void SearchForType(object input)
    {
        Parameter par = (Parameter)input;
       
        if (Interpreter.Instance.FindType(par.templateType, out genericTypes, out genericAsms))
        {
            genericParameterToSet = par;
        }
    }

    void ChangeGenericParameterToSelectedType(object input)
    {
        Type type = (Type)input;
        genericParameterToSet.templateType = type.ToString();
        genericParameterToSet.templateTypeAsmPath = type.Assembly.Location;
        genericParameterToSet = null;
    }

    void ToggleContext(object input)
    {
        isContextual = (bool)input;
    }

    void ToggleParameterArg(object input)
    {
        int i = (int)input;
        paramList[i].ToggleType();
    }

    void ChangeToOperation(object input)
    {
        Debug.Log("Inside Change To Operation");
        nodeType = NodeType.Operation;
        MethodInfo info = (MethodInfo)input;

        operatorStr = metaData.operatorStr;

        if (info != null)
        {
            operatorMethodName = info.Name;

            float initHeight = rect.height;

            ParameterInfo[] args = info.GetParameters();

            //foreach (ParameterInfo p in args)
            for (int i = 0; i < args.Length; i++)
            {
                if (i > 0) //The first parameter should be the variable calling the operation
                paramList.Add(new Parameter(args[i].ParameterType, args[i].Name, this));
            }

            if (paramList.Count == 1)
                rect = new Rect(rect.x, rect.y, rect.width, initHeight * (paramList.Count));

            else if (paramList.Count > 1)
                rect = new Rect(rect.x, rect.y, rect.width, initHeight * (paramList.Count));

            foreach (Parameter p in paramList)
            {
                p.GetFieldType();
            }

            if (info.ReturnType != typeof(void))
            {
                isReturning = true;
                returnType = info.ReturnType;
                rect = new Rect(rect.x, rect.y, rect.width, initHeight * (paramList.Count + 1));
                //retType = ReturnVarType.Var;
            }
        }

        else
        {
            //Not performing var++ or var--
            if (!metaData.isIncrementOrDecrement)
            {
                paramList.Add(new Parameter(metaData.selectedType, "other", this));
                //rect = new Rect(rect.x, rect.y, rect.width, rect.height * 2.0f);

                //Not performing =, +=, -=, /=, *= etc
                if (!metaData.isAssign && !metaData.returnBool)
                {
                    isReturning = true;
                    returnType = metaData.selectedType;
                    rect = new Rect(rect.x, rect.y, rect.width, rect.height * 3.0f);
                }

                if (metaData.returnBool)
                {
                    isReturning = true;
                    returnType = typeof(bool);
                    rect = new Rect(rect.x, rect.y, rect.width, rect.height * 3.0f);
                }
            }          
        }

        foreach (Parameter p in paramList)
        {
            p.GetFieldType();
        }

        //targetVar = metaData.varRef;
        varName = metaData.varName;
        input = metaData.input;
        type = metaData.selectedType.ToString();
        isStatic = false;
        assemblyPath = metaData.selectedType.Assembly.Location;
        isDefined = true;
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
            //if (metaData.selectedType.DeclaringType.Assembly.Location != null)
            //    declaringTypeAsmPath = metaData.selectedType.DeclaringType.Assembly.Location;
            //
            //if (metaData.selectedType.DeclaringType.BaseType.Assembly.Location != null)
            //    declaringBaseTypeAsmPath = metaData.selectedType.DeclaringType.BaseType.Assembly.Location;
            //
            //if (metaData.selectedType.ReflectedType.Assembly.Location != null)
            //    reflectedTypeAsmPath = metaData.selectedType.ReflectedType.Assembly.Location;
            //
            //if (metaData.selectedType.ReflectedType.BaseType.Assembly.Location != null)
            //    reflectedTypeBaseAsmPath = metaData.selectedType.ReflectedType.BaseType.Assembly.Location;
            nameSpace = metaData.selectedType.Namespace;

            if (result == "Get")
            {
                nodeType = NodeType.Field_Get;
                fieldVar = metaData.fields[0];
                isStatic = fieldVar.IsStatic;
                rect = new Rect(rect.x, rect.y, rect.width, rect.height * 2.0f);
                returnType = fieldVar.FieldType;
                isReturning = true;
                isDefined = true;
            }

            if (result == "Set")
            {                
                nodeType = NodeType.Field_Set;

                fieldVar = metaData.fields[0];
                isStatic = fieldVar.IsStatic;

                //literalField = new Parameter(fieldVar.FieldType);
                //literalField.GetFieldType();
                //
                //varField = new Parameter(typeof(string));
                //varField.GetFieldType();

                //setMember = new Parameter(fieldVar.FieldType);
                paramList.Add(new Parameter(fieldVar.FieldType, "", this));

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
            //if (metaData.selectedType.DeclaringType.Assembly.Location != null)
            //    declaringTypeAsmPath = metaData.selectedType.DeclaringType.Assembly.Location;
            //
            //if (metaData.selectedType.DeclaringType.BaseType.Assembly.Location != null)
            //    declaringBaseTypeAsmPath = metaData.selectedType.DeclaringType.BaseType.Assembly.Location;
            //
            //if (metaData.selectedType.ReflectedType.Assembly.Location != null)
            //    reflectedTypeAsmPath = metaData.selectedType.ReflectedType.Assembly.Location;
            //
            //if (metaData.selectedType.ReflectedType.BaseType.Assembly.Location != null)
            //    reflectedTypeBaseAsmPath = metaData.selectedType.ReflectedType.BaseType.Assembly.Location;
            nameSpace = metaData.selectedType.Namespace;

            if (result == "Get")
            {
                nodeType = NodeType.Property_Get;
                propertyVar = metaData.properties[0];
                rect = new Rect(rect.x, rect.y, rect.width, rect.height * 2.0f);
                returnType = propertyVar.PropertyType;
                isDefined = true;
            }

            if (result == "Set")
            {
                nodeType = NodeType.Property_Set;

                propertyVar = metaData.properties[0];

                //literalField = new Parameter(propertyVar.PropertyType);
                //literalField.GetFieldType();
                //
                //varField = new Parameter(typeof(string));
                //varField.GetFieldType();
                paramList.Add(new Parameter(propertyVar.PropertyType, "", this));

                //Prepare rect for new appearance
                rect = new Rect(rect.x, rect.y, rect.width, rect.height * 2.0f);
                isDefined = true;
            }
        }
    }

    void SelectObjectType(object index)
    {
        metaData.selectedType = metaData.types[(int)index];        
        metaData.selectedAsm = metaData.selectedType.Assembly;
#if UNITY_EDITOR
        if (!Application.isPlaying)
            Interpreter.Instance.Compile(input, NodeEditor.current, ref metaData);
#endif
        if (Application.isPlaying)
            Interpreter.Instance.Compile(input, RealTimeEditor.Instance.current, ref metaData);
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
            nameSpace = info.ReflectedType.Namespace;
        }

        float initHeight = rect.height;

        ParameterInfo[] args = info.GetParameters();

        foreach (ParameterInfo p in args)
            paramList.Add(new Parameter(p.ParameterType, p.Name, this));

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
            //retType = ReturnVarType.Var;
            rect = new Rect(rect.x, rect.y, rect.width, initHeight * (paramList.Count + 1));
            //returnEntry = new Parameter(typeof(string));
        }

        else
            isReturning = false;


        isDefined = true;

    }

    //Old Version For Cost Functions No longer used
    public void ChangeToGameMethod(object method)
    {
        nodeType = NodeType.Function;        
        hasCost = true;
        isSpecial = false;

        MethodInfo info = (MethodInfo)method;

        if (info != null)
        {
            type = info.ReflectedType.ToString();
            assemblyPath = info.ReflectedType.Assembly.Location;
            nameSpace = info.ReflectedType.Namespace;
        }

        float initHeight = rect.height;

        ParameterInfo[] args = info.GetParameters();

        
        foreach (ParameterInfo p in args)
        {
            Parameter par = new Parameter(p.ParameterType, p.Name, this);

            //if (p.ParameterType == typeof(ActionStatus))
            //    par.shouldDraw = false;
           
            paramList.Add(par);
        }

        if (paramList.Count == 1)            
            rect = new Rect(rect.x, rect.y, rect.width, initHeight * (paramList.Count + 1));


        else if (paramList.Count > 1)
            rect = new Rect(rect.x, rect.y, rect.width, initHeight * paramList.Count);

        foreach (Parameter p in paramList)
        {
            p.GetFieldType();
        }

        returnType = info.ReturnType;
        currentMethod = info;

        if (metaData != null)
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

        if (returnType != typeof(void))
        {
            isReturning = true;
            //retType = ReturnVarType.Var;
            rect = new Rect(rect.x, rect.y, rect.width, initHeight * (paramList.Count + 1));
            //returnEntry = new Parameter(typeof(string));
        }

        else
            isReturning = false;


        isDefined = true;
    }

    public void ChangeToConditional()
    {
        nodeType = NodeType.Conditional;
        rect = new Rect(rect.x, rect.y, rect.width, rect.height * 2.0f);
        paramList.Add(new Parameter(typeof(string), "", this)); //should be a variable        
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
            isVirtual = info.IsVirtual;
            assemblyPath = metaData.selectedType.Assembly.Location;
            
            //if (metaData.selectedType.DeclaringType.Assembly.Location != null)
            //declaringTypeAsmPath = metaData.selectedType.DeclaringType.Assembly.Location;

            //if (metaData.selectedType.DeclaringType.BaseType.Assembly.Location != null)
            //declaringBaseTypeAsmPath = metaData.selectedType.DeclaringType.BaseType.Assembly.Location;

            //if (metaData.selectedType.ReflectedType.Assembly.Location != null)
            //reflectedTypeAsmPath = metaData.selectedType.ReflectedType.Assembly.Location;

            //if (metaData.selectedType.ReflectedType.BaseType.Assembly.Location != null)
            //reflectedTypeBaseAsmPath = metaData.selectedType.ReflectedType.BaseType.Assembly.Location;

            nameSpace = metaData.selectedType.Namespace;
        }

        else
        {
            type = info.ReflectedType.ToString();
            isStatic = info.IsStatic;
            isVirtual = info.IsVirtual;
            assemblyPath = info.ReflectedType.Assembly.Location;

            if (info.DeclaringType.Assembly.Location != null)
                declaringTypeAsmPath = info.DeclaringType.Assembly.Location;

            if (info.DeclaringType.BaseType.Assembly.Location != null)
                declaringBaseTypeAsmPath = info.DeclaringType.BaseType.Assembly.Location;

            if (info.ReflectedType.Assembly.Location != null)
                reflectedTypeAsmPath = info.ReflectedType.Assembly.Location;

            if (info.ReflectedType.BaseType.Assembly.Location != null)
                reflectedTypeBaseAsmPath = info.ReflectedType.BaseType.Assembly.Location;

            nameSpace = info.ReflectedType.Namespace;
        }
        
        float initHeight = rect.height;

        if (info.IsGenericMethod)
        {
            isGenericFunction = true;
            var genericArgs = info.GetGenericArguments();
            
            foreach(Type t in genericArgs)
            {
                Parameter p = new Parameter(t, t.Name, this, t.IsGenericParameter, true);                
                paramList.Add(p);
            }

        }

        ParameterInfo[] args = info.GetParameters();

        foreach (ParameterInfo p in args)
        {            
            paramList.Add(new Parameter(p.ParameterType, p.Name, this, p.ParameterType.IsGenericParameter));
        }
        
        if (paramList.Count == 1)                    
            rect = new Rect(rect.x, rect.y, rect.width, initHeight * (paramList.Count + 1));
        

        else if (paramList.Count > 1)
            rect = new Rect(rect.x, rect.y, rect.width, initHeight * (paramList.Count));
        
        //foreach(Parameter p in paramList)
        //{
        //    p.GetFieldType();
        //}

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
            if (paramList.Count > 1)
                rect = new Rect(rect.x, rect.y, rect.width, initHeight * (paramList.Count + 1));

            if (paramList.Count <= 1)
                rect = new Rect(rect.x, rect.y, rect.width, initHeight * (paramList.Count + 2));
            //returnEntry = new Parameter(typeof(string));
            //retType = ReturnVarType.Var;
        }

        else        
            isReturning = false;
        

        isDefined = true;
    }

    //Accounts for Generics
    public void ChangeToConstructor(object method)
    {
        nodeType = NodeType.Constructor;

        ConstructorInfo info = (ConstructorInfo)method;

        if (metaData != null && !isSpecial)
        {
            //targetVar = metaData.varRef;
            varName = metaData.varName;
            input = metaData.input;
            type = metaData.selectedType.ToString();
            isStatic = info.IsStatic;
            assemblyPath = metaData.selectedType.Assembly.Location;
            //if (metaData.selectedType.DeclaringType.Assembly.Location != null)
            //    declaringTypeAsmPath = metaData.selectedType.DeclaringType.Assembly.Location;
            //
            //if (metaData.selectedType.DeclaringType.BaseType.Assembly.Location != null)
            //    declaringBaseTypeAsmPath = metaData.selectedType.DeclaringType.BaseType.Assembly.Location;
            //
            //if (metaData.selectedType.ReflectedType.Assembly.Location != null)
            //    reflectedTypeAsmPath = metaData.selectedType.ReflectedType.Assembly.Location;
            //
            //if (metaData.selectedType.ReflectedType.BaseType.Assembly.Location != null)
            //    reflectedTypeBaseAsmPath = metaData.selectedType.ReflectedType.BaseType.Assembly.Location;
            nameSpace = metaData.selectedType.Namespace;
        }

        else
        {
            type = info.ReflectedType.ToString();
            isStatic = info.IsStatic;
            assemblyPath = info.ReflectedType.Assembly.Location;

            //if (info.DeclaringType.Assembly.Location != null)
            //    declaringTypeAsmPath = info.DeclaringType.Assembly.Location;
            //if (info.DeclaringType.BaseType.Assembly.Location != null)
            //    declaringBaseTypeAsmPath = info.DeclaringType.BaseType.Assembly.Location;
            //
            //if (info.ReflectedType.Assembly.Location != null)
            //    reflectedTypeAsmPath = info.ReflectedType.Assembly.Location;
            //
            //if (info.ReflectedType.BaseType.Assembly.Location != null)
            //    reflectedTypeBaseAsmPath = info.ReflectedType.BaseType.Assembly.Location;

            nameSpace = info.ReflectedType.Namespace;
        }

        float initHeight = rect.height;

        if (info.IsGenericMethod)
        {
            isGenericFunction = true;
            var genericArgs = info.GetGenericArguments();

            foreach (Type t in genericArgs)
            {
                Parameter p = new Parameter(t, t.Name, this, t.IsGenericParameter, true);
                paramList.Add(p);
            }

        }

        ParameterInfo[] args = info.GetParameters();

        foreach (ParameterInfo p in args)
            paramList.Add(new Parameter(p.ParameterType, p.Name, this));

        if (paramList.Count == 1)
            rect = new Rect(rect.x, rect.y, rect.width, initHeight * (paramList.Count + 1));


        else if (paramList.Count > 1)
            rect = new Rect(rect.x, rect.y, rect.width, initHeight * (paramList.Count));

        //foreach (Parameter p in paramList)
        //{
        //    p.GetFieldType();
        //}

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
            //returnEntry = new Parameter(typeof(string));
            //retType = ReturnVarType.Var;
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



