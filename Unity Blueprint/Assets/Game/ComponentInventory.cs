using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Reflection;
using System;

[System.Serializable]
public class ClassData
{
    public string className;
    public string classASM;
    public string asmName;

    public ClassData(string type, string asm, string aName)
    {
        className = type;
        classASM = asm;
        asmName = aName;
    }
}

[System.Serializable]
public class ClassCollection
{
    public List<ClassData> classes;
}


public class ComponentInventory : MonoBehaviour
{
    public static ComponentInventory Instance { get { return mInstance; } set { } }
    static ComponentInventory mInstance;

    public ClassCollection classes;
    public ClassCollection customClasses;
    List<Type> customTypes;
    List<AppDomain> appDomains;
    NukeList list;
    //public List<string> asmsToDestroy;
    // Start is called before the first frame update
    void OnEnable()
    {
        if (mInstance == null)
            mInstance = this;

        //asmsToDestroy = new List<string>();
        appDomains = new List<AppDomain>();

        //Nuke List
        if (System.IO.File.Exists(Application.persistentDataPath + "/" + "OldList.asset"))
        {
            string json = System.IO.File.ReadAllText(Application.persistentDataPath + "/" + "OldList.asset");
            list = JsonUtility.FromJson<NukeList>(json);
        }

        if (list != null)
        {
            foreach (string str in list.list)
            {
                print($"Deleting {str}");
                try
                {
                    System.IO.File.Delete(str);
                }

                catch { };
            }

            list.list.Clear();
        }

        else
            list = new NukeList();

        if (System.IO.File.Exists(Application.persistentDataPath + "/" + "CustomClasses.asset"))
        {
            string json = System.IO.File.ReadAllText(Application.persistentDataPath + "/" + "CustomClasses.asset");
            customClasses = JsonUtility.FromJson<ClassCollection>(json);
        }

        customTypes = new List<Type>();

        if (customClasses != null)
        {
            foreach (ClassData data in customClasses.classes)
            {
                Assembly asm = Assembly.LoadFile(data.classASM);
                if (asm != null)
                    customTypes.Add(asm.GetType(data.className));
                //if (File.Exists(data.classASM))
                //{
                //    Assembly asm = Assembly.LoadFile(data.classASM);
                //    //Assembly asm = Assembly.ReflectionOnlyLoad(data.asmName);
                //    //Assembly asm = Assembly.ReflectionOnlyLoadFrom(data.classASM);
                //
                //    if (asm != null)
                //    {
                //        //print($"Loading {data.asmName}");
                //        //AppDomain domain = AppDomain.CreateDomain(data.className);
                //        //AssemblyName newName = new AssemblyName(data.asmName);
                //        ////Assembly asm = domain.Load(newName);
                //        //domain.Load(asm.GetName());
                //
                //        //FileStream fs = new FileStream(data.classASM, FileMode.Open);
                //        //byte[] rawASM = new byte[(int)fs.Length];
                //        //fs.Read(rawASM, 0, rawASM.Length);
                //        //fs.Close();
                //        
                //        //domain.Load(asm.GetName());
                //        customTypes.Add(asm.GetType(data.className));
                //        //appDomains.Add(domain);
                //    }
                //}

            }
        }

        else
        {
            customClasses = new ClassCollection();
        }

        if (File.Exists(Application.persistentDataPath + "/" + "Inventory.asset"))
        {
            print("Found Inventory file");            
            string json = File.ReadAllText(Application.persistentDataPath + "/" + "Inventory.asset");
            classes = JsonUtility.FromJson<ClassCollection>(json);

            //BinaryFormatter bf = new BinaryFormatter();
            //FileStream file = File.Open(Application.persistentDataPath + "/Inventory.asset", FileMode.Open);
            //classes = (ClassCollection)bf.Deserialize(file);
            //file.Close();
        }
        
        if (classes != null)
        {
            //print("Adding Inventory classes");
            foreach (ClassData data in classes.classes)
            {
                //Assembly asm = Assembly.LoadFile(data.classASM);
                //Type newType = asm.GetType(data.className);
                //gameObject.AddComponent(newType);
                Type newType = SearchClass(data.className);
                gameObject.AddComponent(newType);
            }
        }

        else
            classes = new ClassCollection();
    }

