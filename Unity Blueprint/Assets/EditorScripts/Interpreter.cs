using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Reflection;
using System.Linq;
using System.Linq.Expressions;
using System;
using System.IO;
public class RealTimeVar
{
    object obj;
    Type type;
}

public class CodeNode
{
    Func<object, object[], object> function;
    CodeNode nextPath;
    CodeNode falsePath;
    string returnVarName;
}

public class Interpreter
{
    public List<string> keywords = new List<string>()
    {        
    "Awake",	                        //Awake is called when the script instance is being loaded.
    "FixedUpdate",	                    //Frame-rate independent MonoBehaviour.FixedUpdate message for physics calculations.
    "LateUpdate",	                    //LateUpdate is called every frame, if the Behaviour is enabled.
    "OnAnimatorIK",	                    //Callback for setting up animation IK (inverse kinematics).
    "OnAnimatorMove",	                //Callback for processing animation movements for modifying root motion.
    "OnApplicationFocus",	            //Sent to all GameObjects when the player gets or loses focus.
    "OnApplicationPause",	            //Sent to all GameObjects when the application pauses.
    "OnApplicationQuit",	            //Sent to all game objects before the application quits.
    "OnAudioFilterRead",	            //If OnAudioFilterRead is implemented, Unity will insert a custom filter into the audio DSP chain.
    "OnBecameInvisible",	            //OnBecameInvisible is called when the renderer is no longer visible by any camera.
    "OnBecameVisible",	                //OnBecameVisible is called when the renderer became visible by any camera.
    "OnCollisionEnter",	                //OnCollisionEnter is called when this collider/rigidbody has begun touching another rigidbody/collider.
    "OnCollisionEnter2D",	            //Sent when an incoming collider makes contact with this object's collider (2D physics only).
    "OnCollisionExit",	                //OnCollisionExit is called when this collider/rigidbody has stopped touching another rigidbody/collider.
    "OnCollisionExit2D",	            //Sent when a collider on another object stops touching this object's collider (2D physics only).
    "OnCollisionStay",	                //OnCollisionStay is called once per frame for every collider/rigidbody that is touching rigidbody/collider.
    "OnCollisionStay2D",	            //Sent each frame where a collider on another object is touching this object's collider (2D physics only).
    "OnConnectedToServer",	            //Called on the client when you have successfully connected to a server.
    "OnControllerColliderHit",	        //OnControllerColliderHit is called when the controller hits a collider while performing a Move.
    "OnDestroy",	                    //Destroying the attached Behaviour will result in the game or Scene receiving OnDestroy.
    "OnDisable",	                    //This function is called when the behaviour becomes disabled.
    "OnDisconnectedFromServer",	        //Called on the client when the connection was lost or you disconnected from the server.
    "OnDrawGizmos",	                    //Implement OnDrawGizmos if you want to draw gizmos that are also pickable and always drawn.
    "OnDrawGizmosSelected",	            //Implement OnDrawGizmosSelected to draw a gizmo if the object is selected.
    "OnEnable",	                        //This function is called when the object becomes enabled and active.
    "OnFailedToConnect",	            //Called on the client when a connection attempt fails for some reason.
    "OnFailedToConnectToMasterServer",	//Called on clients or servers when there is a problem connecting to the MasterServer.
    "OnGUI",	                        //OnGUI is called for rendering and handling GUI events.
    "OnJointBreak",	                    //Called when a joint attached to the same game object broke.
    "OnJointBreak2D",	                //Called when a Joint2D attached to the same game object breaks.
    "OnMasterServerEvent",	            //Called on clients or servers when reporting events from the MasterServer.
    "OnMouseDown",	                    //OnMouseDown is called when the user has pressed the mouse button while over the Collider.
    "OnMouseDrag",	                    //OnMouseDrag is called when the user has clicked on a Collider and is still holding down the mouse.
    "OnMouseEnter",	                    //Called when the mouse enters the Collider.
    "OnMouseExit",	                    //Called when the mouse is not any longer over the Collider.
    "OnMouseOver",	                    //Called every frame while the mouse is over the Collider.
    "OnMouseUp",	                    //OnMouseUp is called when the user has released the mouse button.
    "OnMouseUpAsButton",	            //OnMouseUpAsButton is only called when the mouse is released over the same Collider as it was pressed.
    "OnNetworkInstantiate",	            //Called on objects which have been network instantiated with Network.Instantiate.
    "OnParticleCollision",	            //OnParticleCollision is called when a particle hits a Collider.
    "OnParticleSystemStopped",	        //OnParticleSystemStopped is called when all particles in the system have died, and no new particles will be born. New particles cease to be created either after Stop is called, or when the duration property of a non-looping system has been exceeded.
    "OnParticleTrigger",	            //OnParticleTrigger is called when any particles in a Particle System meet the conditions in the trigger module.
    "OnParticleUpdateJobScheduled",	    //OnParticleUpdateJobScheduled is called when a Particle System's built-in update job has been scheduled.
    "OnPlayerConnected",	            //Called on the server whenever a new player has successfully connected.
    "OnPlayerDisconnected",	            //Called on the server whenever a player disconnected from the server.
    "OnPostRender",	                    //OnPostRender is called after a camera finished rendering the Scene.
    "OnPreCull",	                    //OnPreCull is called before a camera culls the Scene.
    "OnPreRender",	                    //OnPreRender is called before a camera starts rendering the Scene.
    "OnRenderImage",	                //OnRenderImage is called after all rendering is complete to render image.
    "OnRenderObject",	                //OnRenderObject is called after camera has rendered the Scene.
    "OnSerializeNetworkView",	        //Used to customize synchronization of variables in a script watched by a network view.
    "OnServerInitialized",	            //Called on the server whenever a Network.InitializeServer was invoked and has completed.
    "OnTransformChildrenChanged",	    //This function is called when the list of children of the transform of the GameObject has changed.
    "OnTransformParentChanged",	        //This function is called when the parent property of the transform of the GameObject has changed.
    "OnTriggerEnter",	                //When a GameObject collides with another GameObject, Unity calls OnTriggerEnter.
    "OnTriggerEnter2D",	                //Sent when another object enters a trigger collider attached to this object (2D physics only).
    "OnTriggerExit",	                //OnTriggerExit is called when the Collider other has stopped touching the trigger.
    "OnTriggerExit2D",	                //Sent when another object leaves a trigger collider attached to this object (2D physics only).
    "OnTriggerStay",	                //OnTriggerStay is called once per physics update for every Collider other that is touching the trigger.
    "OnTriggerStay2D",	                //Sent each frame where another object is within a trigger collider attached to this object (2D physics only).
    "OnValidate",	                    //This function is called when the script is loaded or a value is changed in the Inspector (Called in the editor only).
    "OnWillRenderObject",	            //OnWillRenderObject is called for each camera if the object is visible and not a UI element.
    "Reset",	                        //Reset to default values.
    "Start",	                        //Start is called on the frame when a script is enabled just before any of the Update methods are called the first time.
    "Update"                            //Update is called every frame, if the MonoBehaviour is enabled.
 
    };

