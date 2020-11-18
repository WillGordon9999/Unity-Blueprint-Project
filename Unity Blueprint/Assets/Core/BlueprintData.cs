using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class BlueprintData : ScriptableObject
{
    public string ComponentName;
    public List<NodeData> nodes;
    public List<Var> variables;
    public List<Var> passInParams; //For functions
    public int ID_Count = 0; //This will contain the max number of ids for each node    
    public List<ConnectionData> connections;

    public string compiledClassType;
    public string compiledClassTypeAsmPath;

}

[System.Serializable]
public class BlueprintFile
{
    public string ComponentName;
    public List<NodeData> nodes;
    public List<Var> variables;
    public List<Var> passInParams;
    public int ID_Count = 0; //This will contain the max number of ids for each node    
    public List<ConnectionData> connections;

    public string compiledClassType;
    public string compiledClassTypeAsmPath;
}

//Supposed thing for turning lists into dictionaries
////textObjectReferences.Find((x) => x.gameObject.name == variableName).text = levelData[variableName][currentLevelIndex];
