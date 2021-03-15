using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;
using System.Reflection;

public class Parameter
{
    public string name { get; set; }
    public object arg;
    
    float[] numFields; //For managing vectors, I hope this approach will allow for value editing during play mode
    public Type type;
    public Rect rect;
    public Action<float> draw;

    public string varInput;
    Action<float> varDraw;
    Action<float> literalDraw;

    public ParameterData.ParamType paramType;
    public UnityEngine.Object obj;
    public string objType;
    public bool noType;
    public bool inputVar; //determine whether to take in a literal or a variable
    public bool shouldDraw = true;

    public bool isGeneric = false;
    public bool isGenericDef = false; //If the thing is only in the < > so SomeFunc<x>(y), this is x if this is true
    public string templateType;
    public string templateTypeAsmPath;
    
    public Node nodeRef; //Reference to node

    public Parameter() { }


    public Parameter(Type objType, string parName = "", Node Ref = null, bool generic = false, bool isDef = false)
    {
        type = objType;
        name = parName;
        nodeRef = Ref;
        isGeneric = generic;
        isGenericDef = isDef;

        if (isGeneric)
        {
            inputVar = true;
            noType = true;

            varDraw = delegate (float width)
            {
                GUILayout.BeginHorizontal(GUILayout.MaxWidth(width));

                GUILayout.Label(name);
                
                templateType = GUILayout.TextField(templateType);

                if (!isGenericDef)
                    varInput = GUILayout.TextField(varInput);
                
                GUILayout.EndHorizontal();
            };

            draw = varDraw;
        }

        else
        {
            //varDraw = delegate { varInput = EditorGUI.TextField(rect, varInput); };
            varDraw = delegate (float width)
            {
                GUILayout.BeginHorizontal(GUILayout.MaxWidth(width));

                GUILayout.Label(name);

                varInput = GUILayout.TextField(varInput);

                GUILayout.EndHorizontal();
            };

            GetFieldType();
            literalDraw = draw;
        }
       
    }

    //public Parameter(object obj, Type objType, ParameterData.ParamType parType, bool inVar, string varIn, bool toDraw = true, string label = "", Node Ref = null, bool generic = false, string tempType = "")
    public Parameter(ParameterData data, Node Ref)
    {
        arg = data.GetValue();
        type = data.GetSystemType();
        paramType = data.type;
        name = data.name;
        inputVar = data.inputVar;
        varInput = data.varInput;
        nodeRef = Ref;
        isGeneric = data.isGeneric;
        isGenericDef = data.isGenericDef;
        templateType = data.templateType;
        templateTypeAsmPath = data.templateTypeAsmPath;
        //prevTemplateInput = templateType;

        if (isGeneric)
        {
            inputVar = true;

            varDraw = delegate (float width)
            {
                GUILayout.BeginHorizontal(GUILayout.MaxWidth(width));

                GUILayout.Label(name);
                templateType = GUILayout.TextField(templateType);

                if (!isGenericDef)
                    varInput = GUILayout.TextField(varInput);

                GUILayout.EndHorizontal();
            };
            literalDraw = draw;
        }

        else
        {
            varDraw = delegate (float width)
            {
                GUILayout.BeginHorizontal(GUILayout.MaxWidth(width));

                GUILayout.Label(name);
                varInput = GUILayout.TextField(varInput);

                GUILayout.EndHorizontal();
            };
            GetFieldType();
        }

        literalDraw = draw;
        shouldDraw = data.shouldDraw;

        if (inputVar)
            draw = varDraw;
        else
            draw = literalDraw;

    }

