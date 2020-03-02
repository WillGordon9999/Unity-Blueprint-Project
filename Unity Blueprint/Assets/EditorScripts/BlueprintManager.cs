using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//As static class and static dictionary it does work when called in onvalidate 
//public static class BlueprintManager 
//{
//    public static Dictionary<string, Blueprint> blueprints;      
//}

public class BlueprintManager : MonoBehaviour
{
    public static Dictionary<string, Blueprint> blueprints;
    public BlueprintData data; //Test
    //public static Dictionary<BlueprintData, Blueprint> blueprints;

    private void OnValidate()
    {
        if (blueprints == null)
        {
            print("Instantiating new blueprint dictionary");
            blueprints = new Dictionary<string, Blueprint>();
            //blueprints = new Dictionary<BlueprintData, Blueprint>();
        }

        if (Application.isPlaying)
        {
            print("In construction of blueprint");
            Blueprint bp = new Blueprint();
            bp.name = data.ComponentName;

            foreach(NodeData node in data.nodes)
            {
                Node newNode = new Node(node, null, null, null);
                
                if (node.isEntryPoint)
                {
                    Debug.Log("Entry point confirmed");
                    bp.entryPoints[node.input] = newNode;
                }

                bp.nodes.Add(newNode);
            }
           
            foreach(Node node in bp.nodes)
            {
                //Interpreter.Instance.CompileNode(node);
                
                foreach(Node node2 in bp.nodes)
                {
                    if (node.nextID == node2.ID)
                    {
                        node.nextNode = node2;
                        break;
                    }
                }
            }
            

            BlueprintManager.blueprints[bp.name] = bp;

        }
    }

    private void Update()
    {
        if (blueprints == null)
        {
            print("Instantiating new blueprint dictionary at runtime");
            blueprints = new Dictionary<string, Blueprint>();
            //blueprints = new Dictionary<BlueprintData, Blueprint>();
        }
    }
}
