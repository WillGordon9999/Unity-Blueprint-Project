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
    public string title;
    public bool isDragged;
    public bool isSelected;

    public GUIStyle style;
    public GUIStyle defaultNodeStyle;
    public GUIStyle selectedNodeStyle;

    public ConnectionPoint inPoint;
    public ConnectionPoint outPoint;

    public string input;
    string prevInput;

    public Action<Node> OnRemoveNode;

    MethodInfo[] methodDefinitions;

    public float testProperty;
    public GameObject testObj;

    ObjectField[] fields;
    ObjectField test;

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
        prevInput = "";
        test = new ObjectField();
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
        float width = rect.width * 0.75f;
        float height = rect.height * 0.5f;
        float x = Mathf.Lerp(rect.position.x, rect.position.x + rect.width, 0.125f);
        float y = Mathf.Lerp(rect.position.y, rect.position.y + rect.height, 0.2f);
                
        input = EditorGUI.TextField(new Rect(x, y, width, height), input);
        //testProperty = EditorGUI.ObjectField(new Rect(x, y + height, width, height), "Test", testProperty, testProperty.GetType());
        //testObj = (GameObject)EditorGUI.ObjectField(new Rect(x, y + height, width, height), testObj, testObj.GetType(), true);
        testProperty = EditorGUI.FloatField(new Rect(x, y + height, width * 2, height), "Test", testProperty);        
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

                //if (e.button == 1 && isSelected)
                if (e.button == 1 && rect.Contains(e.mousePosition))
                {
                    Debug.Log("In right click");
                    e.Use();
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
                if (isSelected)
                {
                    if (e.keyCode == KeyCode.Return)
                    {
                        Debug.Log("In return on node");
                        if (input != prevInput)
                        {
                            e.Use();
                            Debug.Log("Trying to call reflection");
                            //Do a reflection call here
                            methodDefinitions = Interpreter.Instance.GetFunctionDefinitions(input);
                        }
                    }
                }
                break;

            //case EventType.MouseUp:
            //    if (e.button == 0 && isDragged)
            //    {
            //        Drag(e.delta);
            //        e.Use();
            //        return true;
            //    }
            //    break;
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

        else
            Debug.Log("Method Definitions is null");

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

        fields = new ObjectField[args.Length];
    }
}
