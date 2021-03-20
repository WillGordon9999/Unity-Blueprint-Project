using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Reflection;
using System.IO;
using System.Runtime.CompilerServices;
using System.CodeDom.Compiler;
using Microsoft.CSharp;
public class AppDomainTest : MonoBehaviour
{
    AppDomain domain;
    const string fullName = "LoadTest2, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null";

    const string classDef = @"using Game;\n public class TestComponent : GameComponent { public void Start() { print(GetType().ToString()); } }";

    private void Awake()
    {
        //string codeText =
        //    @"public class Test
        //    {
        //       public static void GetGameObjectName (UnityEngine.Object obj) { UnityEngine.Debug.Log( "" Test Debug "" + obj.name); }
        //    }
        //    ";
        //
        //CSharpCodeProvider code = new CSharpCodeProvider();
        //CompilerParameters parameters = new CompilerParameters();
        //
        //parameters.GenerateInMemory = true;
        //parameters.ReferencedAssemblies.Add(typeof(MonoBehaviour).Assembly.Location);
        //parameters.ReferencedAssemblies.Add(this.GetType().Assembly.Location);
        //parameters.ReferencedAssemblies.Add(typeof(string).Assembly.Location);
        //parameters.ReferencedAssemblies.Add(typeof(GameObject).Assembly.Location);
        //parameters.ReferencedAssemblies.Add(typeof(Debug).Assembly.Location); 
        //parameters.OutputAssembly = "MemoryTest.dll";
        //
        //CompilerResults compile = code.CompileAssemblyFromSource(parameters, codeText);
        //
        //if (compile.Errors.Count > 0)
        //{
        //    Debug.Log($"Errors building {classDef} to  {compile.PathToAssembly}");
        //
        //    foreach (CompilerError error in compile.Errors)
        //    {
        //        Debug.Log($"  {error.ToString()}  ");
        //    }
        //}
        //
        //TempFileCollection files = compile.TempFiles;
        //files.KeepFiles = true;
        //print(files.BasePath);
        //print(files.ToString());

        //if (asmPath != null)
        //{
        //    print(asmPath);
        //    //byte[] array = File.ReadAllBytes(asmPath);
        //}

        string path = @"C:/Users/Will/Desktop/Unity Blueprint Repo/Unity-Blueprint-Project/Unity Blueprint/LoadTest2.dll";
        //Assembly asm = Assembly.LoadFile("C:/Users/Will/Desktop/Unity Blueprint Repo/Unity-Blueprint-Project/Unity Blueprint/LoadTest2.dll");
        //print(asm.FullName);

        //domain = AppDomain.CreateDomain("TestDomain");
        //FileStream fs = new FileStream("C:/Users/Will/Desktop/Unity Blueprint Repo/Unity-Blueprint-Project/Unity Blueprint/LoadTest2.dll", FileMode.Open);
        //byte[] rawASM = new byte[(int)fs.Length];
        //fs.Read(rawASM, 0, rawASM.Length);
        //fs.Close();
        //domain.Load(rawASM);

        //Assembly asm = Assembly.LoadFrom("C:/Users/Will/Desktop/Unity Blueprint Repo/Unity-Blueprint-Project/Unity Blueprint/LoadTest2.dll");
        //GameComponent comp = (GameComponent)domain.CreateInstanceFromAndUnwrap(path, "LoadTest2");
        //comp.print("Game component print");
        //domain.Load(this.GetType().Assembly.FullName);
        //domain.Load(path);
        //System.Runtime.Remoting.ObjectHandle instance = domain.CreateInstanceFrom(path, "LoadTest2");
        //GameComponent comp = (GameComponent)instance.Unwrap();
        
        //string date = DateTime.Now.ToShortDateString() + "_" + DateTime.Now.ToLongTimeString();
        //print(date);
        //date = date.Replace('/', '_');
        //date = date.Replace(' ', '_');
        //date = date.Replace(':', '_');
        //print(date);      
        
        

    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        //if (Input.GetKeyDown(KeyCode.Return))
        //{
        //    AppDomain.Unload(domain);
        //}
    }
}
