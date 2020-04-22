using System.Collections;
using System.Collections.Generic;
using UnityEngine;
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
    public ConnectionPointData falsePoint;

    public NodeType nodeType;

    //REFLECTION SPECIFIC
    public List<ParameterData> paramList;
    public string input; //the raw input - can probably be used to activate the reflection
    public string type; //The stored type from original reflection    
    public string assemblyPath; //The path for the assembly
    public int index; //The index of the list found for function overloads
    public UnityEngine.Object target;

    //Function Specific
    public string returnType; //If a function that returns notify
    public string returnInput; //Tells where to return to
    public string returnAsmPath;
    public bool isReturning;
    public bool isSpecial;
    public string returnVarName;
    public Node.ReturnVarType retType;

    public bool isStatic;
    public bool isContextual;
    public int ID; //The way to find the correct node to reference at runtime because serialization
    public int nextID;
    public int prevID;
    public int falseID;

    public ParameterData literalField; //For Setting applicable serializable types to a literal
    public ParameterData varField; //For referencing variables in the blueprint
    public Var varRef;
    public string varName;
    public bool isVar;

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
        nodeType = node.nodeType;


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
        //target = node.target;
        isVar = node.isVar;

        isReturning = node.isReturning;
        isSpecial = node.isSpecial;

        if (node.returnType != null)
            returnType = node.returnType.ToString();

        returnInput = node.returnInput;        
        //returnAsmPath = node.returnAsmPath;
        //returnVarName = node.returnVarName;
        retType = node.retType;

        if (node.literalField != null)
            literalField = new ParameterData(node.literalField);

        if (node.varField != null)
            varField = new ParameterData(node.varField);

        //varRef = node.targetVar;
        varName = node.varName;
        
        if (paramList == null)
            paramList = new List<ParameterData>();

        if (node.paramList != null)
        {
            foreach (Parameter par in node.paramList)
            {
                paramList.Add(new ParameterData(par));
            }
        }

        //IDS
        ID = node.ID;
        nextID = node.nextID;
        prevID = node.prevID;
        falseID = node.falseID;
        isContextual = node.isContextual;
        isStatic = node.isStatic;

        inPoint = new ConnectionPointData(node.inPoint, this);
        outPoint = new ConnectionPointData(node.outPoint, this);
        falsePoint = new ConnectionPointData(node.falsePoint, this);
    }

}