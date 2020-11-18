using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class BlueprintManager : MonoBehaviour
{
    //public static Dictionary<string, Blueprint> blueprints;        
    public static Dictionary<BlueprintData, Blueprint> blueprints;

    //As it stands right now, the dictionary can instantiate at editor time, however after closing down
    //that session and opening a new one, that will destroy the dictionary and I believe cause it construct on playmode 
    //when OnValidate is next called.
    [ExecuteAlways]
    private void OnValidate()
    {
        //print("In OnValidate");
        if (blueprints == null)
        {
            //print("Instantiating new blueprint dictionary");
            //blueprints = new Dictionary<string, Blueprint>();
            blueprints = new Dictionary<BlueprintData, Blueprint>();            
        }

        //else
        //    print("Dictionary instantiated");              
    }
   
    private void Update()
    {
        if (blueprints == null)
        {
            //print("Instantiating new blueprint dictionary at runtime");
            //blueprints = new Dictionary<string, Blueprint>();
            blueprints = new Dictionary<BlueprintData, Blueprint>();
        }
    }
}