    public void ToggleType()
    {
        if (isGeneric)
            return;

        inputVar = !inputVar;

        if (inputVar)        
            draw = varDraw;
        
        else
            draw = literalDraw;

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
#if UNITY_EDITOR
                //if (!Application.isPlaying)
                //     draw = delegate { arg = EditorGUI.Toggle(rect, GetType<bool>()); };
#endif          //
                //
                //if (Application.isPlaying) 
                //     draw = delegate { arg = RealTimeEditor.Toggle(rect, GetType<bool>()); };

                draw = (width) => arg = GUILayout.Toggle(GetType<bool>(), name, GUILayout.MaxWidth(width));

                paramType = ParameterData.ParamType.Bool;
                return;
            }

            if (type == typeof(int))
            {
#if UNITY_EDITOR
                //if (!Application.isPlaying)
                //    draw = delegate { arg = EditorGUI.IntField(rect, GetType<int>()); };
#endif          //
                //
                //if (Application.isPlaying)
                //    draw = delegate { arg = RealTimeEditor.IntField(rect, GetType<int>()); };

                //draw = (width) => arg = RealTimeEditor.IntField()
                draw = delegate (float width)
                {
                    GUILayout.BeginHorizontal(GUILayout.MaxWidth(width));

                    GUILayout.Label(name);
                    arg = RealTimeEditor.IntField(Rect.zero, GetType<int>());

                    GUILayout.EndHorizontal();
                };

                paramType = ParameterData.ParamType.Int;
                return;

                //draw = delegate { arg = EditorGUI.IntField(rect, GetType<int>()); };
                //paramType = ParameterData.ParamType.Int;
                //return;
            }

            if (type == typeof(long))
            {
#if UNITY_EDITOR
                //if (!Application.isPlaying)
                //    draw = delegate { arg = EditorGUI.LongField(rect, GetType<long>()); };
#endif          //
                //
                //if (Application.isPlaying)
                //    draw = delegate { arg = RealTimeEditor.LongField(rect, GetType<long>()); };
                draw = delegate (float width)
                {
                    GUILayout.BeginHorizontal(GUILayout.MaxWidth(width));

                    GUILayout.Label(name);
                    arg = RealTimeEditor.LongField(Rect.zero, GetType<long>());

                    GUILayout.EndHorizontal();
                };


                paramType = ParameterData.ParamType.Long;
                return;
                //draw = delegate { arg = EditorGUI.LongField(rect, GetType<long>()); };
                //paramType = ParameterData.ParamType.Long;
                //return;
            }

            if (type == typeof(float))
            {
#if UNITY_EDITOR
                //if (!Application.isPlaying)
                //    draw = delegate { arg = EditorGUI.FloatField(rect, GetType<float>()); };
#endif          //
                //
                //if (Application.isPlaying)
                //    draw = delegate { arg = RealTimeEditor.FloatField(rect, GetType<float>()); };

                draw = delegate (float width)
                {
                    GUILayout.BeginHorizontal(GUILayout.MaxWidth(width));

                    GUILayout.Label(name);
                    arg = RealTimeEditor.FloatField(Rect.zero, GetType<float>());

                    GUILayout.EndHorizontal();
                };

                paramType = ParameterData.ParamType.Float;
                return;
                //draw = delegate { arg = EditorGUI.FloatField(rect, GetType<float>()); };
                //paramType = ParameterData.ParamType.Float;
                //return;
            }

