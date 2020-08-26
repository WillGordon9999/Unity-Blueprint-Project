using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Reflection;

[Serializable]
public class ParameterData
{
    //I wonder if I can make a type as a string along with the assembly path, and just a byte array to store any variable
    public string name;
    public Rect rect; //display rect  
    
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
    public string objType; //More specific object type for proper casting
    public bool inputVar; //Is this a variable access
    public string varInput; //name of variable to access

    public string strType;
    public string asmPath;

    public enum ParamType { Bool, Int, Enum, Float, Char, Long, Double, String, Rect, Color, Vec2, Vec3, Vec4, Object }

    public ParamType type;
    
    //This constructor here poses issues for the separation, recommend making a 'constructor' of ParameterData in Parameter
    public ParameterData(Parameter par)
    {
        type = par.paramType;
        inputVar = par.inputVar;
        varInput = par.varInput;
        name = par.name;

        strType = par.type.ToString();
        asmPath = par.type.Assembly.Location;
                                
        if (par.noType)
        {
            return;
        }

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
                    vec2Val = (Vector2)par.arg;
                    break;
                }
            case ParamType.Vec3:
                {                    
                    vec3Val = (Vector3)par.arg;
                    break;
                }
            case ParamType.Vec4:
                {                    
                    vec4Val = (Vector4)par.arg;
                    break;
                }
            case ParamType.Object:
                {
                    //obj = (UnityEngine.Object)par.arg;                    
                    obj = par.obj;
                    objType = par.objType;
                    break;
                }
        }        
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
                //return typeof(System.Enum);
                return Interpreter.Instance.LoadVarType(strType, asmPath);

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