    static Interpreter mInstance;

    Interpreter() {}

    public static Interpreter Instance
    {   get
        {
            if (mInstance == null)
            {
                //Debug.Log("Instantiating new interpreter");
                mInstance = new Interpreter();
            }
            
            return mInstance;
        }
        private set { }
    }

    //Source: https://wiki.unity3d.com/index.php/CreateScriptableObjectAsset
    public T CreateAsset<T>(string filePath) where T: ScriptableObject
    {       
        T asset = AssetDatabase.LoadAssetAtPath<T>(filePath);

        if (asset == null)
        {
            //Debug.Log("Creating new instance of blueprint collection");
            asset = ScriptableObject.CreateInstance<T>();
            
            string assetPathAndName = AssetDatabase.GenerateUniqueAssetPath(filePath);
            AssetDatabase.CreateAsset(asset, assetPathAndName);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Selection.activeObject = asset;
            return asset;
        }

        else
        {
            //Debug.Log("collection was found returning");
            return asset;
        }
    }

    public BlueprintData LoadBlueprint(string name)
    {
        BlueprintData asset = AssetDatabase.LoadAssetAtPath<BlueprintData>("Assets/" + name + ".asset");
        return asset;
    }
       
    public void Compile()
    {
        string text = "";        
        object target;
        object[] passArgs;

        string proc = text;
        string[] raw = proc.Split(' ');
        string[] args = new string[raw.Length - 2];

        Component comp = null;
        //var comp = GameObject.GetComponent(raw[0]);        
        Type type = null;
        
        if (comp == null)
        {
            //Source: https://stackoverflow.com/questions/8499593/c-sharp-how-to-check-if-namespace-class-or-method-exists-in-c
            type = (from assembly in AppDomain.CurrentDomain.GetAssemblies() from type2 in assembly.GetTypes() where type2.Name == raw[0] select type2).FirstOrDefault();
        
            if (type == null)
            {
                Debug.Log("Interpreter Error: No Component or Class defintion Found");
                return;
            }
        }
        
        else
        {
            type = comp.GetType();
            target = comp;
        }
        
        Debug.Log("Type is " + type.ToString());
        
        Array.Copy(raw, 2, args, 0, raw.Length - 2);
        object[] finalArgs = ParseArgumentTypes(args);
        passArgs = finalArgs;
        
        MethodInfo method = GetMethodMatch(type, raw[1], finalArgs);
        
        Debug.Log(method.Name);                              
        //newTest = method.Bind();             
    }