            if (type == typeof(double))
            {
#if UNITY_EDITOR
                //if (!Application.isPlaying)
                //    draw = delegate { arg = EditorGUI.DoubleField(rect, GetType<double>()); };
#endif          //
                //
                //if (Application.isPlaying)
                //    draw = delegate { arg = RealTimeEditor.DoubleField(rect, GetType<double>()); };

                draw = delegate (float width)
                {
                    GUILayout.BeginHorizontal(GUILayout.MaxWidth(width));

                    GUILayout.Label(name);
                    arg = RealTimeEditor.DoubleField(Rect.zero, GetType<double>());

                    GUILayout.EndHorizontal();
                };

                paramType = ParameterData.ParamType.Double;
                return;
                //draw = delegate { arg = EditorGUI.DoubleField(rect, GetType<double>()); };
                //paramType = ParameterData.ParamType.Double;
                //return;
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
#if UNITY_EDITOR
                //if (!Application.isPlaying)
                //    //draw = delegate { arg = EditorGUI.TextField(rect, (string)System.Convert.ChangeType(arg, typeof(string))); };                
                //    draw = delegate { arg = EditorGUILayout.TextField((string)System.Convert.ChangeType(arg, typeof(string))); };
#endif

                //if (Application.isPlaying)
                //draw = delegate { arg = GUILayout.TextField((string)System.Convert.ChangeType(arg, typeof(string))); };
                //draw = (width) => arg = GUILayout.TextField((string)System.Convert.ChangeType(arg, typeof(string)), GUILayout.MaxWidth(width));

                draw = delegate (float width)
                {
                    GUILayout.BeginHorizontal(GUILayout.MaxWidth(width));

                    GUILayout.Label(name);                    
                    arg = GUILayout.TextField((string)System.Convert.ChangeType(arg, typeof(string)));

                    GUILayout.EndHorizontal();
                };
                //draw = delegate (float width) { rect.width += width; };

                paramType = ParameterData.ParamType.String;
                return;

                //draw = delegate { arg = EditorGUI.TextField(rect, (string)System.Convert.ChangeType(arg, typeof(string))); };
                //paramType = ParameterData.ParamType.String;
                //return;
            }

            if (type == typeof(Vector2))
            {
                //draw = delegate { arg = EditorGUI.Vector2Field(rect, name, GetType<Vector2>()); };                    
                //I'm hoping this approach might allow for live value editing during play later
                if (arg != null)
                {
                    Vector2 v = (Vector2)arg;
                    numFields = new float[] { v.x, v.y };
                }
                else
                {
                    //arg = new float[] { 0.0f, 0.0f };
                    numFields = new float[] { 0.0f, 0.0f };
                    arg = new Vector2(0.0f, 0.0f);
                }

#if UNITY_EDITOR
                //if (!Application.isPlaying)
                //{
                //    draw = delegate
                //    {
                //        EditorGUI.MultiFloatField(rect, new GUIContent[] { new GUIContent("X"), new GUIContent("Y") }, numFields);
                //        arg = new Vector2(numFields[0], numFields[1]);
                //    };
                //}
#endif          //
                //if (Application.isPlaying)
                //{
                //    draw = delegate { arg = RealTimeEditor.Vector2Field(rect, GetType<Vector2>()); };
                //}

                draw = delegate (float width)
                {
                    GUILayout.BeginHorizontal(GUILayout.MaxWidth(width));

                    GUILayout.Label(name);
                    arg = RealTimeEditor.Vector2Field(Rect.zero, GetType<Vector2>());

                    GUILayout.EndHorizontal();
                };

                paramType = ParameterData.ParamType.Vec2;
                return;
            }

            if (type == typeof(Vector3))
            {
                //draw = delegate { arg = EditorGUI.Vector3Field(rect, name, GetType<Vector3>()); }; 

                if (arg != null)
                {
                    Vector3 v = (Vector3)arg;
                    numFields = new float[] { v.x, v.y, v.z };
                }

                else
                {
                    //arg = new float[] { 0.0f, 0.0f, 0.0f };
                    numFields = new float[] { 0.0f, 0.0f, 0.0f };
                    arg = new Vector3(0.0f, 0.0f, 0.0f);
                }
#if UNITY_EDITOR
                //if (!Application.isPlaying)
                //{
                //    draw = delegate
                //    {
                //        EditorGUI.MultiFloatField(rect, new GUIContent[] { new GUIContent("X"), new GUIContent("Y"), new GUIContent("Z") }, numFields);
                //        arg = new Vector3(numFields[0], numFields[1], numFields[2]);
                //    };
                //}
#endif          //
                //if (Application.isPlaying)
                //    draw = delegate { arg = RealTimeEditor.Vector3Field(rect, GetType<Vector3>()); };
                draw = delegate (float width)
                {
                    GUILayout.BeginHorizontal(GUILayout.MaxWidth(width));

                    GUILayout.Label(name);
                    arg = RealTimeEditor.Vector3Field(Rect.zero, GetType<Vector3>());

                    GUILayout.EndHorizontal();
                };

                paramType = ParameterData.ParamType.Vec3;
                return;
            }

