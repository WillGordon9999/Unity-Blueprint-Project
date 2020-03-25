using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.UIElements;
using System;
using System.Reflection;

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
        switch (type)
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
                    //float[] val = (float[])par.arg;
                    //rectVal = new Rect(val[0], val[1], val[2], val[3]);
                    rectVal = (Rect)par.arg;
                    break;
                }
            case ParamType.Color:
                {
                    colVal = (Color)par.arg;
                    break;
                }
            case ParamType.Vec2:
                {
                    //float[] val = (float[])par.arg;
                    //vec2Val = new Vector2(val[0], val[1]);
                    vec2Val = (Vector2)par.arg;
                    break;
                }
            case ParamType.Vec3:
                {
                    //float[] val = (float[])par.arg;                    
                    //vec3Val = new Vector3(val[0], val[1], val[2]);
                    vec3Val = (Vector3)par.arg;
                    break;
                }
            case ParamType.Vec4:
                {
                    //float[] val = (float[])par.arg;
                    //vec4Val = new Vector4(val[0], val[1], val[2], val[3]);
                    vec4Val = (Vector4)par.arg;
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
        switch (type)
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