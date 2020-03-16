﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.UIElements;
using System;
using System.Reflection;

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