            if (type == typeof(Vector4))
            {
                //draw = delegate { arg = EditorGUI.Vector4Field(rect, name, GetType<Vector4>()); };

                if (arg != null)
                {
                    Vector4 v = (Vector4)arg;
                    numFields = new float[] { v.x, v.y, v.z, v.w };
                }
                else
                {
                    //arg = new float[] { 0.0f, 0.0f, 0.0f, 0.0f };
                    numFields = new float[] { 0.0f, 0.0f, 0.0f, 0.0f };
                    arg = new Vector4(0.0f, 0.0f, 0.0f, 0.0f);
                }
#if UNITY_EDITOR
                //if (!Application.isPlaying)
                //{
                //    draw = delegate
                //    {
                //        EditorGUI.MultiFloatField(rect, new GUIContent[] { new GUIContent("X"), new GUIContent("Y"), new GUIContent("Z"), new GUIContent("W") }, numFields);
                //        arg = new Vector4(numFields[0], numFields[1], numFields[2], numFields[3]);
                //    };
                //}
#endif

                //if (Application.isPlaying)
                //    draw = delegate { arg = RealTimeEditor.Vector4Field(rect, GetType<Vector4>()); };

                draw = delegate (float width)
                {
                    GUILayout.BeginHorizontal(GUILayout.MaxWidth(width));

                    GUILayout.Label(name);
                    arg = RealTimeEditor.Vector4Field(Rect.zero, GetType<Vector4>());

                    GUILayout.EndHorizontal();
                };

                paramType = ParameterData.ParamType.Vec4;
                return;
            }

            if (type == typeof(Quaternion))
            {
                draw = delegate (float width)
                {
                    GUILayout.BeginHorizontal(GUILayout.MaxWidth(width));

                    GUILayout.Label(name);
                    arg = RealTimeEditor.QuaternionField(Rect.zero, GetType<Quaternion>());

                    GUILayout.EndHorizontal();
                };

                paramType = ParameterData.ParamType.Quaternion;
                return;
            }

            if (type == typeof(Rect))
            {
                //draw = delegate { arg = EditorGUI.RectField(rect, GetType<Rect>()); };

                if (arg != null)
                {
                    Rect r = (Rect)arg;
                    numFields = new float[] { r.x, r.y, r.width, r.height };
                }
                else
                {
                    //arg = new float[] { 0.0f, 0.0f, 0.0f, 0.0f };                        
                    numFields = new float[] { 0.0f, 0.0f, 0.0f, 0.0f };
                    arg = new Rect(0.0f, 0.0f, 0.0f, 0.0f);
                }
#if UNITY_EDITOR
                //if (!Application.isPlaying)
                //{
                //    draw = delegate
                //    {
                //        EditorGUI.MultiFloatField(rect, new GUIContent[] { new GUIContent("X"), new GUIContent("Y"), new GUIContent("W"), new GUIContent("H") }, numFields);
                //        arg = new Rect(numFields[0], numFields[1], numFields[2], numFields[3]);
                //    };
                //}
#endif          //
                //if (Application.isPlaying)
                //{
                //    draw = delegate { RealTimeEditor.RectField(rect, GetType<Rect>()); };
                //}

                draw = delegate (float width)
                {
                    GUILayout.BeginHorizontal(GUILayout.MaxWidth(width));

                    GUILayout.Label(name);
                    arg = RealTimeEditor.RectField(Rect.zero, GetType<Rect>());

                    GUILayout.EndHorizontal();
                };

                paramType = ParameterData.ParamType.Rect;
                return;
            }

