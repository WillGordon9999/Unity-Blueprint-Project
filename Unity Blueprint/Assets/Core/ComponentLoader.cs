using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Reflection;
using System;

public class ComponentLoader : MonoBehaviour
{
    public List<BlueprintData> components;

    private void OnEnable()
    {
        if (components != null)
        {
            foreach(BlueprintData data in components)
            {
                Assembly asm = Assembly.LoadFile(data.compiledClassTypeAsmPath);
                Type newType = asm.GetType(data.compiledClassType);
                gameObject.AddComponent(newType);
            }
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
