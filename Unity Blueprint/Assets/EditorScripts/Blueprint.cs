using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Blueprint
{
    public string name;
    public Dictionary<string, Node> entryPoints;
    public Dictionary<string, Var> variables;
    public Dictionary<string, Var> passInParams;

    //Editor side stuff
    public List<Node> nodes;
    public List<Connection> connections;
    public BlueprintData dataRef;

    public Blueprint()
    {
        nodes = new List<Node>();
        connections = new List<Connection>();
        entryPoints = new Dictionary<string, Node>();
        variables = new Dictionary<string, Var>();
    }
}