            if (type == typeof(Color))
            {
#if UNITY_EDITOR
                //if (!Application.isPlaying)
                //{
                //    draw = delegate { arg = EditorGUI.ColorField(rect, name, GetType<Color>()); };
                //}
#endif          //
                //if (Application.isPlaying)
                //    draw = delegate { arg = RealTimeEditor.ColorField(rect, GetType<Color>()); };

                draw = delegate (float width)
                {
                    GUILayout.BeginHorizontal(GUILayout.MaxWidth(width));

                    GUILayout.Label(name);
                    arg = RealTimeEditor.ColorField(Rect.zero, GetType<Color>());

                    GUILayout.EndHorizontal();
                };


                paramType = ParameterData.ParamType.Color;
                return;
            }

            //Found Here: https://stackoverflow.com/questions/15855881/create-instance-of-unknown-enum-with-string-value-using-reflection-in-c-sharp
            if (type.BaseType == typeof(System.Enum))
            {
                if (arg == null)
                    arg = Enum.ToObject(type, 0);
                else
                    arg = Enum.ToObject(type, arg);
                //draw = delegate { arg = EditorGUI.EnumPopup(rect, (System.Enum)System.Convert.ChangeType(arg, type)); };
#if UNITY_EDITOR
                //if (!Application.isPlaying)
                //{
                //    draw = delegate { arg = EditorGUI.EnumPopup(rect, (System.Enum)arg); };
                //}
#endif          //
                //if (Application.isPlaying)
                //    draw = delegate { arg = RealTimeEditor.EnumPopup(rect, (System.Enum)arg); };

                draw = delegate (float width)
                {
                    //GUILayout.BeginHorizontal(GUILayout.MaxWidth(width));

                    GUILayout.Label(name);
                    //GUILayout.FlexibleSpace();
                    //arg = RealTimeEditor.EnumPopup(nodeRef.final, (System.Enum)arg, width);
                    RealTimeEditor.EnumPopup(nodeRef.final, this, width);
                    //arg = RealTimeEditor.EnumPopup(rect, (System.Enum)arg, width);

                    //GUILayout.EndHorizontal();
                };

                paramType = ParameterData.ParamType.Enum;

                return;
            }

            if (type == typeof(UnityEngine.Object))
            {
#if UNITY_EDITOR
                if (!Application.isPlaying)
                {
                    draw = delegate
                    {
                        obj = EditorGUI.ObjectField(rect, GetType<UnityEngine.Object>(), type, true);
                        arg = obj;
                        objType = obj.GetType().ToString();
                    };
                    paramType = ParameterData.ParamType.Object;
                    return;
                }
#endif                                
            }

            //if (type == typeof(Var))
            //{
            //    draw = delegate { arg = EditorGUI.TextField(rect, (string)System.Convert.ChangeType(arg, typeof(string))); };
            //    paramType = ParameterData.ParamType.String;
            //    return;
            //}
            
            //Debug.Log("Type not found");
            noType = true;

            
            //Debug.Log("Defaulting to Text field");

            //#if UNITY_EDITOR
            //            if (!Application.isPlaying)
            //                draw = delegate { arg = EditorGUI.TextField(rect, (string)System.Convert.ChangeType(arg, typeof(string))); };
            //#endif
            //
            //            if (Application.isPlaying)
            //                draw = delegate { arg = GUILayout.TextField((string)System.Convert.ChangeType(arg, typeof(string))); };

            draw = delegate (float width)
            {
                GUILayout.BeginHorizontal(GUILayout.MaxWidth(width));

                GUILayout.Label(name);                
                arg = GUILayout.TextField((string)System.Convert.ChangeType(arg, typeof(string)));

                GUILayout.EndHorizontal();
            };

            paramType = ParameterData.ParamType.Object;
            return;
        }
    }

}