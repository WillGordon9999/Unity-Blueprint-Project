using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.IO;
using System.Reflection;

[System.AttributeUsage
    (System.AttributeTargets.Class | System.AttributeTargets.Struct 
    | System.AttributeTargets.Field | System.AttributeTargets.Method | 
    System.AttributeTargets.Constructor | System.AttributeTargets.Property, AllowMultiple = true)]
public class UnlockStatus : System.Attribute
{
    public string name;    
    public bool unlocked;
    public object obj;

    public UnlockStatus(string objectName, bool unlock)
    {
        name = objectName;
        unlocked = unlock;        
        Debug.Log("Adding UnlockStatus");
        GameUnlockManager.Instance.AddUnlock(this);
    }

}

[System.Serializable]
public class UnlockData
{
    public string name;
    public bool unlocked;

    public UnlockData(string objName, bool unlock)
    {
        name = objName;
        unlocked = unlock;
    }

}

[System.Serializable]
public class UnlockFile
{
    public List<UnlockData> unlocks;

    public UnlockFile()
    {
        unlocks = new List<UnlockData>();
    }

}


public class GameUnlockManager : MonoBehaviour
{
    public static GameUnlockManager Instance { get { return mInstance; } set { } }
    private static GameUnlockManager mInstance;
    public static List<UnlockStatus> unlocks = new List<UnlockStatus>();
    public bool UseUnlocks = true;
    UnlockFile unlockFile;
    
    private void Awake()
    {
        if (mInstance == null)
            mInstance = this;

        if (File.Exists(Application.persistentDataPath + "/" + "Unlocks.asset"))
        {
            print("Loading unlocks");
            string json = File.ReadAllText(Application.persistentDataPath + "/" + "Unlocks.asset");
            unlockFile = JsonUtility.FromJson<UnlockFile>(json);

            //foreach(UnlockStatus status in unlocks)
            //{
            //    foreach(UnlockData data in unlockFile.unlocks)
            //    {
            //        if (data.name == status.name)
            //            status.unlocked = data.unlocked;
            //    }
            //}
        }

        if (unlockFile == null)
            unlockFile = new UnlockFile();
    }

    private void OnApplicationQuit()
    {
        if (unlockFile.unlocks.Count > 0)
        {
            print("Saving Unlocks");
            string json = JsonUtility.ToJson(unlockFile);
            File.WriteAllText(Application.persistentDataPath + "/" + "Unlocks.asset", json);
        }
    }

    
    // Update is called once per frame
    void Update()
    {
        Interpreter.Instance.UseUnlocks = UseUnlocks;
    }

    public void AddUnlock(UnlockStatus newUnlock)
    {
        if (unlockFile != null)
        {
            foreach (UnlockData data in unlockFile.unlocks)
            {
                if (data.name == newUnlock.name)
                {
                    return;
                }
            }

            print("Adding new Unlock");
            //unlocks.Add(newUnlock);
            unlockFile.unlocks.Add(new UnlockData(newUnlock.name, newUnlock.unlocked));
        }
    }

    public bool CheckUnlock(MemberInfo info)
    {       
        UnlockStatus status = info.GetCustomAttribute<UnlockStatus>();

        if (status != null)
        {
            foreach(UnlockData data in unlockFile.unlocks)
            {
                if (data.name == status.name)                
                    return data.unlocked;                
            }

            return false;
        }

        return true; //Because if status is null we want to be able to access it
    }

    public bool CheckUnlock(Type type)
    {
        UnlockStatus status = type.GetCustomAttribute<UnlockStatus>();

        if (status != null)
        {
            foreach (UnlockData data in unlockFile.unlocks)
            {
                if (data.name == status.name)
                    return data.unlocked;
            }

            return false;
        }

        return true; //Because if status is null we want to be able to access it
    }

    public void SetUnlock(string name, bool val)
    {
        if (unlockFile != null)
        {
            foreach (UnlockData data in unlockFile.unlocks)
            {
                if (data.name == name)
                {
                    data.unlocked = val;
                    return;
                }
            }
        }
    }
}