    private void OnApplicationQuit()
    {
        if (customClasses.classes.Count > 0)
        {
            print("Saving Inventory");
            string json = JsonUtility.ToJson(customClasses);
            System.IO.File.WriteAllText(Application.persistentDataPath + "/" + "CustomClasses.asset", json);

            json = JsonUtility.ToJson(list);
            System.IO.File.WriteAllText(Application.persistentDataPath + "/" + "OldList.asset", json);
        }

        if (classes.classes.Count > 0)
        {
            print("Saving Inventory");            
            string json = JsonUtility.ToJson(classes);            
            File.WriteAllText(Application.persistentDataPath + "/" + "Inventory.asset", json);

            //BinaryFormatter bf = new BinaryFormatter();
            //FileStream file = File.Create(Application.persistentDataPath + "/Inventory.asset");
            //bf.Serialize(file, classes);
            //file.Close();

        }        
    }

    public void AddOldClass(string path)
    {
        list.list.Add(path);
    }

    public Type SearchClass(string name)
    {
        //print("name being searched for is " + name);
        foreach (Type t in customTypes)
        {
            if (t.ToString() == name)
            {
                //print("Found Custom Class in GameManager");
                return t;
            }
        }

        return null;
    }

    public void AddClassToInventory(string name, string asm, string asmName)
    {
        if (name != null && asm != null)
        {
            print("Successfully added new class to inventory");
            classes.classes.Add(new ClassData(name, asm, asmName));
            Assembly asmObj = Assembly.LoadFile(asm);
            Type newType = asmObj.GetType(name);            
            gameObject.AddComponent(newType);
        }
    }

    //In the new template add state, we want to make sure the state gets removed properly if recompiled
    public void AddClassToInventoryFromState<T>() where T: GameComponent
    {
        Type newType = typeof(T);
        classes.classes.Add(new ClassData(newType.Name, newType.Assembly.Location, newType.Assembly.GetName().ToString()));
    }

    public void AddClassToInventoryFromType(Type type)
    {
        classes.classes.Add(new ClassData(type.Name, type.Assembly.Location, type.Assembly.GetName().ToString()));
    }

    public void AddCustomClass(Type type)
    {
        if (type != null)
        {
            print("Adding class to Custom Class list");
            customClasses.classes.Add(new ClassData(type.ToString(), type.Assembly.Location, type.Assembly.GetName().ToString()));
            customTypes.Add(type);

            //AppDomain domain = AppDomain.CreateDomain(type.Name);
            //domain.Load(type.Assembly.GetName());
            //appDomains.Add(domain);

        }
    }

    public void RemoveClassFromInventory(string name, string path = "")
    {
        ClassData data = null;
        foreach(var comp in classes.classes)
        {
            if (comp.className == name)
            {
                data = comp;
                break;
            }
        }

        if (data != null)
        {
            classes.classes.Remove(data);
            Destroy(GetComponent(name));
            //if (path != "")
            //    System.IO.File.Delete(path);
        }
    }

    public void RemoveCustomClass(string name)
    {
        ClassData data = null;
        foreach (var comp in customClasses.classes)
        {
            if (comp.className == name)
            {
                data = comp;
                break;
            }
        }

        if (data != null)
        {
            Type target = null;
            foreach (Type t in customTypes)
            {
                if (t.Name == name)
                {
                    target = t;
                    break;
                }
            }

            if (target != null)
            {
                print("Removing " + target + " from custom types");
                customTypes.Remove(target);
            }

            AddOldClass(data.classASM);
            customClasses.classes.Remove(data);
        }

        //foreach (AppDomain domain in appDomains)
        //{
        //    if (domain.FriendlyName == name)
        //    {
        //        print("Removing " + name + " from appDomains");
        //        appDomains.Remove(domain);
        //        AppDomain.Unload(domain);
        //        return;
        //    }
        //}
    }
}
