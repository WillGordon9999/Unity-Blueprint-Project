using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class BlueprintData : ScriptableObject
{
    public string ComponentName;
    public List<NodeData> nodes;
    public int ID_Count = 0; //This will contain the max number of ids for each node
    //public List<ConnectionPointData> connections;
    public List<ConnectionData> connections;
    public List<string> activeFunctions;    
}

//public string ComponentName;
//public string json;
//public Dictionary<string, RealTimeVar> variables;
//public Dictionary<string, Node> entryPoints;
//public List<string> activeFunctions;
//public List<Node> nodes;
//public List<Connection> connections;
//public float dotProductTest;
//public int ptr;
//public string typeTest;
//public System.Type type;
//public string CompileTest;
//public UnityEngine.Object objectTest;
//public System.Enum enumTest;
//public Component compTest;
//
////textObjectReferences.Find((x) => x.gameObject.name == variableName).text = levelData[variableName][currentLevelIndex];
//
//public BlueprintData(string name)
//{
//    ComponentName = name;
//    variables = new Dictionary<string, RealTimeVar>();
//    entryPoints = new Dictionary<string, Node>();
//    activeFunctions = new List<string>();
//    nodes = new List<Node>();
//}
//
//public void Create(string name)
//{
//    ComponentName = name;
//    variables = new Dictionary<string, RealTimeVar>();
//    entryPoints = new Dictionary<string, Node>();
//    activeFunctions = new List<string>();
//    nodes = new List<Node>();
//}