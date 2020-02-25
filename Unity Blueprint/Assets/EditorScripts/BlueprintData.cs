using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BlueprintData : ScriptableObject
{
    public string ComponentName;
    public string json;
    public Dictionary<string, RealTimeVar> variables;
    public Dictionary<string, Node> entryPoints;
    public List<string> activeFunctions;
    public List<Node> nodes;
    public List<Connection> connections;

    public BlueprintData(string name)
    {
        ComponentName = name;
        variables = new Dictionary<string, RealTimeVar>();
        entryPoints = new Dictionary<string, Node>();
        activeFunctions = new List<string>();
        nodes = new List<Node>();
    }

    public void Create(string name)
    {
        ComponentName = name;
        variables = new Dictionary<string, RealTimeVar>();
        entryPoints = new Dictionary<string, Node>();
        activeFunctions = new List<string>();
        nodes = new List<Node>();
    }
}
