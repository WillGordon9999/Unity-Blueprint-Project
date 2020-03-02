using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Blueprint
{
    public string name;
    public Dictionary<string, Node> entryPoints;

    //Editor side stuff
    public List<Node> nodes;
    public List<Connection> connections;

    public Blueprint()
    {
        nodes = new List<Node>();
        connections = new List<Connection>();
        entryPoints = new Dictionary<string, Node>();
    }
}

