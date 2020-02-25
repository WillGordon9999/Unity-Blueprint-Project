﻿using System.Collections;
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
    Vector2 initPos;
    Vector2 initDimensions;
    public string title;
    public bool isDragged;
    public bool isSelected;
    bool isDefined;

    public GUIStyle style;
    public GUIStyle defaultNodeStyle;
    public GUIStyle selectedNodeStyle;

    public ConnectionPoint inPoint;
    public ConnectionPoint outPoint;

    public string input;    

    public Action<Node> OnRemoveNode;

    MethodInfo[] methodDefinitions;
    
    List<Parameter> paramList;

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
                paramList[i].draw();                          
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
                    Interpreter.Instance.ParseKeywords(input, this);
                    methodDefinitions = Interpreter.Instance.GetFunctionDefinitions(input);
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

        isDefined = true;
    }
}

public class Parameter
{
    public string name;
    public object arg;
    public Type type;
    public Rect rect;
    public Action draw;

    public Parameter() { }


    public Parameter(Type objType)
    {
        type = objType;
    }

    public Parameter(object obj, Type objType, string label)
    {
        arg = obj;
        type = objType;
        name = label;
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
                return;
            }

            if (type == typeof(int))
            {
                draw = delegate { arg = EditorGUI.IntField(rect, GetType<int>()); };
                return;
            }

            if (type == typeof(long))
            {
                draw = delegate { arg = EditorGUI.LongField(rect, GetType<long>()); };
                return;
            }

            if (type == typeof(float))
            {
                draw = delegate { arg = EditorGUI.FloatField(rect, GetType<float>()); };
                return;
            }

            if (type == typeof(double))
            {
                draw = delegate { arg = EditorGUI.DoubleField(rect, GetType<double>()); };
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
                return;
            }

            if (type == typeof(Vector2))
            {
                //draw = delegate { arg = EditorGUI.Vector2Field(rect, name, GetType<Vector2>()); };                    
                arg = new float[] { 0.0f, 0.0f };
                draw = delegate { EditorGUI.MultiFloatField(rect, new GUIContent[] { new GUIContent("X"), new GUIContent("Y") }, (float[])arg); };
                return;
            }

            if (type == typeof(Vector3))
            {
                //draw = delegate { arg = EditorGUI.Vector3Field(rect, name, GetType<Vector3>()); }; 
                arg = new float[] { 0.0f, 0.0f, 0.0f };
                draw = delegate { EditorGUI.MultiFloatField(rect, new GUIContent[] { new GUIContent("X"), new GUIContent("Y"), new GUIContent("Z") }, (float[])arg); };
                return;
            }

            if (type == typeof(Vector4))
            {
                //draw = delegate { arg = EditorGUI.Vector4Field(rect, name, GetType<Vector4>()); };                                        
                arg = new float[] { 0.0f, 0.0f, 0.0f, 0.0f };
                draw = delegate { EditorGUI.MultiFloatField(rect, new GUIContent[] { new GUIContent("X"), new GUIContent("Y"), new GUIContent("Z"), new GUIContent("W") }, (float[])arg); };
                return;
            }

            if (type == typeof(Rect))
            {
                //draw = delegate { arg = EditorGUI.RectField(rect, GetType<Rect>()); };
                arg = new float[] { 0.0f, 0.0f, 0.0f, 0.0f };
                draw = delegate { EditorGUI.MultiFloatField(rect, new GUIContent[] { new GUIContent("X"), new GUIContent("Y"), new GUIContent("W"), new GUIContent("H") }, (float[])arg); };
                return;
            }

            if (type == typeof(Color))
            {
                draw = delegate { arg = EditorGUI.ColorField(rect, name, GetType<Color>()); };
                return;
            }

            //Found Here: https://stackoverflow.com/questions/15855881/create-instance-of-unknown-enum-with-string-value-using-reflection-in-c-sharp
            if (type.BaseType == typeof(System.Enum))
            {
                if (arg == null)
                    arg = Enum.ToObject(type, 0);

                draw = delegate { arg = EditorGUI.EnumPopup(rect, (System.Enum)System.Convert.ChangeType(arg, type)); };

                return;
            }

            if (type == typeof(UnityEngine.Object))
            {
                draw = delegate { arg = EditorGUI.ObjectField(rect, GetType<UnityEngine.Object>(), type, true); };
                return;
            }

            Debug.Log("Type not found");

        }
    }

}