    public void CompileNode(Node node, object target = null)
    {
        //Debug.Log("In compile node");
        if (node.isEntryPoint)
        {
            //Debug.Log("Node is entry point returning");
            return;
        }
           
        if (node.currentMethod == null)
        {
            //Debug.Log("Attempting to construct methodinfo");
            node.currentMethod = LoadMethod(node.input, node.type, node.assemblyPath, node.index);

            if (node.currentMethod == null)
            {
                //Debug.Log("Node has no method or is entry point");
                return;
            }
        }
             
        if (node.paramList.Count > 0)
        {
            node.passArgs = new object[node.paramList.Count];

            for (int i = 0; i < node.paramList.Count; i++)
            {               
                node.passArgs[i] = node.paramList[i].arg;
            }
        }

        if (target != null)
        {
            //Debug.Log("passed in target is not null");
            node.actualTarget = target;
        }

        node.function = node.currentMethod.Bind();
    }

    public MethodInfo LoadMethod(string input, string type, string path, int index)
    {
        //Debug.Log($"location test {path}");
        if (path == "")
            return null;

        Assembly TestASM = Assembly.LoadFile(path);

        string[] args = input.Split(' ');
        string name = "";

        if (args.Length > 1)
            name = args[1];
        else
        {
            Debug.Log("No function name found returning null");
            return null;
        }

        if (TestASM != null)
        {
            Type typeTest = TestASM.GetType(type.ToString());
            //Debug.Log($"Type test from assembly load test {typeTest.ToString()}");
            MethodInfo[] allMethods = typeTest.GetMethods();
            List<MethodInfo> methods = new List<MethodInfo>();

            MethodInfo final = null;

            foreach (MethodInfo m in allMethods)
            {
                if (m.Name == name)
                    methods.Add(m);
            }

            final = methods[index];
            return final;

        }

        return null;
    }

    public void ParseKeywords(string text, Node node)
    {
        //MethodInfo info = typeof(MonoBehaviour).GetMethod(text);
        MethodInfo info = typeof(BlueprintComponent).GetMethod(text);
        if (info != null)
        {
            Debug.Log($"Method found for {text}");          
            Debug.Log($"keyword {text}");
            Debug.Log($"node: {node.ToString()}");

            //if (node.blueprint.activeFunctions == null)
            //    node.blueprint.activeFunctions = new List<string>();

            //node.blueprint.activeFunctions.Add(text);

            node.isDefined = true;
            node.isEntryPoint = true;
        }        
    }

