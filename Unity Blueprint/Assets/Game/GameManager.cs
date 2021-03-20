using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;
using System.Reflection;

[System.Serializable]
public class NukeList
{
    public List<string> list;

    public NukeList()
    {
        list = new List<string>();
    }
}

public class GameManager : MonoBehaviour
{
    private static GameManager mInstance;
    public static GameManager Instance { get { return mInstance; } set { } }
    public GameCodebase codebase;
    public bool UseGameCompile;
    public string input = "";

    public ClassCollection customClasses;
    List<Type> customTypes;
    List<AppDomain> appDomains;
    NukeList list;    

    private void OnEnable()
    {
        if (mInstance == null)
        {
            mInstance = this;            
        }

        Interpreter.Instance.UseGameCompile = UseGameCompile;
        
        //Nuke List
        //if (System.IO.File.Exists(Application.persistentDataPath + "/" + "OldList.asset"))
        //{
        //    string json = System.IO.File.ReadAllText(Application.persistentDataPath + "/" + "OldList.asset");
        //    list = JsonUtility.FromJson<NukeList>(json);
        //}
        //
        //if (list != null)
        //{
        //    foreach (string str in list.list)
        //    {
        //        print("Attempting to delete lingering old DLLs");
        //        System.IO.File.Delete(str);
        //    }
        //
        //    list.list.Clear();
        //}
        //
        //else
        //    list = new NukeList();

        //Set up Custom Class Collection
        //if (System.IO.File.Exists(Application.persistentDataPath + "/" + "CustomClasses.asset"))
        //{
        //    string json = System.IO.File.ReadAllText(Application.persistentDataPath + "/" + "CustomClasses.asset");
        //    customClasses = JsonUtility.FromJson<ClassCollection>(json);
        //}
        //
        //customTypes = new List<Type>();
        //
        //if (customClasses != null)
        //{
        //    foreach (ClassData data in customClasses.classes)
        //    {
        //        //Assembly asm = Assembly.LoadFile(data.classASM);
        //        //if (asm != null)
        //        //    customTypes.Add(asm.GetType(data.className));                
        //        AppDomain domain = AppDomain.CreateDomain(data.className);
        //        domain.Load(data.classASM);
        //        appDomains.Add(domain);
        //
        //    }
        //}
        //
        //else
        //{
        //    customClasses = new ClassCollection();
        //}
    }


    // Start is called before the first frame update
    void Start()
    {
        //foreach(GameClass game in codebase.classes)
        //{
        //    System.Text.StringBuilder builder = new System.Text.StringBuilder();
        //    
        //    foreach (MemberData member in game.members)
        //    {
        //        builder.Append(member.name + "\n");
        //    }
        //
        //    System.IO.File.WriteAllText($"Assets/Reflection/{game.className}.txt", builder.ToString());
        //}        
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
        Interpreter.Instance.UseGameCompile = UseGameCompile;

        //if (Input.GetKeyDown(KeyCode.Return) && input != "")
        //{
        //    Assembly assembly = this.GetType().Assembly;
        //
        //    foreach(Type t in assembly.GetTypes())
        //    {
        //        if (t.Namespace == "Game" && t.Name == input)
        //        {
        //            print("Should add class");
        //            AddClassToCodeBase(t);
        //            break;
        //        }
        //    }            
        //}
    }   //

    private void OnApplicationQuit()
    {
        //if (customClasses.classes.Count > 0)
        //{
        //    print("Saving Inventory");
        //    string json = JsonUtility.ToJson(customClasses);
        //    System.IO.File.WriteAllText(Application.persistentDataPath + "/" + "CustomClasses.asset", json);
        //
        //    //json = JsonUtility.ToJson(list);
        //    //System.IO.File.WriteAllText(Application.persistentDataPath + "/" + "OldList.asset", json);
        //
        //    //BinaryFormatter bf = new BinaryFormatter();
        //    //FileStream file = File.Create(Application.persistentDataPath + "/Inventory.asset");
        //    //bf.Serialize(file, classes);
        //    //file.Close();
        //
        //}        
    }

    private void OnGUI()
    {
        //input = GUI.TextField(new Rect(Screen.width / 2, Screen.height / 2, 300, 100), input);
    }

    //public void AddOldClass(string path)
    //{
    //    list.list.Add(path);
    //}


    //public void AddClass(Type type)
    //{
    //    if (type != null)
    //    {
    //        print("Adding class to Custom Class list");
    //        customClasses.classes.Add(new ClassData(type.ToString(), type.Assembly.Location));
    //        customTypes.Add(type);
    //
    //        AppDomain domain = AppDomain.CreateDomain(type.Name);
    //        domain.Load(type.Assembly.Location);
    //        appDomains.Add(domain);
    //
    //    }
    //}

    //public Type SearchClass(string name)
    //{
    //    foreach(Type t in customTypes)
    //    {
    //        if (t.Name == name)
    //        {
    //            print("Found Custom Class in GameManager");
    //            return t;
    //        }
    //    }
    //
    //    return null;
    //}

    //public void RemoveClass(string name)
    //{
    //    ClassData data = null;
    //    foreach (var comp in customClasses.classes)
    //    {
    //        if (comp.className == name)
    //        {
    //            data = comp;
    //            break;
    //        }
    //    }
    //
    //    if (data != null)
    //    {
    //        Type target = null;
    //        foreach(Type t in customTypes)
    //        {
    //            if (t.Name == name)
    //            {
    //                target = t;
    //                break;
    //            }
    //        }
    //
    //        if (target != null)
    //            customTypes.Remove(target);
    //
    //        customClasses.classes.Remove(data);                        
    //    }
    //
    //    foreach(AppDomain domain in appDomains)
    //    {
    //        if (domain.FriendlyName == name)
    //        {
    //            AppDomain.Unload(domain);
    //            appDomains.Remove(domain);
    //            return;
    //        }
    //    }
    //}

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

    void AddClassToCodeBase(Type t)
    {
        if (codebase.classes == null)
            codebase.classes = new List<GameClass>();

        GameClass gameClass = new GameClass(t.Name, ClassAccess.All);

        List<MemberData> members = new List<MemberData>();

        ConstructorInfo[] constructors = t.GetConstructors(BindingFlags.DeclaredOnly);

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

        //MethodInfo[] methods = t.GetMethods();
        MethodInfo[] methods = t.GetMethods(BindingFlags.DeclaredOnly);

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

        FieldInfo[] fields = t.GetFields(BindingFlags.DeclaredOnly);

        foreach (FieldInfo f in fields)
        {
            string name = f.FieldType + " " + f.Name;
            int meta = f.MetadataToken;

            members.Add(new MemberData(name, MemberData.MemberType.Field, MemberData.AccessType.Both, meta));
        }

        PropertyInfo[] props = t.GetProperties(BindingFlags.DeclaredOnly);

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

#if UNITY_EDITOR
        EditorUtility.SetDirty(codebase);
        AssetDatabase.Refresh();
#endif
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
