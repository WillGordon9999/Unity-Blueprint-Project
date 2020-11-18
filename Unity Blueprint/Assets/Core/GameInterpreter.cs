using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;
using System.Reflection;

public class GameInterpreter : MonoBehaviour
{
    private static GameInterpreter mInstance;
    public static GameInterpreter Instance;
    public GameCodebase codebase;
    public bool UseGameCompile;
    public string input;

    private void OnEnable()
    {
        if (mInstance == null)
            mInstance = this;

        Interpreter.Instance.UseGameCompile = UseGameCompile;
    }


    // Start is called before the first frame update
    void Start()
    {
       
    }

    private void OnDestroy()
    {
        //print("In OnDestroy");
        //EditorUtility.SetDirty(codebase);
        //AssetDatabase.Refresh();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnGUI()
    {
        //input = GUI.TextField(new Rect(Screen.width / 2, Screen.height / 2, 300, 100), input);
    }

    void AddSubclassesOf(Type type)
    {
        if (codebase)
        {
            List<Type> compTypes = new List<Type>();
        
            if (codebase.classes == null)
                codebase.classes = new List<GameClass>();
        
            foreach(Assembly asm in AppDomain.CurrentDomain.GetAssemblies())
            {
                Type[] types = asm.GetTypes();
        
                foreach(Type t in types)
                {
                    if (t.IsSubclassOf(type) && t.Namespace != "UnityEditor")
                    {
                        compTypes.Add(t);
                    }
                }
            }
        
            foreach(Type t in compTypes)
            {
                codebase.classes.Add(new GameClass(t.Name, ClassAccess.All));
            }

#if UNITY_EDITOR
            EditorUtility.SetDirty(codebase);
            AssetDatabase.Refresh();
#endif
        }
    }

    void AddClassToCodeBase()
    {
        if (codebase.classes == null)
            codebase.classes = new List<GameClass>();

        Type[] types;
        Assembly[] asms;

        if (Interpreter.Instance.FindType(input, out types, out asms))
        {
            foreach (Type t in types)
            {
                GameClass gameClass = new GameClass(t.Name, ClassAccess.All);

                List<MemberData> members = new List<MemberData>();

                ConstructorInfo[] constructors = t.GetConstructors();

                foreach (ConstructorInfo c in constructors)
                {
                    ParameterInfo[] pars = c.GetParameters();

                    string name = c.Name + "(";

                    for (int i = 0; i < pars.Length; i++)
                    {
                        name += " " + pars[i].ParameterType.Name + " " + pars[i].Name;

                        if (i < pars.Length - 1)
                            name += ", ";
                    }

                    name += " )";

                    int meta = c.MetadataToken;

                    members.Add(new MemberData(name, MemberData.MemberType.Constructor, MemberData.AccessType.Both, meta));
                }

                MethodInfo[] methods = t.GetMethods();

                foreach (MethodInfo m in methods)
                {
                    ParameterInfo[] pars = m.GetParameters();

                    string name = m.Name + "(";

                    for (int i = 0; i < pars.Length; i++)
                    {
                        name += " " + pars[i].ParameterType.Name + " " + pars[i].Name;

                        if (i < pars.Length - 1)
                            name += ", ";
                    }

                    name += " )";

                    int meta = m.MetadataToken;

                    members.Add(new MemberData(name, MemberData.MemberType.Function, MemberData.AccessType.Both, meta));
                }

                FieldInfo[] fields = t.GetFields();

                foreach (FieldInfo f in fields)
                {
                    string name = f.FieldType + " " + f.Name;
                    int meta = f.MetadataToken;

                    members.Add(new MemberData(name, MemberData.MemberType.Field, MemberData.AccessType.Both, meta));
                }

                PropertyInfo[] props = t.GetProperties();

                foreach (PropertyInfo p in props)
                {
                    string name = p.PropertyType + " " + p.Name;

                    MemberData.AccessType access = MemberData.AccessType.Both;

                    if (p.CanRead && p.CanWrite)
                        access = MemberData.AccessType.Both;

                    else if (p.CanRead)
                        access = MemberData.AccessType.Get;

                    else if (p.CanWrite)
                        access = MemberData.AccessType.Set;

                    int meta = p.MetadataToken;

                    members.Add(new MemberData(name, MemberData.MemberType.Property, access, meta));
                }

                gameClass.members = members;
                codebase.classes.Add(gameClass);
            }

#if UNITY_EDITOR
            EditorUtility.SetDirty(codebase);
            AssetDatabase.Refresh();
#endif
        }
    }

}