    public T CreateInstance<T>(Type type)
    {
        //This feels like it might be useful to store these Func<> types in a dictionary of <Type, Func>
        //Source: https://stackoverflow.com/questions/752/how-to-create-a-new-object-instance-from-a-type

        Func<T> creator = Expression.Lambda<Func<T>>(Expression.New(type.GetConstructor(Type.EmptyTypes))).Compile();
        T obj = creator();
        
        if (obj == null) Debug.Log("obj in create instance is NULL");
        return obj;
    }
        
    public MethodInfo[] GetFunctionDefinitions(string text, out string typeStr, out string asmPath)
    {
        string proc = text;
        string[] raw = proc.Split(' ');
        
        if (raw.Length <= 1)
        {
            typeStr = "";
            asmPath = "";            
            return null;
        }

        string name = raw[1];        
        
        Type type = null;
        Assembly ASM = null;
        //Source: https://stackoverflow.com/questions/8499593/c-sharp-how-to-check-if-namespace-class-or-method-exists-in-c
        //type = (from assembly in AppDomain.CurrentDomain.GetAssemblies() from type2 in assembly.GetTypes() where type2.Name == raw[0] select type2).FirstOrDefault();

        foreach(Assembly asm in AppDomain.CurrentDomain.GetAssemblies())
        {
            Type[] types = asm.GetTypes();

            foreach(Type t in types)
            {
                if (t.Name == raw[0])
                {
                    type = t;
                    ASM = asm;
                }
            }
        }

        //string location = ASM.Location;
        //Debug.Log($"location test {location}");
        //Assembly TestASM = Assembly.LoadFile(location);
        //
        //if (TestASM != null)
        //{
        //    Type typeTest = TestASM.GetType(type.ToString());
        //    Debug.Log($"Type test from assembly load test {typeTest.ToString()}");            
        //}

        if (type == null)
        {
            Debug.Log("Interpreter Error: No Component or Class defintion Found");
            typeStr = "";
            asmPath = "";            
            return null;
        }

        typeStr = type.ToString();
        asmPath = ASM.Location;
                              
        List<MethodInfo> target = new List<MethodInfo>();
        MethodInfo[] methods = type.GetMethods();

        //Assembly asm = type.Assembly;        
        string test = type.ToString();
        

        foreach (MethodInfo m in methods)
        {
            if (m.Name == name)
                target.Add(m);
        }
        
        return target.ToArray();
      
    }

    object[] ParseArgumentTypes(string[] args)
    {
        object[] objs = new object[args.Length];

        for (int i = 0; i < args.Length; i++)
        {
            //TO-DO: Add Keywords or find a way to get members or possible function calls, maybe try using the LINQ thing above 
            bool bVal;
            int iVal;
            float fVal;
            double dVal;            

            if (Boolean.TryParse(args[i], out bVal))
            {
                //print("Parsing Bool!");
                objs[i] = bVal;
                continue;
            }

            if (Int32.TryParse(args[i], out iVal))
            {
                //print("Parsing int");
                objs[i] = iVal;
                continue;
            }

            if (Single.TryParse(args[i], out fVal))
            {
                //print("Parsing float");
                objs[i] = fVal;
                continue;
            }

            if (Double.TryParse(args[i], out dVal))
            {
                //print("Parsing string");
                objs[i] = dVal;
                continue;
            }

            // Check the " character
            if (args[i][0] == '"')
            {
                objs[i] = args[i];
            }
        }

        return objs;
    }

    MethodInfo GetMethodMatch(Type type, string name, object[] args)
    {
        MethodInfo[] initial = type.GetMethods();
        List<MethodInfo> methods = new List<MethodInfo>();

        foreach(MethodInfo m in initial)
        {
            if (m.Name == name)
                methods.Add(m);
        }

        if (methods.Count > 0)
        {            
            foreach (MethodInfo m in methods)
            {
                bool isMatch = true;
                ParameterInfo[] parameters = m.GetParameters();

                if (parameters.Length != args.Length)
                    continue;
                else
                {                    
                    for (int i = 0; i < parameters.Length; i++)
                        if (parameters[i].ParameterType != args[i].GetType())
                            isMatch = false;                        

                    if (isMatch)
                        return m;
                }
            }
        }
        return null;
    }
}

