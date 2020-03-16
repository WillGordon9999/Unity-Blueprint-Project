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

        foreach (Parameter par in node.paramList)
        {
            paramList.Add(new ParameterData(par));
        }

        ID = node.ID;
        nextID = node.nextID;

        inPoint = new ConnectionPointData(node.inPoint, this);
        outPoint = new ConnectionPointData(node.outPoint, this);
    }

}