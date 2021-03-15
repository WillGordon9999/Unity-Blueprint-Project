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

    public ClassData(string type, string asm)
    {
        className = type;
        classASM = asm;
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
    public List<string> asmsToDestroy;
    // Start is called before the first frame update
    void OnEnable()
    {
        if (mInstance == null)
            mInstance = this;

        asmsToDestroy = new List<string>();

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
            foreach (ClassData data in classes.classes)
            {                
                Assembly asm = Assembly.LoadFile(data.classASM);
                Type newType = asm.GetType(data.className);
                gameObject.AddComponent(newType);
            }
        }

        else
            classes = new ClassCollection();
    }

    private void OnApplicationQuit()
    {
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

    // Update is called once per frame
    void Update()
    {
        
    }

    public void AddClass(string name, string asm)
    {
        if (name != null && asm != null)
        {
            print("Successfully added new class to inventory");
            classes.classes.Add(new ClassData(name, asm));
            Assembly asmObj = Assembly.LoadFile(asm);
            Type newType = asmObj.GetType(name);
            gameObject.AddComponent(newType);
        }
    }

    public void RemoveClass(string name, string path = "")
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
            if (path != "")
                System.IO.File.Delete(path);
        }
    }
}
