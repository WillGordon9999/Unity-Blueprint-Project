using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Reflection;
using System.Linq;
using System.Linq.Expressions;
using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.CodeDom.Compiler;
using Microsoft.CSharp;
using System.Text;

/*
 CRITICAL INFO: 
 IN ORDER TO USE CSharpCodeProvider Class FOR UNITY
 GO INTO PROJECT SETTINGS > PLAYER > OTHER SETTINGS > API COMPATIBILITY AND SET IT TO .NET 4.X
 SOURCE: https://answers.unity.com/questions/1585741/the-type-or-namespace-name-ilgenerator-could-not-b.html
 */


[Serializable]
public class Var
{
    public string name;
    public ParameterData data; //This will be primarily for initial values, I don't think I should worry about this now
    public string input;
    public string strType;
    public string asmPath;
    //public enum VarType { Field, Property };
    //public VarType varType;

    //To be set up during runtime
    public object obj; //For safety this should probably be the containing object of the property/field
    public Type type; //Type of class         

    public Var() { }

    public Var(object o, Type t)
    {
        obj = o;
        type = t;        
    }
    
    public Var(Type t, string theName)
    {
        obj = null;
        name = theName;
        type = t;
        strType = type.ToString();
        asmPath = type.Assembly.Location;
    }
}

public class CodeNode
{
    Func<object, object[], object> function;
    CodeNode nextPath;
    CodeNode falsePath;
    string returnVarName;
}

public class InterpreterData
{
    public Type[] types;
    public MethodInfo[] methods;
    public PropertyInfo[] properties;
    public FieldInfo[] fields;
    public Assembly[] asms;
    public ConstructorInfo[] constructors;

    //Operator Specifics
    public string operatorStr;
    public bool isOperator; //If using a valid operator
    public bool isAssign;
    public bool isIncrementOrDecrement;
    public bool isBaseClass = false;

    public bool returnBool;
    public bool isKeyWord;
    public bool isStatic; //If class name is called directly and var is not used    
    public string varName; //The target
    public Var varRef; //pretty sure this isn't being used

    public string input;
    public Type selectedType;
    public Assembly selectedAsm;
    public AccessType access; 
    
    public enum AccessType { Get, Set, Both };
    
    /*
     Should there be some kind of InterpreterParameter struct which are args afterwards
     
     I think there needs to be
     baseType of the object, which in this case is just selectedType
     but then we need an index in a split string array to know which possible function/field/property to evaluate

    This probably isn't the wisest to tackle now
    */
    public InterpreterData() { }

    public InterpreterData(Type[] t, MethodInfo[] m, PropertyInfo[] p, FieldInfo[] f, Assembly[] a)
    {
        types = t;
        methods = m;
        properties = p;
        fields = f;
        asms = a;
    }

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

    public bool UseGameCompile = false;
    public bool UseUnlocks = false;


    //public Dictionary<string, AppDomain> appDomains = new Dictionary<string, AppDomain>();
    List<Node> referenceNodes;
    List<Parameter> refParams;

    Node GetNode(int id)
    {
        if (referenceNodes != null)
        {
            foreach (Node node in referenceNodes)
                if (node.ID == id)
                    return node;
        }

        return null;
    }
   
    void PrintParameters()
    {
        string final = "";

        if (refParams != null)
        {
            foreach(Parameter p in refParams)
            {
                final += $"node input: {p.nodeRef.input} name: {p.name} arg: {p.arg} inputVar: {p.inputVar} varInput {p.varInput} \n";
            }

            Debug.Log(final);
        }
    }

    void PrintNodeParameters(Node node)
    {
        string final = "";

        if (node != null)
        {
            foreach(Parameter p in node.paramList)
            {
                final += $"name: {p.name} arg: {p.arg} inputVar: {p.inputVar} varInput: {p.varInput} \n";
            }

            Debug.Log($"Node {node.input} ID: {node.ID} parameters: \n" + final);
        }

    }

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

#if UNITY_EDITOR
    
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

    public BlueprintData LoadBlueprintAssets(string name)
    {
        BlueprintData asset = AssetDatabase.LoadAssetAtPath<BlueprintData>("Assets/" + name + ".asset");
        return asset;          
    }

#endif

    public BlueprintData CreateBlueprint(string name)
    {
        BlueprintData bp = ScriptableObject.CreateInstance<BlueprintData>();        
        //FileStream file = File.Create(Application.persistentDataPath + "/" + name + ".asset");
        return bp;
    }

    public void SaveBlueprint(BlueprintData data)
    {
        //BinaryFormatter bf = new BinaryFormatter();
        //string json = JsonUtility.ToJson(data);
        //FileStream file = File.Create(Application.persistentDataPath + "/" + data.ComponentName + ".asset");
        ////FileStream file = File.Open(Application.persistentDataPath + "/" + data.ComponentName + ".asset", FileMode.Open);
        //bf.Serialize(file, json);        
        //file.Close();
        BlueprintFile bp = new BlueprintFile();

        bp.ComponentName = data.ComponentName;
        
        bp.nodes = data.nodes;
        bp.entryPoints = data.entryPoints;
        bp.variables = data.variables;
        bp.passInParams = data.passInParams;
        bp.ID_Count = data.ID_Count;    
        bp.connections = data.connections;

        bp.compiledClassType = data.compiledClassType;
        bp.compiledClassTypeAsmPath = data.compiledClassTypeAsmPath;

        string json = JsonUtility.ToJson(bp);
        System.IO.File.WriteAllText(Application.persistentDataPath + "/" + $"{data.ComponentName}.asset", json);
    }

    public BlueprintData LoadBlueprint(string name)
    {
        //BinaryFormatter bf = new BinaryFormatter();
        //FileStream file = File.Open(Application.persistentDataPath + "/" + name + ".asset", FileMode.Open);
        //string json = (string)bf.Deserialize(file);
        //file.Close();

        //BlueprintFile bp = JsonUtility.FromJson<BlueprintFile>(json);
        //
        BlueprintData data = ScriptableObject.CreateInstance<BlueprintData>();
        string json = "";

        try
        {
            json = System.IO.File.ReadAllText(Application.persistentDataPath + "/" + $"{name}.asset");
        }

        catch
        {
            return null;
        }
        
        BlueprintFile bp = JsonUtility.FromJson<BlueprintFile>(json);
                
        data.ComponentName = bp.ComponentName;
        data.entryPoints = bp.entryPoints;
        data.nodes = bp.nodes;
        data.variables = bp.variables;
        data.passInParams = bp.passInParams;
        data.ID_Count = bp.ID_Count;
        data.connections = bp.connections; //SERIOUS CONCERN WITH THIS

        return data;
    }
    
    //This is only for processing code nodes, defining variables should be handled by CreateVariable
    public void Compile(string input, BlueprintData blueprint, ref InterpreterData data, Node node = null)
    {
        if (input == null)
            return;

        string[] args = input.Split(' ');

        if (args.Length < 1)
            return;

        Type varType = null;

        if (data != null)
        {
            if (input != data.input)
            {
                data = null;
                Debug.Log($"Resetting interpreter data input is {input}");                
            }
        }

        var gameFlags = BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.Public | BindingFlags.Static;
        var gameFlagsInstance = BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.Public;
        var gameFlagsStatic = BindingFlags.DeclaredOnly | BindingFlags.Public | BindingFlags.Static;

        if (data == null)
        {
            data = new InterpreterData();
            data.input = input;

            if (node.isContextual)
            {
                if (node.prevNode != null)
                {
                    if (node.prevNode.nodeType == NodeType.Field_Get || node.prevNode.nodeType == NodeType.Property_Get || node.prevNode.isReturning)
                    {
                        data.selectedType = node.prevNode.returnType;
                        data.selectedAsm = data.selectedType.Assembly;
                    }
                }
            }

            else
            {
                if (ParseKeywords(args[0], ref node, ref blueprint))
                {
                    return;
                }

                //CHECKING WHETHER A TYPE IS PART OF THE GAME NAMESPACE IS HANDLED IN FINDTYPE
             
                //Check for inherited members

                FieldInfo field;
                PropertyInfo prop;
                MethodInfo[] methodInfos;

                if (UseGameCompile)
                {
                    field = typeof(GameComponent).GetField(input, gameFlags);
                    prop = typeof(GameComponent).GetProperty(input, gameFlags);
                    methodInfos = typeof(GameComponent).GetMethods(gameFlags);

                    if (UseUnlocks)
                    {
                        //if (field != null)
                        //    if (field.GetCustomAttribute<UnlockStatus>() != null)
                        //        if (!field.GetCustomAttribute<UnlockStatus>().unlocked)
                        //            field = null;

                        if (field != null)
                            if (!GameUnlockManager.Instance.CheckUnlock(field))
                                field = null;

                        //if (prop != null)
                        //    if (prop.GetCustomAttribute<UnlockStatus>() != null)
                        //        if (!prop.GetCustomAttribute<UnlockStatus>().unlocked)
                        //            prop = null;

                        if (prop != null)
                            if (!GameUnlockManager.Instance.CheckUnlock(prop))
                                prop = null;
                    }
                }

                else
                {
                    field = typeof(MonoBehaviour).GetField(input);
                    prop = typeof(MonoBehaviour).GetProperty(input);
                    methodInfos = typeof(MonoBehaviour).GetMethods();
                }
                
                List<MethodInfo> baseMethods = new List<MethodInfo>();

                foreach(MethodInfo m in methodInfos)
                {
                    if (m.Name == input)
                    {
                        if (UseUnlocks && UseGameCompile)
                        {
                           //if (m.GetCustomAttribute<UnlockStatus>() != null)
                           //    if (m.GetCustomAttribute<UnlockStatus>().unlocked)                                  
                           //        baseMethods.Add(m);                                
                           if (GameUnlockManager.Instance.CheckUnlock(m))
                               baseMethods.Add(m);                            
                        }
                        else
                            baseMethods.Add(m);
                    }
                }
                //May want to re-work how a base class is selected
                if (field != null || prop != null || baseMethods.Count > 0)
                {
                    if (!UseGameCompile)
                        data.selectedType = typeof(MonoBehaviour);
                    else
                        data.selectedType = typeof(GameComponent);

                    data.fields = new FieldInfo[] { field };
                    data.properties = new PropertyInfo[] { prop };
                    data.methods = baseMethods.ToArray();
                    data.isBaseClass = true;
                    return;
                }

                //Check for variables - again not a dictionary right now since I don't where it would be stored        
                if (blueprint.variables != null)
                {
                    foreach (Var v in blueprint.variables)
                    {
                        if (v.name == args[0])
                        {
                            //Methods, Fields, and Properties are found below
                            varType = v.type;
                            data.selectedType = varType;
                            data.isStatic = false;
                            data.varName = args[0];
                            data.varRef = v;
                            break;
                        }
                    }
                }

                //Check for Pass In Params in my functions - probably ought to add a scope check
                if (blueprint.passInParams != null)
                {
                    foreach(Var v in blueprint.passInParams)
                    {
                        if (v.name == args[0])
                        {
                            varType = v.type;
                            data.selectedType = varType;
                            data.isStatic = false;
                            data.varName = args[0];
                            data.varRef = v;
                            break;
                        }
                    }
                }
                
                //If no variable is found, find the type if it exists
                if (varType == null)
                {
                    Type[] types;
                    Assembly[] asms;

                    //If multiple definitions found
                    if (FindType(args[0], out types, out asms))
                    {
                        data.types = types;
                        data.asms = asms;
                        data.isStatic = true;                        

                        if (data.types.Length > 1)
                            return;

                        if (data.types.Length == 1)
                        {
                            data.selectedType = data.types[0];
                            data.selectedAsm = data.selectedType.Assembly;
                            node.isStatic = true;
                        }
                    }
                }
            }
        }
        

        //If type selected
        //TypeSelected:
        
        //if (UseGameCompile)
        //{
        //    //int result = ParseGameFunctions(args, ref node, ref data, ref blueprint);
        //    //
        //    //if (result == 1) //Not Allowed
        //    //    return;
        //    //
        //    //if (result == 2) //Get Access only
        //    //    getAccessOnly = true;
        //}


        if (data.selectedType != null && !data.isBaseClass)
        {            
            string name = "";

            if (node != null && node.isContextual)
                name = args[0];
            else
                name = args[1];

            if (UseGameCompile && data.selectedType.Namespace == "Game" && data.selectedType.BaseType == typeof(Component))
            {
                Debug.Log("Skipping constructors");
                goto SkipConstructors;
            }

            ConstructorInfo[] constructors = data.selectedType.GetConstructors();
            
            if (args.Length > 1)
            {
                if (args[0] == args[1])
                {
                    if (UseGameCompile && UseUnlocks)
                    {
                        List<ConstructorInfo> infos = new List<ConstructorInfo>();

                        foreach (ConstructorInfo info in constructors)
                        {
                            //if (info.GetCustomAttribute<UnlockStatus>() != null)
                            //    if (info.GetCustomAttribute<UnlockStatus>().unlocked)
                            //        infos.Add(info);                                
                            if (GameUnlockManager.Instance.CheckUnlock(info))
                                infos.Add(info);
                        }

                        data.constructors = infos.ToArray();
                    }

                    else
                        data.constructors = constructors;
                }
            }

            SkipConstructors:

            MethodInfo[] methods;

            if (UseGameCompile)
                methods = data.selectedType.GetMethods(gameFlags);
            else
                methods = data.selectedType.GetMethods();
            List<MethodInfo> methodInfo = new List<MethodInfo>();

            foreach (MethodInfo m in methods)
            {
                 if (data.isStatic)
                 {
                    if (m.Name == name && m.IsStatic)
                    {
                        if (UseGameCompile && UseUnlocks)
                        {
                            //if (m.GetCustomAttribute<UnlockStatus>() != null)
                            //    if (m.GetCustomAttribute<UnlockStatus>().unlocked)
                            //        methodInfo.Add(m);                            
                                if (GameUnlockManager.Instance.CheckUnlock(m))
                                    methodInfo.Add(m);
                        }

                        else
                            methodInfo.Add(m);  
                    }
                 }
                 else
                 {
                    if (m.Name == name && !m.IsStatic)
                    {
                        if (UseGameCompile && UseUnlocks)
                        {
                            //if (m.GetCustomAttribute<UnlockStatus>() != null)
                            //    if (m.GetCustomAttribute<UnlockStatus>().unlocked)
                            //        methodInfo.Add(m);
                            
                                if (GameUnlockManager.Instance.CheckUnlock(m))
                                    methodInfo.Add(m);
                        }

                        else
                            methodInfo.Add(m);
                    }
                 }                              
            }

            data.methods = methodInfo.ToArray();

            //Enforce calls only on variables
            if (node != null)
                if (varType != null || node.isContextual)
                    ParseOperators(ref data, node, args);

            if (data.isStatic)
            {
                if (name == "methodDebug")
                {
                    data.methods = data.selectedType.GetMethods();
                }

                if (name == "propDebug")
                {
                    data.properties = data.selectedType.GetProperties();
                    return;
                }

                if (name == "fieldDebug")
                {
                    data.fields = data.selectedType.GetFields();
                    return;
                }

                data.fields = new FieldInfo[1];
                FieldInfo result;

                if (UseGameCompile)
                {
                    result = data.selectedType.GetField(name, gameFlagsStatic);

                    if (UseUnlocks)
                    {
                        //if (result != null)
                        //    if (result.GetCustomAttribute<UnlockStatus>() != null)
                        //        if (!result.GetCustomAttribute<UnlockStatus>().unlocked)
                        //            result = null;
                        if (result != null)
                            if (!GameUnlockManager.Instance.CheckUnlock(result))
                                result = null;
                    }

                }
                else
                    result = data.selectedType.GetField(name);

                if (result != null && result.IsPublic)
                {
                    data.fields[0] = result;
                    data.access = InterpreterData.AccessType.Both;
                }

                data.properties = new PropertyInfo[1];
                PropertyInfo prop;
                if (UseGameCompile)
                {
                    prop = data.selectedType.GetProperty(name, gameFlagsStatic);

                    if (UseUnlocks)
                    {
                        //if (prop != null)
                        //    if (prop.GetCustomAttribute<UnlockStatus>() != null)
                        //        if (!prop.GetCustomAttribute<UnlockStatus>().unlocked)
                        //            prop = null;

                        if (prop != null)
                            if (!GameUnlockManager.Instance.CheckUnlock(prop))
                                prop = null;
                    }
                }
                else
                    prop = data.selectedType.GetProperty(name);

                if (prop != null && (prop.CanRead || prop.CanWrite))
                {
                    if (prop.CanRead && prop.CanWrite)
                        data.access = InterpreterData.AccessType.Both;

                    else if (prop.CanRead)
                        data.access = InterpreterData.AccessType.Get;

                    else if (prop.CanWrite)
                        data.access = InterpreterData.AccessType.Set;

                    data.properties[0] = prop;
                }
            }

            else
            {
                data.fields = new FieldInfo[1];
                FieldInfo result;

                if (UseGameCompile)
                {
                    result = data.selectedType.GetField(name, gameFlagsInstance);

                    if (UseUnlocks)
                    {
                        //if (result != null)
                        //    if (result.GetCustomAttribute<UnlockStatus>() != null)
                        //        if (!result.GetCustomAttribute<UnlockStatus>().unlocked)
                        //            result = null;

                        if (result != null)
                            if (!GameUnlockManager.Instance.CheckUnlock(result))
                                result = null;
                    }
                }
                else
                    result = data.selectedType.GetField(name);

                if (result != null && result.IsPublic)
                {
                    data.fields[0] = result;
                    data.access = InterpreterData.AccessType.Both;
                }

                data.properties = new PropertyInfo[1];

                PropertyInfo prop;

                if (UseGameCompile)
                {
                    prop = data.selectedType.GetProperty(name, gameFlagsInstance);

                    if (UseUnlocks)
                    {
                        //if (prop != null)
                        //    if (prop.GetCustomAttribute<UnlockStatus>() != null)
                        //        if (!prop.GetCustomAttribute<UnlockStatus>().unlocked)
                        //            prop = null;

                        if (prop != null)
                            if (!GameUnlockManager.Instance.CheckUnlock(prop))
                                prop = null;
                    }
                }
                else
                    prop = data.selectedType.GetProperty(name);

                if (prop != null && (prop.CanRead || prop.CanWrite))
                {
                    if (varType == null)
                        data.isStatic = true;

                    if (prop.CanRead && prop.CanWrite)
                        data.access = InterpreterData.AccessType.Both;

                    else if (prop.CanRead)
                        data.access = InterpreterData.AccessType.Get;

                    else if (prop.CanWrite)
                        data.access = InterpreterData.AccessType.Set;

                    data.properties[0] = prop;
                }
            }

            //if (getAccessOnly)
            //    data.access = InterpreterData.AccessType.Get;
        }                
    }

    public void ParseOperators(ref InterpreterData data, Node node, string[] args)
    {
        string arg = "";

        if (node.isContextual)
            arg = args[0];
        else
            arg = args[1];

        MethodInfo[] GetMethodMatches(Type type, string name)
        {
            MethodInfo[] infos = type.GetMethods();
            List<MethodInfo> methods = new List<MethodInfo>();

            foreach (MethodInfo m in infos)
                if (name == m.Name)
                    methods.Add(m);

            return methods.ToArray();
        }

        data.operatorStr = arg;

        switch(arg)
        {
            //Assignment
            case "=":
                {                    
                    data.methods = GetMethodMatches(data.selectedType, "op_Assign");
                    data.isOperator = true;
                    data.isAssign = true;
                }
                break;

            //case "+=":                
            //    data.methods = GetMethodMatches(data.selectedType, "op_AdditionAssignment");
            //    if (data.selectedType.IsPrimitive || data.methods.Length > 0)
            //    {
            //        data.isOperator = true;
            //        data.isAssign = true;
            //    }
            //    break;
            //
            //case "-=":
            //    data.methods = GetMethodMatches(data.selectedType, "op_SubtractionAssignment");
            //    if (data.selectedType.IsPrimitive || data.methods.Length > 0)
            //    {
            //        data.isOperator = true;
            //        data.isAssign = true;
            //    }
            //    break;
            //
            //case "*=":
            //    data.methods = GetMethodMatches(data.selectedType, "op_MultiplicationAssignment");
            //    if (data.selectedType.IsPrimitive || data.methods.Length > 0)
            //    {
            //        data.isOperator = true;
            //        data.isAssign = true;
            //    }
            //    break;
            //
            //case "/=":
            //    data.methods = GetMethodMatches(data.selectedType, "op_DivisionAssignment");
            //    if (data.selectedType.IsPrimitive || data.methods.Length > 0)
            //    {
            //        data.isOperator = true;
            //        data.isAssign = true;
            //    }
            //    break;
            //
            //case "%=":
            //    data.methods = GetMethodMatches(data.selectedType, "op_ModulusAssignment");
            //    if (data.selectedType.IsPrimitive || data.methods.Length > 0)
            //    {
            //        data.isOperator = true;
            //        data.isAssign = true;
            //    }
            //    break;

            case "++":
                data.methods = GetMethodMatches(data.selectedType, "op_Increment");
                if (data.selectedType.IsPrimitive || data.methods.Length > 0)
                {
                    data.isOperator = true;
                    data.isIncrementOrDecrement = true;
                }
                break;

            case "--":
                data.methods = GetMethodMatches(data.selectedType, "op_Decrement");
                if (data.selectedType.IsPrimitive || data.methods.Length > 0)
                {
                    data.isOperator = true;
                    data.isIncrementOrDecrement = true;
                }
                break;
           
            //Standard Operations
            case "+":
                data.methods = GetMethodMatches(data.selectedType, "op_Addition");
                if (data.selectedType.IsPrimitive || data.methods.Length > 0)
                    data.isOperator = true;
                break;

            case "-":
                data.methods = GetMethodMatches(data.selectedType, "op_Subtraction");
                if (data.selectedType.IsPrimitive || data.methods.Length > 0)
                    data.isOperator = true;
                break;

            case "*":
                data.methods = GetMethodMatches(data.selectedType, "op_Multiply");
                if (data.selectedType.IsPrimitive || data.methods.Length > 0)
                    data.isOperator = true;
                break;

            case "/":
                data.methods = GetMethodMatches(data.selectedType, "op_Division");
                if (data.selectedType.IsPrimitive || data.methods.Length > 0)
                    data.isOperator = true;
                break;

            case "%":
                data.methods = GetMethodMatches(data.selectedType, "op_Modulus");
                if (data.selectedType.IsPrimitive || data.methods.Length > 0)
                    data.isOperator = true;
                break;

            //Bitwise Operations
            //case "&":
            //    data.methods = GetMethodMatches(data.selectedType, "op_BitwiseAnd");
            //    if (data.selectedType.IsPrimitive || data.methods.Length > 0)
            //        data.isOperator = true;
            //    break;
            //
            //case "|":
            //    data.methods = GetMethodMatches(data.selectedType, "op_BitwiseOr");
            //    if (data.selectedType.IsPrimitive || data.methods.Length > 0)
            //        data.isOperator = true;
            //    break;
            //
            //case "^":
            //    data.methods = GetMethodMatches(data.selectedType, "op_ExclusiveOr");
            //    if (data.selectedType.IsPrimitive || data.methods.Length > 0)
            //        data.isOperator = true;
            //    break;
            //
            //case "~":
            //    data.methods = GetMethodMatches(data.selectedType, "op_OnesComplement");
            //    if (data.selectedType.IsPrimitive || data.methods.Length > 0)
            //        data.isOperator = true;
            //    break;
            //
            //case "<<":
            //    data.methods = GetMethodMatches(data.selectedType, "op_LeftShift");
            //    if (data.selectedType.IsPrimitive || data.methods.Length > 0)
            //        data.isOperator = true;
            //    break;
            //
            //case ">>":
            //    data.methods = GetMethodMatches(data.selectedType, "op_RightShift");
            //    if (data.selectedType.IsPrimitive || data.methods.Length > 0)
            //        data.isOperator = true;
            //    break;
            
            //Conditionals
            case "==":
                data.methods = GetMethodMatches(data.selectedType, "op_Equality");                
                data.isOperator = true;
                data.returnBool = true;
                break;

            case "<":
                data.methods = GetMethodMatches(data.selectedType, "op_LessThan");
                if (data.selectedType.IsPrimitive || data.methods.Length > 0)
                {
                    data.isOperator = true;
                    data.returnBool = true;
                }
                break;

            case ">":
                data.methods = GetMethodMatches(data.selectedType, "op_GreaterThan");
                if (data.selectedType.IsPrimitive || data.methods.Length > 0)
                {
                    data.isOperator = true;
                    data.returnBool = true;
                }
                break;

            case "<=":
                data.methods = GetMethodMatches(data.selectedType, "op_LessThanOrEqual");
                if (data.selectedType.IsPrimitive || data.methods.Length > 0)
                {
                    data.isOperator = true;
                    data.returnBool = true;
                }
                break;

            case ">=":
                data.methods = GetMethodMatches(data.selectedType, "op_GreaterThanOrEqual");
                if (data.selectedType.IsPrimitive || data.methods.Length > 0)
                {
                    data.isOperator = true;
                    data.returnBool = true;
                }
                break;

            case "!=":
                data.methods = GetMethodMatches(data.selectedType, "op_Inequality");                
                data.isOperator = true;
                data.returnBool = true;
                break;

            case "&&":
                data.methods = GetMethodMatches(data.selectedType, "op_LogicalAnd");
                if (data.selectedType == typeof(bool) || data.methods.Length > 0)
                {
                    data.isOperator = true;
                    data.returnBool = true;
                }
                break;

            case "||":
                data.methods = GetMethodMatches(data.selectedType, "op_LogicalOr");
                if (data.selectedType == typeof(bool) || data.methods.Length > 0)
                {
                    data.isOperator = true;
                    data.returnBool = true;
                }
                break;
        }
    }

    //DEPRECATED
    public void CompileNode(Node node, object target = null, Blueprint blueprint = null)
    {        
        //This might need to be changed later in the event of unity messages with params such as OnCollisionEnter
        if (node.isEntryPoint)
        {            
            return;
        }

        if (blueprint != null)
            node.blueprint = blueprint;

        if (node.nodeType == NodeType.Function)
        {            
            if (node.currentMethod == null)
            {               
                node.currentMethod = LoadMethod(node.input, node.type, node.assemblyPath, node.index, node.isContextual);                                
            }

            if (node.constructorMethod == null && node.currentMethod == null)
            {
                node.constructorMethod = LoadConstructor(node.input, node.type, node.assemblyPath, node.index, node.isContextual);
            }

            if (node.currentMethod == null && node.constructorMethod == null)
            {
                Debug.Log("Node has no method or is entry point");
                return;
            }

            if (node.paramList.Count > 0)
            {
                node.passArgs = new object[node.paramList.Count];

                for (int i = 0; i < node.paramList.Count; i++)
                {
                    //I don't remember why I added this, but there must have been a good reason I hope
                    if (node.isSpecial && node.paramList[i].noType)
                    {
                        //node.passArgs[i] = node.actualTarget;
                        //node.actualTarget = null;
                        continue;
                    }

                    node.passArgs[i] = node.paramList[i].arg;
                }
            }
            
            if (target != null)
            {
                //Debug.Log("passed in target is not null");
                //node.actualTarget = target;
            }

            if (node.currentMethod != null)
            {
                //if (node.isReturning && node.isStatic)
                //    //Instance.expressions.Add(NonVoidStaticMethodExpression(node.currentMethod));
                //    //node.expressionBody.Add(NonVoidStaticMethodExpression(node.currentMethod));
                //
                //else if (node.isReturning && !node.isStatic)
                //    //Instance.expressions.Add(NonVoidInstanceMethodExpression(node.currentMethod));
                //    //node.expressionBody.Add(NonVoidInstanceMethodExpression(node.currentMethod));
                //
                //else if (!node.isReturning && node.isStatic)
                //    //Instance.expressions.Add(VoidStaticMethodExpression(node.currentMethod));
                //    //node.expressionBody.Add(VoidStaticMethodExpression(node.currentMethod));
                //
                //else if (!node.isReturning && !node.isStatic)
                //    //Instance.expressions.Add(VoidInstanceMethodExpression(node.currentMethod));
                //    //node.expressionBody.Add(VoidInstanceMethodExpression(node.currentMethod));

                node.function = node.currentMethod.Bind();
            }

            if (node.constructorMethod != null)
            {                
                node.function = CreateConstructor(node.constructorMethod);
            }

            return;
        }

        if (node.nodeType == NodeType.Field_Get)
        {
            node.fieldVar = LoadField(node.input, node.type, node.assemblyPath, node.isContextual);
            node.function = CreateGetFunction(node.fieldVar);
        }

        if (node.nodeType == NodeType.Field_Set)
        {
            //Find the base type
            node.fieldVar = LoadField(node.input, node.type, node.assemblyPath, node.isContextual);
            node.function = CreateSetFunction(node.fieldVar);
            
            //if ((string)node.varField.arg != "")
            //{
            //      node.isVar = true;
            //}                                  
        }

        if (node.nodeType == NodeType.Property_Get)
        {
            node.propertyVar = LoadProperty(node.input, node.type, node.assemblyPath, node.isContextual);
            node.function = CreateGetFunction(node.propertyVar);
        }

        if (node.nodeType == NodeType.Property_Set)
        {
            node.propertyVar = LoadProperty(node.input, node.type, node.assemblyPath);
            node.function = CreateSetFunction(node.propertyVar);
          
            //if ((string)node.varField.arg != "")            
            //    node.isVar = true;
            
        }
    }

    public MethodInfo LoadMethod(string input, string type, string path, int index, bool isContextual = false)
    {
        //Debug.Log($"location test {path}");
        if (path == "")
            return null;

        Assembly TestASM = Assembly.LoadFile(path);

        string[] args = input.Split(' ');
        string name = "";

        //if (isContextual && args.Length == 1)
        if (args.Length == 1)
            name = args[0];

        //else if (args.Length > 1 && !isContextual)
        else if (args.Length > 1)
            name = args[1];
        //else
        //{
        //    Debug.Log("No function name found returning null");
        //    return null;
        //}

        if (TestASM != null)
        {
            Type typeTest = TestASM.GetType(type);            
            MethodInfo[] allMethods = typeTest.GetMethods();
            List<MethodInfo> methods = new List<MethodInfo>();
           
            MethodInfo final = null;

            foreach (MethodInfo m in allMethods)
            {
                if (m.Name == name)
                    methods.Add(m);
            }

            if (methods != null && index < methods.Count)
            {
                final = methods[index];
                return final;
            }            
        }

        return null;
    }

    //I'm not proud of this
    public ConstructorInfo LoadConstructor(string input, string type, string path, int index, bool isContextual = false)
    {        
        if (path == "")
            return null;

        Assembly TestASM = Assembly.LoadFile(path);

        string[] args = input.Split(' ');
        string name = "";
      
        if (args.Length > 1 && !isContextual)
            name = args[1];
        else
        {
            Debug.Log("No function name found returning null");
            return null;
        }

        if (TestASM != null)
        {
            Type typeTest = TestASM.GetType(type);
            ConstructorInfo[] allMethods = typeTest.GetConstructors();            
           
            if (allMethods != null && index < allMethods.Length)
                return allMethods[index];

        }

        return null;
    }

    public Type LoadVarType(string type, string asmPath)
    {
        if (asmPath == "" || asmPath == null)
            return null;

        Assembly TestASM = Assembly.LoadFile(asmPath);
        
        if (TestASM != null)
        {
            return TestASM.GetType(type);
        }

        return null;
    }

    public FieldInfo LoadField(string input, string type, string path, bool isContextual = false)
    {
        if (path == "")
            return null;

        Assembly TestASM = Assembly.LoadFile(path);

        string[] args = input.Split(' ');
        string name = "";

        if (args.Length > 1)
            name = args[1];

        else if (args.Length == 1 && isContextual)
            name = args[0];

        else
        {
            Debug.Log("No field name found returning null");
            return null;
        }

        if (TestASM != null)
        {
            Type typeTest = TestASM.GetType(type);
            if (typeTest != null)
            {
                FieldInfo info = typeTest.GetField(name);

                if (info != null)
                    return info;
            }
        }

        return null;
    }

    public PropertyInfo LoadProperty(string input, string type, string path, bool isContextual = false)
    {
        if (path == "")
            return null;

        Assembly TestASM = Assembly.LoadFile(path);

        string[] args = input.Split(' ');
        string name = "";

        if (args.Length > 1)
            name = args[1];

        else if (args.Length == 1 && isContextual)
        {
            name = args[0];
        }
        else
        {
            Debug.Log("No field name found returning null");
            return null;
        }

        if (TestASM != null)
        {
            Type typeTest = TestASM.GetType(type);
            if (typeTest != null)
            {
                PropertyInfo info = typeTest.GetProperty(name);

                if (info != null)
                    return info;
            }
        }

        return null;
    }   
    public int ParseGameFunctions(string[] args, ref Node node, ref InterpreterData data, ref BlueprintData bp)
    {
        /*
         Status Codes are as follows: 
         0: No issues
         1: Not Allowed or not found
         2: Get Access only
         */

        Type type = typeof(GameFramework);        
        MethodInfo[] methods = type.GetMethods();
        List<MethodInfo> methodInfo = new List<MethodInfo>();
        
        foreach (MethodInfo m in methods)
        {
            if (m.Name == args[0] && m.IsStatic)
                methodInfo.Add(m);
        }
        
        data.methods = methodInfo.ToArray();
        
        if (data.methods != null && data.methods.Length > 0)
        {
            data.selectedType = typeof(GameFramework);
            data.selectedAsm = data.selectedType.Assembly;
            node.hasCost = true;
            data.isStatic = true;
            return 0;
        }
        
        GameCodebase code = GameManager.Instance.codebase;
        
        //Determine whether this node is contextual or using a variable
        bool isVar = false;
        Type varType = null;
        
        foreach(Var v in bp.variables)
        {
            if (v.name == args[0])
            {
                isVar = true;
                varType = v.type;
                //data.selectedType = v.type;
                //data.selectedAsm = v.type.Assembly;                
            }
        }

        //Eject if operation

        //if (args[0] == "=")
        //    return 0;      
        //if (args[0] == "++")
        //    return 0;      
        //if (args[0] == "--")
        //    return 0;      
        //if (args[0] == "+")
        //    return 0;      
        //if (args[0] == "-")
        //    return 0;      
        //if (args[0] == "*")
        //    return 0;      
        //if (args[0] == "/")
        //    return 0;      
        //if (args[0] == "%")
        //    return 0;      
        //if (args[0] == "==")
        //    return 0;      
        //if (args[0] == "<" )
        //    return 0;      
        //if (args[0] == ">" )
        //    return 0;      
        //if (args[0] == "<=")
        //    return 0;      
        //if (args[0] == ">=")
        //    return 0;      
        //if (args[0] == "!=")
        //    return 0;      
        //if (args[0] == "&&")
        //    return 0;      
        //if (args[0] == "||")
        //    return 0;

        if (args.Length > 1)
        {
            if (args[1] == "=")
                return 0;
            if (args[1] == "++")
                return 0;
            if (args[1] == "--")
                return 0;
            if (args[1] == "+")
                return 0;
            if (args[1] == "-")
                return 0;
            if (args[1] == "*")
                return 0;
            if (args[1] == "/")
                return 0;
            if (args[1] == "%")
                return 0;
            if (args[1] == "==")
                return 0;
            if (args[1] == "<")
                return 0;
            if (args[1] == ">")
                return 0;
            if (args[1] == "<=")
                return 0;
            if (args[1] == ">=")
                return 0;
            if (args[1] == "!=")
                return 0;
            if (args[1] == "&&")
                return 0;
            if (args[1] == "||")
                return 0;
        }


        foreach (GameClass typeName in code.classes)
        {
            if (isVar)
            {
                if (typeName.className == varType.Name)
                {                    
                    foreach (MemberData member in typeName.members)
                    {
                        if (member.name.Split(' ')[1] == args[1])
                        {                           
                            data.selectedType = varType;
                            data.selectedAsm = varType.Assembly;
                            Debug.Log("Member Found in Game for Variable");

                            if (member.memberType == MemberData.MemberType.Field || member.memberType == MemberData.MemberType.Property)
                            {
                                if (member.access == MemberData.AccessType.Get)
                                    return 2;
                                if (member.access == MemberData.AccessType.Both)
                                    return 0;
                            }

                            return 0;
                        }

                        if (member.name.Split('(')[0] == args[1])
                        {
                            data.selectedType = varType;
                            data.selectedAsm = varType.Assembly;
                            Debug.Log("Member Found in Game for Variable");
                            return 0;
                        }
                    }
                }
            }

            if (node.isContextual)
            {
                if (typeName.className == data.selectedType.Name)
                {                    
                    foreach (MemberData member in typeName.members)
                    {
                        if (member.access == MemberData.AccessType.Both)
                        {
                            if (args[0] == "=")
                                return 0;
                            if (args[0] == "++")
                                return 0;
                            if (args[0] == "--")
                                return 0;
                            if (args[0] == "+")
                                return 0;
                            if (args[0] == "-")
                                return 0;
                            if (args[0] == "*")
                                return 0;
                            if (args[0] == "/")
                                return 0;
                            if (args[0] == "%")
                                return 0;
                            if (args[0] == "==")
                                return 0;
                            if (args[0] == "<")
                                return 0;
                            if (args[0] == ">")
                                return 0;
                            if (args[0] == "<=")
                                return 0;
                            if (args[0] == ">=")
                                return 0;
                            if (args[0] == "!=")
                                return 0;
                            if (args[0] == "&&")
                                return 0;
                            if (args[0] == "||")
                                return 0;
                        }

                        if (member.name.Split(' ')[1] == args[0])
                        {
                            Debug.Log("Member found in Game for Contextual");

                            if (member.memberType == MemberData.MemberType.Field || member.memberType == MemberData.MemberType.Property)
                            {
                                if (member.access == MemberData.AccessType.Get)
                                    return 2;
                                if (member.access == MemberData.AccessType.Both)
                                    return 0;
                            }
                            
                            return 0;
                        }

                        if (member.name.Split('(')[0] == args[0])
                        {
                            Debug.Log("Member found in Game for Contextual");
                            return 0;
                        }
                    }

                    //It failed disable it
                    Debug.Log("Not found in Game Code Base");
                    data.selectedType = null;
                    data.selectedAsm = null;
                    return 1;
                }
            }

            //If static
            if (typeName.className == args[0])
            {
                foreach (MemberData member in typeName.members)
                {                    
                    if (member.name.Split(' ')[1] == args[1])
                    {
                        Type newType = FindType(args[0]);

                        if (newType != null)
                        {
                            data.selectedType = newType;
                            data.selectedAsm = newType.Assembly;
                            Debug.Log("Member found for Static in Game");
                            if (member.memberType == MemberData.MemberType.Field || member.memberType == MemberData.MemberType.Property)
                            {
                                if (member.access == MemberData.AccessType.Get)
                                    return 2;
                                if (member.access == MemberData.AccessType.Both)
                                    return 0;
                            }

                            return 0;
                        }

                        else
                            return 1;
                        
                    }

                    if (member.name.Split('(')[0] == args[1])
                    {
                        Type newType = FindType(args[0]);

                        if (newType != null)
                        {
                            data.selectedType = newType;
                            data.selectedAsm = newType.Assembly;
                            Debug.Log("Member found for Static in Game");
                            return 0;
                        }

                        else
                            return 1;
                    }
                }
            }
        }

        Debug.Log("No class found in Game");
        return 1;       
    }

    public bool ParseKeywords(string text, ref Node node, ref BlueprintData data)
    {
        MethodInfo info = null;
        PropertyInfo prop = null;
        FieldInfo field = null;

        //try
        //{
        //    info = typeof(MonoBehaviour).GetMethod(text);
        //    prop = typeof(MonoBehaviour).GetProperty(text);
        //    field = typeof(MonoBehaviour).GetField(text);
        //}
        //
        //catch
        //{
        //
        //}

        try
        {            
             info = typeof(BlueprintComponent).GetMethod(text);                        
        }

        catch
        {            
            //if (text == "GetComponent" || text == "AddComponent" || text == "Set")
            //{                
            //    info = typeof(BlueprintComponent).GetMethod(text, new Type[] { typeof(string), typeof(string) });
            //}            
        }
        
        if (info != null)
        {
            //Questionable if this is still needed here
            if (info.Name == "GetComponent" || info.Name == "AddComponent" || info.Name == "Set")
            {
                node.isSpecial = true;                
                node.ChangeToSpecialMethod(info);
                return true;
            }            
            
            Debug.Log($"Method found for {text}");

            //Check if it already exists - so it can be called           

            if (data.entryPoints == null)
                data.entryPoints = new List<NodeData>();

            foreach(NodeData nodeData in data.entryPoints)
            {
                if (nodeData.input == text && nodeData.isEntryPoint)
                {
                    Debug.Log("Message Function is defined");
                    //node.isSpecial = true;
                    //node.input = text;
                    //node.ChangeToSpecialMethod(info);                    
                    return false;
                }
            }

            ParameterInfo[] pars = info.GetParameters();
            
            if (node.passInParams == null)
            {
                Debug.Log("Allocating new passInParam List for node");
                node.passInParams = new List<string>();
            }

            if (data.passInParams == null)
            {
                Debug.Log("Allocating new passInParam List for data");
                data.passInParams = new List<Var>();
            }
            
            foreach (ParameterInfo p in pars)
            {
                string final = p.ParameterType + " " + p.Name;
                node.passInParams.Add(final);
                data.passInParams.Add(new Var(p.ParameterType, p.Name));
            }

            float initHeight = node.rect.height;

            if (node.passInParams.Count == 1)
                node.rect = new Rect(node.rect.x, node.rect.y, node.rect.width, initHeight * (node.passInParams.Count + 1));


            else if (node.passInParams.Count > 1)
                node.rect = new Rect(node.rect.x, node.rect.y, node.rect.width, initHeight * (node.passInParams.Count));

            node.isDefined = true;
            node.isEntryPoint = true;
            node.isVirtual = info.IsVirtual;

            if (node.isVirtual)
                Debug.Log("NODE IS CONFIRMED VIRTUAL");

            if (data.entryPoints == null)
                data.entryPoints = new List<NodeData>();

            data.entryPoints.Add(new NodeData(node));

            return true;
        }

        if (text == "if")
        {
            node.ChangeToConditional();
            node.isDefined = true;
            return true;
        }

        //Just so there doesn't have to be allocations over and over
        //Type[] argTypes = { typeof(Var), typeof(Var) };
        //
        ////Conditionals
        //switch(text)
        //{                    
        //    case "==":
        //        node.input = "Equals";
        //        node.isSpecial = true;
        //        //node.ChangeToMethod(typeof(HelperFunctions).GetMethod("Equals", argTypes));
        //        node.ChangeToSpecialMethod(typeof(BlueprintComponent).GetMethod("Equals", new Type[] { typeof(string), typeof(string) }));
        //        break;
        //
        //    case "<":
        //        node.input = "LessThan";
        //        node.isSpecial = true;
        //        //node.ChangeToMethod(typeof(HelperFunctions).GetMethod("LessThan", argTypes));
        //        node.ChangeToSpecialMethod(typeof(BlueprintComponent).GetMethod("LessThan", new Type[] { typeof(string), typeof(string) }));
        //        break;
        //
        //    case ">":
        //        node.input = "GreaterThan";
        //        node.isSpecial = true;
        //        //node.ChangeToMethod(typeof(HelperFunctions).GetMethod("GreaterThan", argTypes));
        //        node.ChangeToSpecialMethod(typeof(BlueprintComponent).GetMethod("GreaterThan", new Type[] { typeof(string), typeof(string) }));
        //        break;
        //
        //    case "<=":
        //        node.input = "LessThanOrEqual";
        //        node.isSpecial = true;
        //        //node.ChangeToMethod(typeof(HelperFunctions).GetMethod("LessThanOrEqual", argTypes));
        //        node.ChangeToSpecialMethod(typeof(BlueprintComponent).GetMethod("LessThanOrEqual", new Type[] { typeof(string), typeof(string) }));
        //        break;
        //
        //    case ">=":
        //        node.input = "GreaterThanOrEqual";
        //        node.isSpecial = true;
        //        //node.ChangeToMethod(typeof(HelperFunctions).GetMethod("GreaterThanOrEqual", argTypes));
        //        node.ChangeToSpecialMethod(typeof(BlueprintComponent).GetMethod("GreaterThanOrEqual", new Type[] { typeof(string), typeof(string) }));
        //        break;
        //
        //    case "!=":
        //        node.input = "NotEquals";
        //        node.isSpecial = true;
        //        //node.ChangeToMethod(typeof(HelperFunctions).GetMethod("NotEquals", argTypes));
        //        node.ChangeToSpecialMethod(typeof(BlueprintComponent).GetMethod("NotEquals", new Type[] { typeof(string), typeof(string) }));
        //        break;
        //
        //    case "&&":
        //        node.input = "And";
        //        node.isSpecial = true;
        //        //node.ChangeToMethod(typeof(HelperFunctions).GetMethod("And", argTypes));
        //        node.ChangeToSpecialMethod(typeof(BlueprintComponent).GetMethod("And", new Type[] { typeof(string), typeof(string) }));
        //        break;
        //
        //    case "||":
        //        node.input = "Or";
        //        node.isSpecial = true;
        //        //node.ChangeToMethod(typeof(HelperFunctions).GetMethod("Or", argTypes));
        //        node.ChangeToSpecialMethod(typeof(BlueprintComponent).GetMethod("Or", new Type[] { typeof(string), typeof(string) }));
        //        break;
        //}
        

        return false;       
    }

    public MethodInfo GetSpecialFunction(string input, bool hasCost, int index = 0)
    {
        MethodInfo info = null;

        if (hasCost)
        {
            //info = typeof(GameFramework).GetMethod(input);
            MethodInfo[] allMethods = typeof(GameFramework).GetMethods();
            List<MethodInfo> methods = new List<MethodInfo>();

            MethodInfo final = null;

            foreach (MethodInfo m in allMethods)
            {
                if (m.Name == input)
                    methods.Add(m);
            }

            if (methods != null && index < methods.Count)
            {
                final = methods[index];
                return final;
            }
        }

        else
            info = typeof(BlueprintComponent).GetMethod(input, new Type[] { typeof(string), typeof(string) });

        if (info != null)        
            return info;
        
        return null;
    }
    
    public Type FindType(string input)
    {
        foreach (Assembly asm in AppDomain.CurrentDomain.GetAssemblies())
        {
            Type[] types = asm.GetTypes();

            foreach (Type t in types)
            {
                if (t.Name == input)
                {
                    return t;
                }
            }
        }

        return null;
    }

    //Return true if multiple types are found
    public bool FindType(string input, out Type[] type, out Assembly[] ASM)
    {
        //Source: https://stackoverflow.com/questions/8499593/c-sharp-how-to-check-if-namespace-class-or-method-exists-in-c
        //type = (from assembly in AppDomain.CurrentDomain.GetAssemblies() from type2 in assembly.GetTypes() where type2.Name == raw[0] select type2).FirstOrDefault();

        //To account for multiple namespaces
        int matchCount = 0;
        List<Type> typeList = new List<Type>();
        List<Assembly> asms = new List<Assembly>();

        if (UseGameCompile)
        {
            //This should return the Assembly Assembly C-Sharp that the game framework is defined in
            Assembly assembly = this.GetType().Assembly;
            bool found = false;

            type = null;
            ASM = null;

            //Custom class searching
            if (Application.isPlaying)
            {
                Type t = GameManager.Instance.SearchClass(input);

                if (t != null)
                {
                    type = new Type[] { t };
                    ASM = new Assembly[] { t.Assembly };
                    return true;
                }
            }

            //Support primitive types + Unity standard types
            switch(input)
            {
                case "bool":
                    type = new Type[] { typeof(bool) };
                    ASM = new Assembly[] { type[0].Assembly };
                    found = true;
                    break;

                case "byte":
                    type = new Type[] { typeof(byte) };
                    ASM = new Assembly[] { type[0].Assembly };
                    found = true;
                    break;

                case "char":
                    type = new Type[] { typeof(char) };
                    ASM = new Assembly[] { type[0].Assembly };
                    found = true;
                    break;

                case "short":
                    type = new Type[] { typeof(short) };
                    ASM = new Assembly[] { type[0].Assembly };
                    found = true;
                    break;

                case "ushort":
                    type = new Type[] { typeof(ushort) };
                    ASM = new Assembly[] { type[0].Assembly };
                    found = true;
                    break;

                case "int":
                    type = new Type[] { typeof(int) };
                    ASM = new Assembly[] { type[0].Assembly };
                    found = true;
                    break;

                case "uint":
                    type = new Type[] { typeof(uint) };
                    ASM = new Assembly[] { type[0].Assembly };
                    found = true;
                    break;

                case "long":
                    type = new Type[] { typeof(long) };
                    ASM = new Assembly[] { type[0].Assembly };
                    found = true;
                    break;

                case "ulong":
                    type = new Type[] { typeof(ulong) };
                    ASM = new Assembly[] { type[0].Assembly };
                    found = true;
                    break;

                case "float":
                    type = new Type[] { typeof(float) };
                    ASM = new Assembly[] { type[0].Assembly };
                    found = true;
                    break;

                case "double":
                    type =  new Type[] { typeof(double) };
                    ASM = new Assembly[] { type[0].Assembly };
                    found = true;
                    break;

                case "string":
                    type = new Type[] { typeof(string) };
                    ASM = new Assembly[] { type[0].Assembly };
                    found = true;
                    break;

                case "Vector2":
                    type = new Type[] { typeof(Vector2) };
                    ASM = new Assembly[] { type[0].Assembly };
                    found = true;
                    break;

                case "Vector3":
                    type = new Type[] { typeof(Vector3) };
                    ASM = new Assembly[] { type[0].Assembly };
                    found = true;
                    break;

                case "Vector4":
                    type = new Type[] { typeof(Vector4) };
                    ASM = new Assembly[] { type[0].Assembly };
                    found = true;
                    break;

                case "Quaternion":
                    type = new Type[] { typeof(Quaternion) };
                    ASM = new Assembly[] { type[0].Assembly };
                    found = true;
                    break;

                case "Rect":
                    type = new Type[] { typeof(Rect) };
                    ASM = new Assembly[] { type[0].Assembly };
                    found = true;
                    break;

                case "Color":
                    type = new Type[] { typeof(Color) };
                    ASM = new Assembly[] { type[0].Assembly };
                    found = true;
                    break;

                case "Mathf":
                    type = new Type[] { typeof(Mathf) };
                    ASM = new Assembly[] { type[0].Assembly };
                    found = true;
                    break;

                case "Random":
                    type = new Type[] { typeof(UnityEngine.Random) };
                    ASM = new Assembly[] { type[0].Assembly };
                    found = true;
                    break;

                case "LayerMask":
                    type = new Type[] { typeof(UnityEngine.LayerMask) };
                    ASM = new Assembly[] { type[0].Assembly };
                    found = true;
                    break;
            }
            
            if (found)
                return true;

            foreach(Type t in assembly.GetTypes())
            {
                //Support references to other custom components?
                if (t.Name == input && (t.Namespace == "Game" || t.BaseType == typeof(GameComponent)))
                {
                    type = new Type[] { t };
                    ASM = new Assembly[] { t.Assembly };

                    if (UseUnlocks)
                    {
                        //UnlockStatus status = t.GetCustomAttribute<UnlockStatus>();
                        //
                        //if (status != null)
                        //    if (!status.unlocked)
                        //    {
                        //        type = null;
                        //        ASM = null;
                        //        return false;
                        //    }

                        if (!GameUnlockManager.Instance.CheckUnlock(t))
                        {
                            type = null;
                            ASM = null;
                            return false;
                        }
                    }

                    else
                        return true;
                }
            }

            if (!found)
            {
                type = null;
                ASM = null;
                return false;
            }
        }


        foreach (Assembly asm in AppDomain.CurrentDomain.GetAssemblies())
        {
            Type[] types = asm.GetTypes();

            foreach (Type t in types)
            {
                if (t.Name == input)
                {
                    typeList.Add(t);
                    asms.Add(asm);
                    matchCount++;
                }
            }
        }

        if (matchCount == 0)
        {
            type = null;
            ASM = null;
            return false;
        }

        else
        {
            type = typeList.ToArray();
            ASM = asms.ToArray();
            return true;
        }
    }

    public void CreateVariable(BlueprintData data, ref InterpreterData meta, string input)
    {
        //General Input Validation
        string[] raw = input.Split(' ');

        if (raw.Length <= 1)
        {            
            Debug.Log("Invalid, cannot create variable");
            return;
        }

        if (meta != null)
        {
            if (input != meta.input)
            {
                meta = null;
                Debug.Log($"Resetting interpreter data input is {input}");
            }
        }
        
        if (data.variables == null)
            data.variables = new List<Var>();

        foreach(Var v in data.variables)
        {
            if (v.name == raw[1])
            {
                Debug.Log("A variable with that name already exists");
                return;
            }
        }

        if (meta == null)
        {           
            meta = new InterpreterData();

            Type[] type;
            Assembly[] ASM;

            bool isPrimitive = false;

            //Primitive Check + string + Standard Unity Classes
            switch(raw[0])
            {
                case "bool":
                    meta.selectedType = typeof(bool);
                    isPrimitive = true;
                    break;

                case "byte":
                    meta.selectedType = typeof(byte);
                    isPrimitive = true;
                    break;

                case "char":
                    meta.selectedType = typeof(char);
                    isPrimitive = true;
                    break;

                case "short":
                    meta.selectedType = typeof(short);
                    isPrimitive = true;
                    break;

                case "ushort":
                    meta.selectedType = typeof(ushort);
                    isPrimitive = true;
                    break;

                case "int":
                    meta.selectedType = typeof(int);
                    isPrimitive = true;
                    break;

                case "uint":
                    meta.selectedType = typeof(uint);
                    isPrimitive = true;
                    break;

                case "long":
                    meta.selectedType = typeof(long);
                    isPrimitive = true;
                    break;

                case "ulong":
                    meta.selectedType = typeof(ulong);
                    isPrimitive = true;
                    break;

                case "float":
                    meta.selectedType = typeof(float);
                    isPrimitive = true;
                    break;

                case "double":
                    meta.selectedType = typeof(double);
                    isPrimitive = true;
                    break;

                case "string":
                    meta.selectedType = typeof(string);
                    isPrimitive = true;
                    break;

                case "Vector2":
                    meta.selectedType = typeof(Vector2);
                    isPrimitive = true;
                    break;

                case "Vector3":
                    meta.selectedType = typeof(Vector3);
                    isPrimitive = true;
                    break;

                case "Vector4":
                    meta.selectedType = typeof(Vector4);
                    isPrimitive = true;
                    break;

                case "Quaternion":
                    meta.selectedType = typeof(Quaternion);
                    isPrimitive = true;
                    break;

                case "Rect":
                    meta.selectedType = typeof(Rect);
                    isPrimitive = true;
                    break;

                case "Color":
                    meta.selectedType = typeof(Color);
                    isPrimitive = true;
                    break;
            }

            if (isPrimitive)
            {
                Debug.Log("Creating Primitive");
                Var newVar = new Var(meta.selectedType, raw[1]);
                newVar.input = input;
                data.variables.Add(newVar);
                return;
            }

            else
            {
                FindType(raw[0], out type, out ASM);

                if (type == null)
                {
                    Debug.Log("Type not found in variable creation");
                    return;
                }

                if (type.Length == 1)
                {
                    Debug.Log("One type found, successfully created variable");
                    meta.selectedType = type[0];
                    meta.selectedAsm = meta.selectedType.Assembly;

                    meta.input = input;
                    Var newVar = new Var(type[0], raw[1]);
                    newVar.input = input;
                    data.variables.Add(newVar);
                    return;
                }

                else
                {
                    Debug.Log("Multiple types found");
                    meta.input = input;
                    meta.types = type;
                    meta.asms = ASM;
                    return;
                }
            }
        }
        
        if (meta.selectedType != null)
        {
            Debug.Log("type selected and var successfully created");
            Var newVar = new Var(meta.selectedType, raw[1]);
            newVar.input = meta.input;
            data.variables.Add(newVar);
        }
        
    }

    public object ParseArgumentType(string arg)
    {                        
         //TO-DO: Add Keywords or find a way to get members or possible function calls, maybe try using the LINQ thing above 
        bool bVal;                
        int iVal;        
        long lVal;        
        float fVal;
        double dVal;

        if (Boolean.TryParse(arg, out bVal))                   
           return bVal;
      
        if (Int32.TryParse(arg, out iVal))                   
           return iVal;

       
        if (Int64.TryParse(arg, out lVal))
            return lVal;

        
        if (Single.TryParse(arg, out fVal))                   
           return fVal;
        
        if (Double.TryParse(arg, out dVal))                   
           return dVal;
        
        // Check the " character
        if (arg[0] == '"')        
           return arg;
        

        return null;
    }

    //The Black Magic with LINQ Begins Here: ---------------------------------------------

    public Func<object, object[], object> CreateConstructor(ConstructorInfo info)
    {
        ParameterExpression instanceParameter = Expression.Parameter(typeof(object), "target");
        ParameterExpression argumentsParameter = Expression.Parameter(typeof(object[]), "arguments");
        
        NewExpression call = Expression.New
            (                
                info,
                CreateConstructorParameters(info, argumentsParameter)
            );

        Expression<Func<object, object[], object>> lambda = Expression.Lambda<Func<object, object[], object>>(
            Expression.Convert(call, typeof(object)),
            instanceParameter,
            argumentsParameter);
        
        return lambda.Compile();
    }

    //I'm not exactly happy about this, but this is only solution I could find to make it more readable
    Expression[] CreateConstructorParameters(ConstructorInfo method, Expression argumentsParameter)
    {
        return method.GetParameters().Select
            (
                (parameter, index) =>
            
                Expression.Convert
                (
                    Expression.ArrayIndex(argumentsParameter, Expression.Constant(index)), parameter.ParameterType
                )
          
            ).Cast<Expression>().ToArray();
    }

    public Expression[] CreateMethodParameters(MethodInfo method, Expression argumentsParameter)
    {
        return method.GetParameters().Select
            (
                (parameter, index) =>

                Expression.Convert
                (
                    Expression.ArrayIndex(argumentsParameter, Expression.Constant(index)), parameter.ParameterType
                )

            ).Cast<Expression>().ToArray();
    }

    
    public Func<object, object[], object> CreateGetFunction(MemberInfo info)
    {
        ParameterExpression instanceParameter = Expression.Parameter(typeof(object), "target");
        ParameterExpression argumentsParameter = Expression.Parameter(typeof(object[]), "arguments");

        MemberExpression accessMember = null;

        if (info is FieldInfo)
        {
            FieldInfo field = (FieldInfo)info;

            var convert = Expression.Convert(instanceParameter, field.DeclaringType);

            accessMember = Expression.Field(convert, field);

            var convert2 = Expression.Convert(accessMember, typeof(object));

            Expression<Func<object, object[], object>> lambda = Expression.Lambda<Func<object, object[], object>>
            (
                convert2,
                instanceParameter,
                argumentsParameter
            );

            return lambda.Compile();
        }

        if (info is PropertyInfo)
        {
            PropertyInfo property = (PropertyInfo)info;

            var convert = Expression.Convert(instanceParameter, property.DeclaringType);

            accessMember = Expression.Property(convert, property);

            var convert2 = Expression.Convert(accessMember, typeof(object));

            Expression<Func<object, object[], object>> lambda = Expression.Lambda<Func<object, object[], object>>
            (
                convert2,
                instanceParameter,
                argumentsParameter
            );

            return lambda.Compile();
        }

        return null;
    }

    public Func<object, object[], object> CreateSetFunction(MemberInfo info)
    {
        ParameterExpression instanceParameter = Expression.Parameter(typeof(object), "target");
        ParameterExpression argumentsParameter = Expression.Parameter(typeof(object[]), "arguments");

        MemberExpression accessMember = null;

        if (info is FieldInfo)
        {
            FieldInfo field = (FieldInfo)info;

            //Get the target object converted to assign
            var actualField = Expression.Convert(instanceParameter, field.DeclaringType);

            //Convert the other into the right type - there should only be one argument
            var assign = Expression.Convert(Expression.ArrayIndex(argumentsParameter, Expression.Constant(0)), field.FieldType);
           
            //Get the field from target object
            accessMember = Expression.Field(actualField, field);

            //Perform the assignment
            var result = Expression.Assign(accessMember, assign);

            //Set the return type properly
            var convert = Expression.Convert(result, typeof(object));

            Expression<Func<object, object[], object>> lambda = Expression.Lambda<Func<object, object[], object>>
            (
                convert,
                instanceParameter,
                argumentsParameter
            );

            return lambda.Compile();
        }

        if (info is PropertyInfo)
        {
            PropertyInfo property = (PropertyInfo)info;

            //Get the target object converted to assign
            var actualField = Expression.Convert(instanceParameter, property.DeclaringType);

            //Convert the other into the right type - there should only be one argument
            var assign = Expression.Convert(Expression.ArrayIndex(argumentsParameter, Expression.Constant(0)), property.PropertyType);

            //Get the field from target object
            accessMember = Expression.Property(actualField, property);

            //Perform the assignment
            var result = Expression.Assign(accessMember, assign);

            var convert = Expression.Convert(result, typeof(object));

            Expression<Func<object, object[], object>> lambda = Expression.Lambda<Func<object, object[], object>>
            (
                convert,
                instanceParameter,
                argumentsParameter
            );

            

            return lambda.Compile();
        }

        return null;
    }

   

    /*
        Current Notes on the Full Compile:
        It seems that to execute a sequence, we need the Expression.Block(IEnumerable<ParameterExpression> variables, Expression[] body)
        Do these variables only refer to ones that exist purely as expressions and are not defined elsewhere?

        We should be able to compile each node individually as before, but not return the delegate and instead the expression

        With that in mind, we should then be able to nest the function expression in a new expression tree that prepares the parameters
        for it and calls it.

        May want to return the raw MethodCallExpression and NOT the final lambda

        Big question marks that remain are: Conditionals, and contextual nodes for continuous member access       

        Would recommend running tests of the similar set up in a test class

        Remember end goal should have a delegate where the variables dictionary is passed in

        Add Expression or Expression[] to nodes
    */

    string GetLiteral(object obj)
    {
        Type type = obj.GetType();

        if (type == typeof(bool))
        {
            bool val = (bool)obj;

            if (val)
                return "true";
            else
                return "false";
        }

        if (type == typeof(char))
        {
            return $"'{obj.ToString()}'";
        }

        if (type == typeof(int))
        {
            return obj.ToString();
        }

        if (type == typeof(long))
        {
            return obj.ToString();
        }

        if (type == typeof(float))
        {
            return obj.ToString() + "f";
        }

        if (type == typeof(double))
        {
            return obj.ToString();
        }

        if (type == typeof(string))
        {
            //@"UnityEngine.Debug.Log( ""Object name is  "" + someObj.name );"
            string quote = @"""";
            return quote + obj.ToString() + quote;
        }

        if (type == typeof(Vector2))
        {
            Vector2 v = (Vector2)obj;
            return "new UnityEngine.Vector2" + $"({v.x}f, {v.y}f)";
        }

        if (type == typeof(Vector3))
        {
            Vector3 v = (Vector3)obj;
            return "new UnityEngine.Vector3" + $"({v.x}f, {v.y}f, {v.z}f)";
        }

        if (type == typeof(Vector4))
        {
            Vector4 v = (Vector4)obj;
            return "new UnityEngine.Vector4" + $"({v.x}f, {v.y}f, {v.z}f, {v.w})";
        }

        if (type == typeof(Quaternion))
        {            
            Quaternion q = (Quaternion)obj;
            return "new UnityEngine.Quaternion" + $"({q.x}f, {q.y}f, {q.z}f, {q.w}f)";
        }

        if (type == typeof(Rect))
        {
            Rect r = (Rect)obj;
            return $"new UnityEngine.Rect({r.x}f, {r.y}f, {r.width}f, {r.height}f)";
        }

        if (type == typeof(Color))
        {
            Color c = (Color)obj;
            return $"new UnityEngine.Color({c.r}f, {c.g}f, {c.b}f, {c.a}f)";
        }

        if (type.BaseType == typeof(System.Enum))
        {
            string debug = obj.GetType().ToString();

            debug = debug.Replace('+', '.');

            string debug2 = obj.ToString();
            return debug + "." + obj.ToString();
        }
        
        return "";
    }
    
    public Type FullCompile(Blueprint data, Type baseClass)
    {
        if (data == null)
            return null;

        CSharpCodeProvider compiler = new CSharpCodeProvider();

        CompilerParameters parameters = new CompilerParameters();
        parameters.GenerateExecutable = false;
        parameters.GenerateInMemory = false;
        parameters.ReferencedAssemblies.Add(typeof(MonoBehaviour).Assembly.Location);

        StringBuilder classFile = new StringBuilder();

        //referenceNodes = data.nodes;
        //refParams = new List<Parameter>();
        //uint compileID = 1;
        //Setup the using directives
        
        foreach (Node node in data.nodes)
        {
            if (!parameters.ReferencedAssemblies.Contains(node.assemblyPath))
            {
                parameters.ReferencedAssemblies.Add(node.assemblyPath);
            }

            foreach(Parameter p in node.paramList)
            {
                //p.compileID = compileID;
                //refParams.Add(p);
                //compileID++;

                if (p.isGeneric)
                {
                    parameters.ReferencedAssemblies.Add(p.templateTypeAsmPath);
                }
            }            
        }

        //Debug.Log("Initial Parameters");
        //PrintParameters();


        //classFile.Append("using UnityEngine;\n");
        //classFile.Append("using System.Collections;\n");
        //classFile.Append("using System.Collections.Generic;\n");

        if (UseGameCompile)
            classFile.Append("using Game;");

        //Declare the Type
        if (baseClass != null)
            classFile.Append("public class " + data.name + " : " + baseClass.ToString() + "\n");
        else
            classFile.Append("public class " + data.name + "\n");

        classFile.Append("{\n");

        //Add variable declarations

        if (UseGameCompile)
        {
            parameters.ReferencedAssemblies.Add(this.GetType().Assembly.Location);
            parameters.ReferencedAssemblies.Add(typeof(Input).Assembly.Location);
            parameters.ReferencedAssemblies.Add(typeof(Rigidbody).Assembly.Location);
            parameters.ReferencedAssemblies.Add(typeof(Physics).Assembly.Location);
            parameters.ReferencedAssemblies.Add(typeof(Time).Assembly.Location);
            parameters.ReferencedAssemblies.Add(typeof(Collider).Assembly.Location);
            parameters.ReferencedAssemblies.Add(typeof(Camera).Assembly.Location);
            parameters.ReferencedAssemblies.Add(typeof(GameObject).Assembly.Location);
            parameters.ReferencedAssemblies.Add(typeof(Color).Assembly.Location);
            parameters.ReferencedAssemblies.Add(typeof(Vector3).Assembly.Location);
            parameters.ReferencedAssemblies.Add(typeof(Quaternion).Assembly.Location);
            parameters.ReferencedAssemblies.Add(typeof(Renderer).Assembly.Location);
            parameters.ReferencedAssemblies.Add(typeof(UnityEngine.Random).Assembly.Location);
            parameters.ReferencedAssemblies.Add(typeof(Mathf).Assembly.Location);
        }
        foreach (string varName in data.variables.Keys)
        {
            Var v = data.variables[varName];
            parameters.ReferencedAssemblies.Add(v.type.Assembly.Location);
            string declare = $"public {v.type.ToString()} {v.name};\n";
            classFile.Append(declare);
        }

        classFile.Append("\n");

        foreach (string entryName in data.entryPoints.Keys)
        {
            Node node = data.entryPoints[entryName];

            string passInParams = "";

            if (node.passInParams != null)
            {
                for (int i = 0; i < node.passInParams.Count; i++)
                {
                    passInParams += node.passInParams[i];

                    if (i < node.passInParams.Count - 1)
                        passInParams += ", ";
                }
            }

            StringBuilder funcBuilder = new StringBuilder();

            string funcDeclaration = "";

            if (!node.isVirtual)
                funcDeclaration = $"public void {entryName}(" + passInParams + ")\n{\n";
            else
                funcDeclaration = $"public override void {entryName}(" + passInParams + ")\n{\n";

            funcBuilder.Append(funcDeclaration);

            List<string> lines = new List<string>(); //The final lines
            List<string> baseLines = new List<string>();

            //Begin Main Loop
            while (node != null)
            {
                if (node.nodeType == NodeType.Function)
                {
                    string[] split = node.input.Split(' ');

                    string passInArgs = "";
                    string genericDefine = "";

                    List<Parameter> generics = new List<Parameter>();

                    parameters.ReferencedAssemblies.Add(node.assemblyPath);
                  
                    //Set up the parameters
                    //Debug.Log("Function node parameters in initial compile");
                    //PrintNodeParameters(node);

                    if (node.paramList != null)
                    {
                        for (int i = 0; i < node.paramList.Count; i++)
                        {
                            //Add Generics
                            if (node.paramList[i].isGeneric)
                            {
                                generics.Add(node.paramList[i]);
                                continue;
                            }

                            //Add the normal ones
                            if (node.paramList[i].inputVar)
                                passInArgs += node.paramList[i].varInput;
                            else
                                passInArgs += GetLiteral(node.paramList[i].arg);

                            if (i < node.paramList.Count - 1)
                                passInArgs += ", ";
                        }
                    }

                    //Process Generics
                    if (generics.Count > 0)
                    {
                        genericDefine = "<";

                        for (int i = 0; i < generics.Count; i++)
                        {
                            genericDefine += generics[i].templateType;

                            if (i < generics.Count - 1)
                                genericDefine += ", ";
                        }

                        genericDefine += ">";

                    }

                    if (split.Length > 1)
                    {
                        //If a node is returning that should be the end of the particular line
                        if (node.returnInput != "")
                        {
                            if (node.isStatic)
                            {
                                if (node.isGenericFunction)
                                {
                                    baseLines.Add($"{node.type}.{split[1]}{genericDefine}(" + passInArgs + ")");
                                    lines.Add($"{node.returnInput} = {node.type}.{split[1]}{genericDefine}(" + passInArgs + ");");
                                }
                                else
                                {
                                    baseLines.Add($"{node.type}.{split[1]}(" + passInArgs + ")");
                                    lines.Add($"{node.returnInput} = {node.type}.{split[1]}(" + passInArgs + ");");
                                }
                            }
                            else
                            {
                                if (node.isGenericFunction)
                                {
                                    baseLines.Add($"{split[0]}.{split[1]}{genericDefine}(" + passInArgs + ")");
                                    lines.Add($"{node.returnInput} = {split[0]}.{split[1]}{genericDefine}(" + passInArgs + ");");
                                }
                                else
                                {
                                    baseLines.Add($"{split[0]}.{split[1]}(" + passInArgs + ")");
                                    lines.Add($"{node.returnInput} = {split[0]}.{split[1]}(" + passInArgs + ");");
                                }
                            }

                            //if (node.nextNode != null)
                            //{
                            //    if (!node.nextNode.isContextual)
                            //    {
                            //        if (node.isStatic)
                            //        {
                            //            if (node.isGenericFunction)
                            //                lines.Add($"{node.returnInput} = {node.type}.{split[1]}{genericDefine}(" + passInArgs + ");");
                            //            else
                            //                lines.Add($"{node.returnInput} = {node.type}.{split[1]}(" + passInArgs + ");");
                            //        }
                            //        else
                            //        {
                            //            if (node.isGenericFunction)
                            //                lines.Add($"{node.returnInput} = {split[0]}.{split[1]}{genericDefine}(" + passInArgs + ");");
                            //            else
                            //                lines.Add($"{node.returnInput} = {split[0]}.{split[1]}(" + passInArgs + ");");
                            //        }
                            //    }
                            //}
                        }

                        //No Returning
                        else
                        {
                            //Static No Return
                            if (node.isStatic)
                            {                               
                                if (node.isGenericFunction)
                                    baseLines.Add($"{node.type}.{split[1]}{genericDefine}(" + passInArgs + ")");
                                else
                                    baseLines.Add($"{node.type}.{split[1]}(" + passInArgs + ")");

                                //If next node is not contexual execute the line
                                if (node.nextNode != null)
                                {
                                    if (!node.nextNode.isContextual)
                                    {
                                        if (node.isGenericFunction)
                                            lines.Add($"{node.type}.{split[1]}{genericDefine}(" + passInArgs + ");");
                                        else
                                            lines.Add($"{node.type}.{split[1]}(" + passInArgs + ");");
                                    }
                                }

                                //If next node does not exist execute the line
                                else
                                {
                                    if (node.isGenericFunction)
                                        lines.Add($"{node.type}.{split[1]}{genericDefine}(" + passInArgs + ");");
                                    else
                                        lines.Add($"{node.type}.{split[1]}(" + passInArgs + ");");
                                }

                            }

                            //Non Static No Return
                            else
                            {
                                if (node.isGenericFunction)
                                    baseLines.Add($"{split[0]}.{split[1]}{genericDefine}(" + passInArgs + ")");
                                else
                                    baseLines.Add($"{split[0]}.{split[1]}(" + passInArgs + ")");

                                //If next node is not contexual execute the line
                                if (node.nextNode != null)
                                {
                                    if (!node.nextNode.isContextual)
                                    {
                                        if (node.isGenericFunction)
                                            lines.Add($"{split[0]}.{split[1]}{genericDefine}(" + passInArgs + ");");
                                        else
                                            lines.Add($"{split[0]}.{split[1]}(" + passInArgs + ");");
                                    }
                                }

                                //If next node does not exist execute the line
                                else
                                {
                                    if (node.isGenericFunction)
                                        lines.Add($"{split[0]}.{split[1]}{genericDefine}(" + passInArgs + ");");
                                    else
                                        lines.Add($"{split[0]}.{split[1]}(" + passInArgs + ");");
                                }

                            }
                            //if (node.nextNode != null)
                            //{
                            //    if (!node.nextNode.isContextual)
                            //    {
                            //        if (node.isGenericFunction)
                            //            lines.Add($"{split[0]}.{split[1]}{genericDefine}(" + passInArgs + ");");
                            //        else
                            //            lines.Add($"{split[0]}.{split[1]}(" + passInArgs + ");");
                            //    }
                            //}
                            //
                            //else
                            //{
                            //    if (node.isGenericFunction)
                            //        lines.Add($"{split[0]}.{split[1]}{genericDefine}(" + passInArgs + ");");
                            //    else
                            //        lines.Add($"{split[0]}.{split[1]}(" + passInArgs + ");");
                            //}
                        }
                    } 
                    
                    //Enter here if node is contextual or using a base class method
                    else
                    {
                        //If you are returning go in here
                        if (node.returnInput != "")
                        {
                            if (node.isContextual)
                            {
                                //Create the lines
                                string prevLine = baseLines[baseLines.Count - 1];
                                string newLine = prevLine + $".{node.input}(" + passInArgs + ")";

                                if (node.isGenericFunction)
                                    newLine = prevLine + $".{node.input}{genericDefine}(" + passInArgs + ")";

                                //Add the line
                                baseLines.Add(newLine);
                                lines.Add($"{node.returnInput} = " + newLine + ";");
                            }

                            //Not a contextual node
                            else
                            {
                                if (node.isGenericFunction)
                                {
                                    baseLines.Add($"{node.input}{genericDefine}(" + passInArgs + ")");
                                    lines.Add($"{node.returnInput} = {node.input}{genericDefine}(" + passInArgs + ");");
                                }
                                else
                                {
                                    baseLines.Add($"{node.input}(" + passInArgs + ")");
                                    lines.Add($"{node.returnInput} = {node.input}(" + passInArgs + ");");
                                }
                                
                            }
                        }

                        //If not returning
                        else
                        {
                            if (node.isContextual)
                            {
                                string prevLine = baseLines[baseLines.Count - 1];
                                string newLine = "";

                                if (node.isGenericFunction)
                                    newLine = prevLine + $".{node.input}{genericDefine}(" + passInArgs + ")";
                                else
                                    newLine = prevLine + $".{node.input}(" + passInArgs + ")";

                                baseLines.Add(newLine);

                                //If the next node is not contextual execute the line
                                if (node.nextNode != null)
                                {
                                    if (!node.nextNode.isContextual)
                                    {
                                        lines.Add(newLine + ";");
                                    }
                                }

                                //If the next node does not exist, execute the line
                                else
                                {
                                    lines.Add(newLine + ";");
                                }

                            }

                            //If not contextual
                            else
                            {
                                if (node.isGenericFunction)
                                {
                                    baseLines.Add($"{node.input}{genericDefine}(" + passInArgs + ")");
                                    lines.Add($"{node.input}{genericDefine}(" + passInArgs + ");");
                                }
                                else
                                {
                                    baseLines.Add($"{node.input}(" + passInArgs + ")");
                                    lines.Add($"{node.input}(" + passInArgs + ");");
                                }
                            }
                        }
                    }

                }

                if (node.nodeType == NodeType.Constructor)
                {
                    string[] split = node.input.Split(' ');

                    string passInArgs = "";
                    string genericDefine = "";

                    List<Parameter> generics = new List<Parameter>();

                    parameters.ReferencedAssemblies.Add(node.assemblyPath);
                    
                    for (int i = 0; i < node.paramList.Count; i++)
                    {
                        if (node.paramList[i].isGeneric)
                        {
                            generics.Add(node.paramList[i]);
                            continue;
                        }

                        if (node.paramList[i].inputVar)
                            passInArgs += node.paramList[i].varInput;
                        else
                            passInArgs += GetLiteral(node.paramList[i].arg);

                        if (i < node.paramList.Count - 1)
                            passInArgs += ", ";
                    }

                    if (generics.Count > 0)
                    {
                        genericDefine = "<";

                        for (int i = 0; i < generics.Count; i++)
                        {
                            genericDefine += generics[i].templateType;

                            if (i < generics.Count - 1)
                                genericDefine += ", ";
                        }

                        genericDefine += ">";

                    }

                    if (split.Length > 1)
                    {                        
                        string line = $"{node.type}.{split[0]}(" + passInArgs + ")";

                        if (node.isGenericFunction)
                            line = $"{node.type}.{split[0]}{genericDefine}(" + passInArgs + ")";

                        if (node.returnInput != "")
                        {
                            //string line = $"{node.nameSpace}.{split[0]}(" + passInArgs + ")";
                            baseLines.Add(line);
                            lines.Add($"{node.returnInput} =  new " + line + ";");
                        }

                    }
                }


                if (node.nodeType == NodeType.Field_Get)
                {
                    string[] split = node.input.Split(' ');

                    parameters.ReferencedAssemblies.Add(node.assemblyPath);
                    

                    if (node.isContextual)
                    {
                        string prevLine = baseLines[baseLines.Count - 1];
                        string newLine = prevLine + $".{node.input}";
                        baseLines.Add(newLine);

                        if (node.returnInput != "")
                        {                            
                            lines.Add($"{node.returnInput} = " + newLine + ";");                                                        
                        }
                        else
                            lines.Add(newLine + ";");
                    }

                    else
                    {
                        string newLine = "";

                        if (node.isStatic)
                        {
                            //string nameSpace = node.type.Split('.')[0];
                            //newLine = $"{node.nameSpace}.{split[0]}.{split[1]}";
                            //newLine = $"{nameSpace}.{split[0]}.{split[1]}";
                            newLine = $"{node.type}.{split[1]}";
                        }
                        else
                        {
                            if (split.Length > 1)
                                newLine = $"{split[0]}.{split[1]}";
                            else
                                newLine = $"{split[0]}";
                        }

                        baseLines.Add(newLine);

                        if (node.returnInput != "")
                        {
                            lines.Add($"{node.returnInput} = " + newLine + ";");
                        }
                    }
                }

                if (node.nodeType == NodeType.Field_Set)
                {
                    string[] split = node.input.Split(' ');

                    parameters.ReferencedAssemblies.Add(node.assemblyPath);                   

                    if (node.isContextual)
                    {
                        string prevLine = baseLines[baseLines.Count - 1];
                        string newLine = prevLine + $"{node.input}";
                        baseLines.Add(newLine);

                        if (node.paramList[0].inputVar)
                            lines.Add(newLine + $" = {node.paramList[0].varInput};");
                        else
                            lines.Add(newLine + $" = {GetLiteral(node.paramList[0].arg)};");
                    }

                    else
                    {
                        if (node.paramList[0].inputVar)
                        {
                            if (node.isStatic)
                            {
                                //string nameSpace = node.type.Split('.')[0];
                                //lines.Add($"{node.nameSpace}.{split[0]}.{split[1]} = {node.paramList[0].varInput};");
                                //lines.Add($"{nameSpace}.{split[0]}.{split[1]} = {node.paramList[0].varInput};");
                                lines.Add($"{node.type}.{split[1]} = {node.paramList[0].varInput};");
                            }
                            else
                            {
                                if (split.Length > 1)
                                    lines.Add($"{split[0]}.{split[1]} = {node.paramList[0].varInput};");
                                else
                                    lines.Add($"{split[0]} = {node.paramList[0].varInput};");
                            }
                        }
                        else
                        {
                            if (node.isStatic)
                            {
                                //string nameSpace = node.type.Split('.')[0];
                                //lines.Add($"{node.nameSpace}.{split[0]}.{split[1]} = {GetLiteral(node.paramList[0].arg)};");
                                //lines.Add($"{nameSpace}.{split[0]}.{split[1]} = {GetLiteral(node.paramList[0].arg)};");
                                lines.Add($"{node.type}.{split[1]} = {GetLiteral(node.paramList[0].arg)};");
                            }
                            else
                            {
                                if (split.Length > 1)
                                    lines.Add($"{split[0]}.{split[1]} = {GetLiteral(node.paramList[0].arg)};");
                                else
                                    lines.Add($"{split[0]} = {GetLiteral(node.paramList[0].arg)};");
                            }
                        }
                    }

                }

                if (node.nodeType == NodeType.Property_Get)
                {
                    string[] split = node.input.Split(' ');

                    parameters.ReferencedAssemblies.Add(node.assemblyPath);
                    

                    if (node.isContextual)
                    {
                        string prevLine = baseLines[baseLines.Count - 1];
                        string newLine = prevLine + $".{node.input}";
                        baseLines.Add(newLine);

                        if (node.returnInput != "")
                            lines.Add($"{node.returnInput} = " + newLine + ";");
                        else
                            lines.Add(newLine + ";");
                    }

                    else
                    {
                        string newLine = "";

                        if (node.isStatic)
                        {
                            //string nameSpace = node.type.Split('.')[0];
                            //newLine = $"{node.nameSpace}.{split[0]}.{split[1]}";
                            //newLine = $"{nameSpace}.{split[0]}.{split[1]}";
                            newLine = $"{node.type}.{split[1]}";
                        }
                        else
                        {
                            if (split.Length > 1)
                                newLine = $"{split[0]}.{split[1]}";
                            else
                                newLine = $"{split[0]}";
                        }

                        baseLines.Add(newLine);

                        if (node.returnInput != "")
                        {
                            lines.Add($"{node.returnInput} = " + newLine + ";");
                        }
                    }
                }

                if (node.nodeType == NodeType.Property_Set)
                {
                    string[] split = node.input.Split(' ');

                    parameters.ReferencedAssemblies.Add(node.assemblyPath);
                   
                    if (node.isContextual)
                    {
                        string prevLine = baseLines[baseLines.Count - 1];
                        string newLine = prevLine + $"{node.input}";
                        baseLines.Add(newLine);

                        if (node.paramList[0].inputVar)
                            lines.Add(newLine + $" = {node.paramList[0].varInput};");
                        else
                            lines.Add(newLine + $" = {GetLiteral(node.paramList[0].arg)};");
                    }

                    else
                    {
                        if (node.paramList[0].inputVar)
                        {
                            if (node.isStatic)
                            {
                                //string nameSpace = node.type.Split('.')[0];
                                //lines.Add($"{node.nameSpace}.{split[0]}.{split[1]} = {node.paramList[0].varInput};");
                                //lines.Add($"{nameSpace}.{split[0]}.{split[1]} = {node.paramList[0].varInput};");
                                lines.Add($"{node.type}.{split[1]} = {node.paramList[0].varInput};");
                            }
                            else
                            {
                                if (split.Length > 1)
                                    lines.Add($"{split[0]}.{split[1]} = {node.paramList[0].varInput};");
                                else
                                    lines.Add($"{split[0]} = {node.paramList[0].varInput};");
                            }
                        }
                        else
                        {
                            if (node.isStatic)
                            {
                                //string nameSpace = node.type.Split('.')[0];
                                //lines.Add($"{node.nameSpace}.{split[0]}.{split[1]} = {GetLiteral(node.paramList[0].arg)};");
                                //lines.Add($"{nameSpace}.{split[0]}.{split[1]} = {GetLiteral(node.paramList[0].arg)};");
                                lines.Add($"{node.type}.{split[1]} = {GetLiteral(node.paramList[0].arg)};");
                            }
                            else
                            {
                                if (split.Length > 1)
                                    lines.Add($"{split[0]}.{split[1]} = {GetLiteral(node.paramList[0].arg)};");
                                else
                                    lines.Add($"{split[0]}.{split[1]} = {GetLiteral(node.paramList[0].arg)};");
                            }
                        }
                    }
                }

                if (node.nodeType == NodeType.Conditional)
                {
                    if (node.nextNode != null)
                    {
                        if (node.isContextual)
                        {
                            string prevLine = baseLines[baseLines.Count - 1];
                            string newLine = $"if (" + prevLine + ")";
                            lines.Add(newLine);
                            lines.Add("{");
                        }

                        else
                        {
                            if (node.paramList[0].inputVar)
                                lines.Add($"if ({node.paramList[0].varInput})");
                            else
                                lines.Add($"if ({node.paramList[0].arg.ToString()})");
                            lines.Add("{");
                        }

                        //Debug.Log("Parameters in initial compile right before true");
                        //PrintParameters();

                        List<string> result = FullCompileConditional(node.nextNode, true, parameters);

                        foreach (string line in result)
                        {
                            lines.Add(line);
                        }

                        lines.Add("}");
                    }

                    if (node.falseNode != null)
                    {
                        lines.Add("else");
                        lines.Add("{");

                        //Debug.Log("Parameters in initial compile right before false");
                        //PrintParameters();

                        List<string> result = FullCompileConditional(node.falseNode, false, parameters);

                        foreach (string line in result)
                        {
                            lines.Add(line);
                        }

                        lines.Add("}");
                    }

                    break;

                }

                if (node.nodeType == NodeType.Operation)
                {
                    string[] split = node.input.Split(' ');

                    if (node.isContextual)
                    {
                        string prevLine = baseLines[baseLines.Count - 1];

                        if (node.paramList[0].inputVar)
                        {
                            string newLine = prevLine + $" {split[0]} {node.paramList[0].varInput}";
                            baseLines.Add(newLine);

                            //Check if the next node is contextual for parentheses
                            if (node.nextNode != null)
                            {
                                if (node.nextNode.isContextual && split[0] != "=")
                                {
                                    baseLines.Add($"({newLine})");
                                }
                            }
                            if (node.returnInput != "")
                                lines.Add($"{node.returnInput} = " + newLine + ";");
                            //else
                            //    lines.Add(newLine + ";");
                        }

                        else
                        {
                            string newLine = prevLine + $" {split[0]} {GetLiteral(node.paramList[0].arg)}";
                            baseLines.Add(newLine);

                            //Check if the next node is contextual for parentheses
                            if (node.nextNode != null)
                            {
                                if (node.nextNode.isContextual && split[0] != "=")
                                {
                                    baseLines.Add($"({newLine})");
                                }
                            }

                            if (node.returnInput != "")
                                lines.Add($"{node.returnInput} = " + newLine + ";");
                            //else
                            //    lines.Add(newLine + ";");
                        }
                    }

                    else
                    {
                        if (node.paramList[0].inputVar)
                        {
                            string newLine = $"{split[0]} {split[1]} {node.paramList[0].varInput}";
                            baseLines.Add(newLine);

                            if (node.nextNode != null)
                            {
                                if (node.nextNode.isContextual && split[1] != "=")
                                {
                                    baseLines.Add($"({newLine})");
                                }
                            }

                            if (node.returnInput != "")
                                lines.Add($"{node.returnInput} = " + newLine + ";");
                            else
                            {
                                if (split[1] == "=")
                                {
                                    lines.Add(newLine + ";");
                                }
                            }
                        }

                        else
                        {
                            string newLine = $"{split[0]} {split[1]} {GetLiteral(node.paramList[0].arg)}";
                            baseLines.Add(newLine);

                            if (node.nextNode != null)
                            {
                                if (node.nextNode.isContextual && split[1] != "=")
                                {
                                    baseLines.Add($"({newLine})");
                                }
                            }

                            if (node.returnInput != "")
                                lines.Add($"{node.returnInput} = " + newLine + ";");
                            
                            else
                            {
                                if (split[1] == "=")
                                {
                                    lines.Add(newLine + ";");
                                }
                            }

                        }
                    }
                }

                if (node.nextNode == null)
                {
                    //lines.Add("}\n");
                    break;
                }
                node = node.nextNode;
            }

            foreach (string line in lines)
            {
                funcBuilder.Append(line + "\n");
            }

            //Add space after function
            funcBuilder.Append("}\n");
            classFile.Append(funcBuilder.ToString());            
        }

        //Ending bracket for class
        classFile.Append("}\n");
        //classFile.Append("}\n");
        //string final = classFile.ToString();
        //final += "}\n";

        //Debug File Create
        //FileStream file = File.Create(Application.persistentDataPath + "/" + data.name + ".cs");
        //BinaryFormatter bf = new BinaryFormatter();
        //string final = classFile.ToString();        
        //bf.Serialize(file, final);
        //file.Close();

        File.WriteAllText(Application.persistentDataPath + "/" + data.name + ".cs", classFile.ToString());

        //Add actual Compiling here        
        //parameters.OutputAssembly = "NewAssembly.dll";
                
        //Game/Realtime-specific
        if (File.Exists($"{data.name}.dll"))
        {
            if (Application.isPlaying)
            {
                Debug.Log("File exists attempting rename");
                //File.Move($"{data.name}.dll", $"{data.name}-old.dll");

                bool rename = false;
                string newName = $"{data.name}";
                
                while (!rename)
                {                    
                    try
                    {
                        
                        int length = (int)UnityEngine.Random.Range(10, 20);
                        for (int i = 0; i < length; i++)
                        {
                            newName += $"{(int)UnityEngine.Random.Range(0, 10000)}";
                        }
                
                        //File.Move($"{data.name}.dll", $"{data.name} ");
                        File.Move($"{data.name}.dll", newName + ".dll");
                        Debug.Log("Setting rename to true");
                        rename = true;
                    }
                
                    catch
                    {
                        Debug.Log("Failed to rename, trying again");
                        rename = false;
                    }
                }

                //string time = System.DateTime.Now.ToString();
                //time = time.Replace('/', '-');
                //string[] parts = time.Split(' ');
                //
                //string finalTime = "";
                //foreach (string str in parts)
                //    finalTime += str;
                //
                //string newName = $"{data.name}{finalTime}.dll";
                //File.Move($"{data.name}.dll", newName);
                
                ComponentInventory.Instance.RemoveClass(data.name);
                GameManager.Instance.RemoveClass(data.name);
                //RealTimeEditor.Instance.deleteAsmPath.Add($"{data.name}-old.dll");
                GameManager.Instance.AddOldClass(newName + ".dll");
                parameters.OutputAssembly = $"{data.name}.dll";
            }

            else
            {
                File.Delete($"{data.name}.dll");
                parameters.OutputAssembly = $"{data.name}.dll";
            }
        }
        else
            parameters.OutputAssembly = $"{data.name}.dll";

        string final = classFile.ToString();

        CompilerResults compile = compiler.CompileAssemblyFromSource(parameters, final);
        

        if (compile.Errors.Count > 0)
        {
            Debug.Log($"Errors building {final} to  {compile.PathToAssembly}");

            foreach (CompilerError error in compile.Errors)
            {
                Debug.Log($"  {error.ToString()}  ");
            }
        }

        Type type = compile.CompiledAssembly.GetType(data.name);

        data.dataRef.compiledClassType = type.ToString();
        data.dataRef.compiledClassTypeAsmPath = type.Assembly.Location;

        if (Application.isPlaying)        
            GameManager.Instance.AddClass(type);
        


#if UNITY_EDITOR
        EditorUtility.SetDirty(data.dataRef);
        AssetDatabase.Refresh();
#endif

        return type;
    }

    List<string> FullCompileConditional(Node node, bool truePath, CompilerParameters parameters)
    {
        List<string> lines = new List<string>();
        List<string> baseLines = new List<string>();

        //Debug.Log($"Parameters in conditional on {truePath} path");
        //PrintParameters();

        while (node != null)
        {
            //node = GetNode(node.ID);

            if (node.nodeType == NodeType.Function)
            {                
                string[] split = node.input.Split(' ');

                string passInArgs = "";
                string genericDefine = "";

                List<Parameter> generics = new List<Parameter>();

                parameters.ReferencedAssemblies.Add(node.assemblyPath);
               
                //Debug.Log($"Function node parameters in conditional on {truePath} path");
                //PrintNodeParameters(node);

                //Set up the parameters
                if (node.paramList != null)
                {
                    for (int i = 0; i < node.paramList.Count; i++)
                    {
                        //Add Generics
                        if (node.paramList[i].isGeneric)                        
                        {
                            generics.Add(node.paramList[i]);
                            continue;
                        }

                        //Add the normal ones
                        if (node.paramList[i].inputVar)
                            passInArgs += node.paramList[i].varInput;
                        else
                            passInArgs += GetLiteral(node.paramList[i].arg);
                       
                        if (i < node.paramList.Count - 1)
                            passInArgs += ", ";
                    }
                }

                //Process Generics
                if (generics.Count > 0)
                {
                    genericDefine = "<";

                    for (int i = 0; i < generics.Count; i++)
                    {
                        genericDefine += generics[i].templateType;

                        if (i < generics.Count - 1)
                            genericDefine += ", ";
                    }

                    genericDefine += ">";

                }

                if (split.Length > 1)
                {
                    //If a node is returning that should be the end of the particular line
                    if (node.returnInput != "")
                    {
                        if (node.isStatic)
                        {
                            if (node.isGenericFunction)
                            {
                                baseLines.Add($"{node.type}.{split[1]}{genericDefine}(" + passInArgs + ")");
                                lines.Add($"{node.returnInput} = {node.type}.{split[1]}{genericDefine}(" + passInArgs + ");");
                            }
                            else
                            {
                                baseLines.Add($"{node.type}.{split[1]}(" + passInArgs + ")");
                                lines.Add($"{node.returnInput} = {node.type}.{split[1]}(" + passInArgs + ");");
                            }
                        }
                        else
                        {
                            if (node.isGenericFunction)
                            {
                                baseLines.Add($"{split[0]}.{split[1]}{genericDefine}(" + passInArgs + ")");
                                lines.Add($"{node.returnInput} = {split[0]}.{split[1]}{genericDefine}(" + passInArgs + ");");
                            }
                            else
                            {
                                baseLines.Add($"{split[0]}.{split[1]}(" + passInArgs + ")");
                                lines.Add($"{node.returnInput} = {split[0]}.{split[1]}(" + passInArgs + ");");
                            }
                        }

                        //if (node.nextNode != null)
                        //{
                        //    if (!node.nextNode.isContextual)
                        //    {
                        //        if (node.isStatic)
                        //        {
                        //            if (node.isGenericFunction)
                        //                lines.Add($"{node.returnInput} = {node.type}.{split[1]}{genericDefine}(" + passInArgs + ");");
                        //            else
                        //                lines.Add($"{node.returnInput} = {node.type}.{split[1]}(" + passInArgs + ");");
                        //        }
                        //        else
                        //        {
                        //            if (node.isGenericFunction)
                        //                lines.Add($"{node.returnInput} = {split[0]}.{split[1]}{genericDefine}(" + passInArgs + ");");
                        //            else
                        //                lines.Add($"{node.returnInput} = {split[0]}.{split[1]}(" + passInArgs + ");");
                        //        }
                        //    }
                        //}
                    }

                    //No Returning
                    else
                    {
                        //Static No Return
                        if (node.isStatic)
                        {
                            if (node.isGenericFunction)
                                baseLines.Add($"{node.type}.{split[1]}{genericDefine}(" + passInArgs + ")");
                            else
                                baseLines.Add($"{node.type}.{split[1]}(" + passInArgs + ")");

                            //If next node is not contexual execute the line
                            if (node.nextNode != null)
                            {
                                if (!node.nextNode.isContextual)
                                {
                                    if (node.isGenericFunction)
                                        lines.Add($"{node.type}.{split[1]}{genericDefine}(" + passInArgs + ");");
                                    else
                                        lines.Add($"{node.type}.{split[1]}(" + passInArgs + ");");
                                }
                            }

                            //If next node does not exist execute the line
                            else
                            {
                                if (node.isGenericFunction)
                                    lines.Add($"{node.type}.{split[1]}{genericDefine}(" + passInArgs + ");");
                                else
                                    lines.Add($"{node.type}.{split[1]}(" + passInArgs + ");");
                            }

                        }

                        //Non Static No Return
                        else
                        {
                            if (node.isGenericFunction)
                                baseLines.Add($"{split[0]}.{split[1]}{genericDefine}(" + passInArgs + ")");
                            else
                                baseLines.Add($"{split[0]}.{split[1]}(" + passInArgs + ")");

                            //If next node is not contexual execute the line
                            if (node.nextNode != null)
                            {
                                if (!node.nextNode.isContextual)
                                {
                                    if (node.isGenericFunction)
                                        lines.Add($"{split[0]}.{split[1]}{genericDefine}(" + passInArgs + ");");
                                    else
                                        lines.Add($"{split[0]}.{split[1]}(" + passInArgs + ");");
                                }
                            }

                            //If next node does not exist execute the line
                            else
                            {
                                if (node.isGenericFunction)
                                    lines.Add($"{split[0]}.{split[1]}{genericDefine}(" + passInArgs + ");");
                                else
                                    lines.Add($"{split[0]}.{split[1]}(" + passInArgs + ");");
                            }

                        }
                        //if (node.nextNode != null)
                        //{
                        //    if (!node.nextNode.isContextual)
                        //    {
                        //        if (node.isGenericFunction)
                        //            lines.Add($"{split[0]}.{split[1]}{genericDefine}(" + passInArgs + ");");
                        //        else
                        //            lines.Add($"{split[0]}.{split[1]}(" + passInArgs + ");");
                        //    }
                        //}
                        //
                        //else
                        //{
                        //    if (node.isGenericFunction)
                        //        lines.Add($"{split[0]}.{split[1]}{genericDefine}(" + passInArgs + ");");
                        //    else
                        //        lines.Add($"{split[0]}.{split[1]}(" + passInArgs + ");");
                        //}
                    }
                }

                //Enter here if node is contextual or using a base class method
                else
                {
                    //If you are returning go in here
                    if (node.returnInput != "")
                    {
                        if (node.isContextual)
                        {
                            //Create the lines
                            string prevLine = baseLines[baseLines.Count - 1];
                            string newLine = prevLine + $".{node.input}(" + passInArgs + ")";

                            if (node.isGenericFunction)
                                newLine = prevLine + $".{node.input}{genericDefine}(" + passInArgs + ")";

                            //Add the line
                            baseLines.Add(newLine);
                            lines.Add($"{node.returnInput} = " + newLine + ";");
                        }

                        //Not a contextual node
                        else
                        {
                            if (node.isGenericFunction)
                            {
                                baseLines.Add($"{node.input}{genericDefine}(" + passInArgs + ")");
                                lines.Add($"{node.returnInput} = {node.input}{genericDefine}(" + passInArgs + ");");
                            }
                            else
                            {
                                baseLines.Add($"{node.input}(" + passInArgs + ")");
                                lines.Add($"{node.returnInput} = {node.input}(" + passInArgs + ");");
                            }

                        }
                    }

                    //If not returning
                    else
                    {
                        if (node.isContextual)
                        {
                            string prevLine = baseLines[baseLines.Count - 1];
                            string newLine = "";

                            if (node.isGenericFunction)
                                newLine = prevLine + $".{node.input}{genericDefine}(" + passInArgs + ")";
                            else
                                newLine = prevLine + $".{node.input}(" + passInArgs + ")";

                            baseLines.Add(newLine);

                            //If the next node is not contextual execute the line
                            if (node.nextNode != null)
                            {
                                if (!node.nextNode.isContextual)
                                {
                                    lines.Add(newLine + ";");
                                }
                            }

                            //If the next node does not exist, execute the line
                            else
                            {
                                lines.Add(newLine + ";");
                            }

                        }

                        //If not contextual
                        else
                        {
                            if (node.isGenericFunction)
                            {
                                baseLines.Add($"{node.input}{genericDefine}(" + passInArgs + ")");
                                lines.Add($"{node.input}{genericDefine}(" + passInArgs + ");");
                            }
                            else
                            {
                                baseLines.Add($"{node.input}(" + passInArgs + ")");
                                lines.Add($"{node.input}(" + passInArgs + ");");
                            }
                        }
                    }
                }

            }

            if (node.nodeType == NodeType.Constructor)
            {
                string[] split = node.input.Split(' ');

                string passInArgs = "";
                string genericDefine = "";

                parameters.ReferencedAssemblies.Add(node.assemblyPath);               

                List<Parameter> generics = new List<Parameter>();

                for (int i = 0; i < node.paramList.Count; i++)
                {
                    if (node.paramList[i].isGeneric)
                    {
                        generics.Add(node.paramList[i]);
                        continue;
                    }

                    if (node.paramList[i].inputVar)
                        passInArgs += node.paramList[i].varInput;
                    else
                        passInArgs += GetLiteral(node.paramList[i].arg);

                    if (i < node.paramList.Count - 1)
                        passInArgs += ", ";
                }

                if (generics.Count > 0)
                {
                    genericDefine = "<";

                    for (int i = 0; i < generics.Count; i++)
                    {
                        genericDefine += generics[i].templateType;

                        if (i < generics.Count - 1)
                            genericDefine += ", ";
                    }

                    genericDefine += ">";

                }

                if (split.Length > 1)
                {
                    //string line = $"{node.nameSpace}.{split[0]}(" + passInArgs + ")";
                    //string nameSpace = node.type.Split('.')[0];
                    //string line = $"{nameSpace}.{split[0]}(" + passInArgs + ")";
                    string line = $"{node.type}.{split[0]}(" + passInArgs + ")";

                    if (node.isGenericFunction)
                        line = $"{node.type}.{split[0]}{genericDefine}(" + passInArgs + ")";

                    if (node.returnInput != "")
                    {
                        //string line = $"{node.nameSpace}.{split[0]}(" + passInArgs + ")";
                        baseLines.Add(line);
                        lines.Add($"{node.returnInput} =  new " + line + ";");
                    }

                }
            }

            if (node.nodeType == NodeType.Field_Get)
            {
                string[] split = node.input.Split(' ');

                parameters.ReferencedAssemblies.Add(node.assemblyPath);               
                if (node.isContextual)
                {
                    string prevLine = baseLines[baseLines.Count - 1];
                    string newLine = prevLine + $".{node.input}";
                    baseLines.Add(newLine);

                    if (node.returnInput != "")
                    {
                        lines.Add($"{node.returnInput} = " + newLine + ";");
                    }
                    else
                        lines.Add(newLine + ";");
                }

                else
                {
                    string newLine = "";

                    if (node.isStatic)
                    {
                        //string nameSpace = node.type.Split('.')[0];
                        //newLine = $"{node.nameSpace}.{split[0]}.{split[1]}";
                        //newLine = $"{nameSpace}.{split[0]}.{split[1]}";
                        newLine = $"{node.type}.{split[1]}";
                    }
                    else
                    {
                        if (split.Length > 1)
                            newLine = $"{split[0]}.{split[1]}";
                        else
                            newLine = $"{split[0]}";
                    }

                    baseLines.Add(newLine);

                    if (node.returnInput != "")
                    {
                        lines.Add($"{node.returnInput} = " + newLine + ";");
                    }
                }
            }

            if (node.nodeType == NodeType.Field_Set)
            {
                string[] split = node.input.Split(' ');

                parameters.ReferencedAssemblies.Add(node.assemblyPath);                
                if (node.isContextual)
                {
                    string prevLine = baseLines[baseLines.Count - 1];
                    string newLine = prevLine + $"{node.input}";
                    baseLines.Add(newLine);

                    if (node.paramList[0].inputVar)
                        lines.Add(newLine + $" = {node.paramList[0].varInput};");
                    else
                        lines.Add(newLine + $" = {GetLiteral(node.paramList[0].arg)};");
                }

                else
                {
                    if (node.paramList[0].inputVar)
                    {
                        if (node.isStatic)
                        {
                            //string nameSpace = node.type.Split('.')[0];
                            //lines.Add($"{node.nameSpace}.{split[0]}.{split[1]} = {node.paramList[0].varInput};");
                            //lines.Add($"{nameSpace}.{split[0]}.{split[1]} = {node.paramList[0].varInput};");
                            lines.Add($"{node.type}.{split[1]} = {node.paramList[0].varInput};");
                        }
                        else
                        {
                            if (split.Length > 1)
                                lines.Add($"{split[0]}.{split[1]} = {node.paramList[0].varInput};");
                            else
                                lines.Add($"{split[0]} = {node.paramList[0].varInput};");
                        }
                    }
                    else
                    {
                        if (node.isStatic)
                        {
                            //string nameSpace = node.type.Split('.')[0];
                            //lines.Add($"{node.nameSpace}.{split[0]}.{split[1]} = {GetLiteral(node.paramList[0].arg)};");
                            //lines.Add($"{nameSpace}.{split[0]}.{split[1]} = {GetLiteral(node.paramList[0].arg)};");
                            lines.Add($"{node.type}.{split[1]} = {GetLiteral(node.paramList[0].arg)};");
                        }
                        else
                        {
                            if (split.Length > 1)
                                lines.Add($"{split[0]}.{split[1]} = {GetLiteral(node.paramList[0].arg)};");
                            else
                                lines.Add($"{split[0]} = {GetLiteral(node.paramList[0].arg)};");
                        }
                    }
                }

            }

            if (node.nodeType == NodeType.Property_Get)
            {
                string[] split = node.input.Split(' ');

                parameters.ReferencedAssemblies.Add(node.assemblyPath);
                
                if (node.isContextual)
                {
                    string prevLine = baseLines[baseLines.Count - 1];
                    string newLine = prevLine + $".{node.input}";
                    baseLines.Add(newLine);

                    if (node.returnInput != "")
                        lines.Add($"{node.returnInput} = " + newLine + ";");
                    else
                        lines.Add(newLine + ";");
                }
                
                else
                {
                    string newLine = "";

                    if (node.isStatic)
                    {
                        //string nameSpace = node.type.Split('.')[0];
                        //newLine = $"{node.nameSpace}.{split[0]}.{split[1]}";
                        //newLine = $"{nameSpace}.{split[0]}.{split[1]}";
                        newLine = $"{node.type}.{split[1]}";
                    }
                    else
                    {
                        if (split.Length > 1)
                            newLine = $"{split[0]}.{split[1]}";
                        else
                            newLine = $"{split[0]}";
                    }

                    baseLines.Add(newLine);

                    if (node.returnInput != "")
                    {
                        lines.Add($"{node.returnInput} = " + newLine + ";");
                    }
                }
            }

            if (node.nodeType == NodeType.Property_Set)
            {
                string[] split = node.input.Split(' ');

                parameters.ReferencedAssemblies.Add(node.assemblyPath);
                
                if (node.isContextual)
                {
                    string prevLine = baseLines[baseLines.Count - 1];
                    string newLine = prevLine + $"{node.input}";
                    baseLines.Add(newLine);

                    if (node.paramList[0].inputVar)
                        lines.Add(newLine + $" = {node.paramList[0].varInput};");
                    else
                        lines.Add(newLine + $" = {GetLiteral(node.paramList[0].arg)};");
                }

                else
                {
                    if (node.paramList[0].inputVar)
                    {
                        if (node.isStatic)
                        {
                            //string nameSpace = node.type.Split('.')[0];
                            //lines.Add($"{node.nameSpace}.{split[0]}.{split[1]} = {node.paramList[0].varInput};");
                            //lines.Add($"{nameSpace}.{split[0]}.{split[1]} = {node.paramList[0].varInput};");
                            lines.Add($"{node.type}.{split[1]} = {node.paramList[0].varInput};");
                        }
                        else
                            lines.Add($"{split[0]}.{split[1]} = {node.paramList[0].varInput};");
                    }
                    else
                    {
                        if (node.isStatic)
                        {
                            //string nameSpace = node.type.Split('.')[0];
                            //lines.Add($"{node.nameSpace}.{split[0]}.{split[1]} = {GetLiteral(node.paramList[0].arg)};");
                            //lines.Add($"{nameSpace}.{split[0]}.{split[1]} = {GetLiteral(node.paramList[0].arg)};");
                            lines.Add($"{node.type}.{split[1]} = {GetLiteral(node.paramList[0].arg)};");
                        }
                        else
                            lines.Add($"{split[0]}.{split[1]} = {GetLiteral(node.paramList[0].arg)};");
                    }
                }
            }

            if (node.nodeType == NodeType.Conditional)
            {
                if (node.nextNode != null)
                {
                    if (node.isContextual)
                    {
                        string prevLine = baseLines[baseLines.Count - 1];
                        string newLine = $"if (" + prevLine + ")";
                        lines.Add(newLine);
                        lines.Add("{");
                    }

                    else
                    {
                        if (node.paramList[0].inputVar)
                            lines.Add($"if ({node.paramList[0].varInput})");
                        else
                            lines.Add($"if ({node.paramList[0].arg.ToString()})");
                        lines.Add("{");
                    }

                    //Debug.Log("Parameters in conditional compile right before entering true path");
                    //PrintParameters();

                    List<string> result = FullCompileConditional(node.nextNode, true, parameters);

                    foreach(string line in result)
                    {
                        lines.Add(line);
                    }

                    lines.Add("}");
                }

                if (node.falseNode != null)
                {
                    lines.Add("else");
                    lines.Add("{");

                    //Debug.Log("Parameters in conditional compile right before entering false path");
                    //PrintParameters();

                    List<string> result = FullCompileConditional(node.falseNode, false, parameters);

                    foreach (string line in result)
                    {
                        lines.Add(line);
                    }

                    lines.Add("}");
                }

                break;
            }

            if (node.nodeType == NodeType.Operation)
            {
                string[] split = node.input.Split(' ');

                if (node.isContextual)
                {
                    string prevLine = baseLines[baseLines.Count - 1];

                    if (node.paramList[0].inputVar)
                    {
                        string newLine = prevLine + $" {split[0]} {node.paramList[0].varInput}";
                        baseLines.Add(newLine);

                        //Check if the next node is contextual for parentheses
                        if (node.nextNode != null)
                        {
                            if (node.nextNode.isContextual)
                            {
                                baseLines.Add($"({newLine})");
                            }
                        }
                        if (node.returnInput != "")
                            lines.Add($"{node.returnInput} = " + newLine + ";");
                        //else
                        //    lines.Add(newLine + ";");
                    }

                    else
                    {
                        string newLine = prevLine + $" {split[0]} {GetLiteral(node.paramList[0].arg)}";
                        baseLines.Add(newLine);

                        //Check if the next node is contextual for parentheses
                        if (node.nextNode != null)
                        {
                            if (node.nextNode.isContextual)
                            {
                                baseLines.Add($"({newLine})");
                            }
                        }

                        if (node.returnInput != "")
                            lines.Add($"{node.returnInput} = " + newLine + ";");
                        //else
                        //    lines.Add(newLine + ";");
                    }
                }

                else
                {
                    if (node.paramList[0].inputVar)
                    {
                        string newLine = $"{split[0]} {split[1]} {node.paramList[0].varInput}";
                        baseLines.Add(newLine);

                        if (node.returnInput != "")
                            lines.Add($"{node.returnInput} = " + newLine + ";");
                        else
                            lines.Add(newLine + ";");
                    }

                    else
                    {
                        string newLine = $"{split[0]} {split[1]} {GetLiteral(node.paramList[0].arg)}";
                        baseLines.Add(newLine);

                        if (node.returnInput != "")
                            lines.Add($"{node.returnInput} = " + newLine + ";");
                        else
                            lines.Add(newLine + ";");
                    }
                }
            }

            if (node.nextNode == null)
                break;
            node = node.nextNode;
        }

        //lines.Add("}\n");

        return lines;
    }

    public Action FullCompile_Expression(BlueprintComponent blueprint, string funcName)
    {
        if (blueprint.bp == null)
            return null;

        Node node = blueprint.bp.entryPoints[funcName];
        List<Expression> expressions = new List<Expression>();
        Expression contextTarget = null;
        LabelTarget endLabel = Expression.Label("End");
        
        while (node != null)
        {
            if (node.isEntryPoint)
            {
                node = node.nextNode;
                continue;
            }

            if (node.isContextual)
            {
                if (node.nodeType == NodeType.Function)
                {
                    expressions.Add(CallFunction(blueprint, node, out contextTarget, contextTarget, endLabel));                    
                }

                if (node.nodeType == NodeType.Field_Get)
                {
                    expressions.Add(GetField(blueprint, node, out contextTarget, contextTarget));                   
                }

                if (node.nodeType == NodeType.Field_Set)
                {
                    expressions.Add(SetField(blueprint, node, out contextTarget, contextTarget));                    
                }

                if (node.nodeType == NodeType.Property_Get)
                {
                    expressions.Add(GetProperty(blueprint, node, out contextTarget, contextTarget));                    
                }

                if (node.nodeType == NodeType.Property_Set)
                {
                    expressions.Add(SetProperty(blueprint, node, out contextTarget, contextTarget));                    
                }

                if (node.nodeType == NodeType.Conditional)
                {
                    Expression trueBranch = CompileConditional_Expression(blueprint, node, true, null, endLabel);
                    Expression falseBranch = CompileConditional_Expression(blueprint, node, false, null, endLabel);

                    //Expression check = Expression.IfThenElse(Expression.Convert(contextTarget, typeof(bool)), trueBranch, falseBranch);
                    //expressions.Add(check);
                    Expression check = null;
                    if (falseBranch != null)
                        check = Expression.IfThenElse(Expression.Convert(contextTarget, typeof(bool)), trueBranch, falseBranch);
                    else
                        check = Expression.IfThen(Expression.Convert(contextTarget, typeof(bool)), trueBranch);

                    if (check != null)
                    {
                        expressions.Add(check);
                        break;
                    }
                }

                if (node.nodeType == NodeType.Operation)
                {
                    expressions.Add(CompileOperation(blueprint, node, out contextTarget, contextTarget));
                }

                node = node.nextNode;
                continue;
            }
        
            if (node.nodeType == NodeType.Function)
            {
                expressions.Add(CallFunction(blueprint, node, out contextTarget, null, endLabel));               
            }

            if (node.nodeType == NodeType.Constructor)
            {
                expressions.Add(CallConstructor(blueprint, node, out contextTarget));                
            }

            if (node.nodeType == NodeType.Field_Get)
            {
                expressions.Add(GetField(blueprint, node, out contextTarget));                
            }

            if (node.nodeType == NodeType.Field_Set)
            {
                expressions.Add(SetField(blueprint, node, out contextTarget));                               
            }

            //Make Property access here

            if (node.nodeType == NodeType.Property_Get)
            {
                expressions.Add(GetProperty(blueprint, node, out contextTarget));                
            }

            if (node.nodeType == NodeType.Property_Set)
            {
                expressions.Add(SetProperty(blueprint, node, out contextTarget));               
            }

            if (node.nodeType == NodeType.Conditional)
            {
                Expression trueBranch = CompileConditional_Expression(blueprint, node, true, null, endLabel);
                Expression falseBranch = CompileConditional_Expression(blueprint, node, false, null, endLabel);

                //Expression check = Expression.IfThenElse(Expression.Convert(GetVariable(blueprint, (string)node.paramList[0].arg), typeof(bool)), trueBranch, falseBranch);
                //expressions.Add(check);
                Expression check = null;
                if (falseBranch != null)
                    check = Expression.IfThenElse(Expression.Convert(GetVariable(blueprint, (string)node.paramList[0].arg), typeof(bool)), trueBranch, falseBranch);
                else
                    check = Expression.IfThen(Expression.Convert(GetVariable(blueprint, (string)node.paramList[0].arg), typeof(bool)), trueBranch);

                if (check != null)
                {
                    expressions.Add(check);
                    break;
                }
            }

            if (node.nodeType == NodeType.Operation)
            {
                expressions.Add(CompileOperation(blueprint, node, out contextTarget));
            }

            if (node.nextNode == null)
                break;
            node = node.nextNode;
        }

        //End While
        if (expressions.Count > 0)
        {
            expressions.Add(Expression.Label(endLabel));

            Expression final = Expression.Block(expressions.ToArray());

            var func = Expression.Lambda<Action>(final, new ParameterExpression[] { });
            
            return func.Compile();                                                
        }

        return null;
    }

    //Printing in lambda
    //lastExpression = assignVar;
    //MethodInfo debugLog = typeof(MonoBehaviour).GetMethod("print");
    //PropertyInfo nameProp = typeof(Transform).GetProperty("name");
    //
    //Expression convert = Expression.Convert(objAccess, nameProp.DeclaringType);
    //
    //Expression getName = Expression.Property(convert, nameProp);
    //
    //Expression checkAssign = Expression.Call(debugLog, getName);
    //
    //expressions.Add(Expression.Block(assignVar, checkAssign));        
    Expression CompileOperation(BlueprintComponent blueprint, Node node, out Expression contextTarget, Expression myTarget = null)
    {
        //Notes on the assignment operators, it seems that we just shouldn't use them and just do Expression.Assign(x, Expression.Add(y))

        Expression target = null;
        Expression assignTarget = null;

        if (myTarget == null)
        {
            assignTarget = (MemberExpression)GetVariable(blueprint, node.varName);
            target = Expression.Convert(assignTarget, blueprint.variables[node.varName].type);            
        }

        else
        {
            target = myTarget;
            assignTarget = myTarget;
        }

        MethodInfo overload = null;
        Expression result = null;
        contextTarget = null;

        if (node.operatorMethodName != "")
        {
            overload = LoadMethod(node.operatorMethodName, node.type, node.assemblyPath, node.index, node.isContextual);
        }

        Expression GetSetArg(int i)
        {
            Expression setArg = null;
            Node myNode = node;
            if (myNode.paramList[i].inputVar)
            {                
                if (!myNode.isContextual || overload != null)
                    //setArg = Expression.Convert(GetVariable(blueprint, myNode.paramList[i].varInput), blueprint.variables[myNode.varName].type);
                    setArg = Expression.Convert(GetVariable(blueprint, myNode.paramList[i].varInput), myNode.paramList[i].type);
                else
                    setArg = Expression.Convert(GetVariable(blueprint, myNode.paramList[i].varInput), target.Type);
            }
            else
            {
                if (!myNode.isContextual || overload != null)
                    //setArg = Expression.Convert(Expression.Constant(myNode.paramList[i].arg), blueprint.variables[myNode.varName].type);
                    setArg = Expression.Convert(Expression.Constant(myNode.paramList[i].arg), myNode.paramList[i].type);
                else
                    setArg = Expression.Convert(Expression.Constant(myNode.paramList[i].arg), target.Type);
            }


            return setArg;
        }

        switch (node.operatorStr)
        {
            case "=":
                Expression setArg = null;
                if (node.paramList[0].inputVar)                    
                    setArg = GetVariable(blueprint, node.paramList[0].varInput);
                else                    
                    setArg = Expression.Convert(Expression.Constant(node.paramList[0].arg), typeof(object));

                result = Expression.Assign(assignTarget, setArg);                
                break;

            //case "+=":
            //    if (overload != null)
            //        result = Expression.AddAssign(target, GetSetArg(1), overload);
            //    else
            //    {
            //        //result = Expression.AddAssign(target, GetSetArg(0));
            //        Expression testTarget = target;
            //        Expression add = Expression.Add(target, GetSetArg(0));
            //        if (!node.isContextual)
            //            result = Expression.Assign(assignTarget, Expression.Convert(add, typeof(object)));
            //        else
            //            result = Expression.Assign(target, add);
            //    }
            //     
            //    break;
            //
            //case "-=":
            //    //if (overload != null)
            //    //    result = Expression.SubtractAssign(target, GetSetArg(1), overload);
            //    //else
            //    //    result = Expression.SubtractAssign(target, GetSetArg(0));                
            //    //break;
            //    if (overload != null)
            //        result = Expression.SubtractAssign(target, GetSetArg(1), overload);
            //    else
            //    {
            //        //result = Expression.AddAssign(target, GetSetArg(0));                                                        
            //        Expression sub = Expression.Subtract(target, GetSetArg(0));
            //        if (!node.isContextual)
            //            result = Expression.Assign(assignTarget, Expression.Convert(sub, typeof(object)));
            //        else
            //            result = Expression.Assign(target, sub);
            //    }
            //    break;
            //
            //case "*=":
            //    if (overload != null)
            //        result = Expression.MultiplyAssign(target, GetSetArg(1), overload);
            //    else
            //        result = Expression.MultiplyAssign(target, GetSetArg(0));
            //    break;
            //
            //case "/=":
            //    if (overload != null)
            //        result = Expression.DivideAssign(target, GetSetArg(1), overload);
            //    else
            //        result = Expression.DivideAssign(target, GetSetArg(0));
            //    break;
            //
            //case "%=":
            //    if (overload != null)
            //        result = Expression.ModuloAssign(target, GetSetArg(1), overload);
            //    else
            //        result = Expression.ModuloAssign(target, GetSetArg(0));
            //    break;
            
            case "++":
                if (overload != null)
                    result = Expression.Increment(target, overload);
                else
                    result = Expression.Increment(target);
                break;

            case "--":
                if (overload != null)
                    result = Expression.Decrement(target, overload);
                else
                    result = Expression.Decrement(target);
                break;

            //Standard Operations
            case "+":
                if (overload != null)
                    result = Expression.Add(target, GetSetArg(0), overload);
                else
                    result = Expression.Add(target, GetSetArg(0));
                contextTarget = result;                
                break;

            case "-":
                if (overload != null)
                    result = Expression.Subtract(target, GetSetArg(0), overload);
                else
                    result = Expression.Subtract(target, GetSetArg(0));
                contextTarget = result;                
                break;

            case "*":
                if (overload != null)
                {
                    Expression testTarget = target;
                    Expression arg = GetSetArg(0);
                    //result = Expression.Multiply(target, GetSetArg(0), overload);
                    result = Expression.Multiply(testTarget, arg, overload);
                }
                else
                    result = Expression.Multiply(target, GetSetArg(0));
                contextTarget = result;
                break;

            case "/":
                if (overload != null)
                    result = Expression.Divide(target, GetSetArg(0), overload);
                else
                    result = Expression.Divide(target, GetSetArg(0));
                contextTarget = result;
                break;

            case "%":
                if (overload != null)
                    result = Expression.Modulo(target, GetSetArg(0), overload);
                else
                    result = Expression.Modulo(target, GetSetArg(0));
                contextTarget = result;
                break;
           
            //Conditionals
            case "==":
                if (overload != null)
                    result = Expression.Equal(target, GetSetArg(0), false, overload);
                else
                    result = Expression.Equal(target, GetSetArg(0));
                contextTarget = result;
                break;

            case "<":
                if (overload != null)
                    result = Expression.LessThan(target, GetSetArg(0), false, overload);
                else
                    result = Expression.LessThan(target, GetSetArg(0));
                contextTarget = result;
                break;

            case ">":
                if (overload != null)
                    result = Expression.GreaterThan(target, GetSetArg(0), false, overload);
                else
                    result = Expression.GreaterThan(target, GetSetArg(0));
                contextTarget = result;
                break;

            case "<=":
                if (overload != null)
                    result = Expression.LessThanOrEqual(target, GetSetArg(0), false, overload);
                else
                    result = Expression.LessThanOrEqual(target, GetSetArg(0));
                contextTarget = result;
                break;

            case ">=":
                if (overload != null)
                    result = Expression.GreaterThanOrEqual(target, GetSetArg(0), false, overload);
                else
                    result = Expression.GreaterThanOrEqual(target, GetSetArg(0));
                contextTarget = result;
                break;

            case "!=":
                if (overload != null)
                    result = Expression.NotEqual(target, GetSetArg(0), false, overload);
                else
                    result = Expression.NotEqual(target, GetSetArg(0));
                contextTarget = result;
                break;

            case "&&":
                if (overload != null)
                    result = Expression.AndAlso(target, GetSetArg(0), overload);
                else
                    result = Expression.AndAlso(target, GetSetArg(0), overload);
                contextTarget = result;
                break;

            case "||":
                if (overload != null)
                    result = Expression.OrElse(target, GetSetArg(0), overload);
                else
                    result = Expression.OrElse(target, GetSetArg(0), overload);
                contextTarget = result;
                break;
        }

        if (node.returnInput != "")
        {
            Expression objAccess = GetVariable(blueprint, node.returnInput);
            Expression assignVar = Expression.Assign(objAccess, Expression.Convert(result, typeof(object)));
            return assignVar;
        }

        //if (node.isContextual)
        //{
        //    //Expression assignVar = Expression.Assign(Expression.Convert(assignTarget, typeof(object)), Expression.Convert(result, typeof(object)));
        //    Expression assignVar = Expression.Assign(target, Expression.Convert(result, typeof(object)));
        //    return assignVar;
        //}

        /*
         IMPORTANT NOTES:
         IF A VARIABLE IS BEING RETURNED TO IN PREVIOUS NODE, THAT WILL BE THE RESULT OF THE CONTEXTUAL NODE
         IF YOU TRULY WANT THE PROPERTY BEING ACCESSED LEAVE BLANK
         */
        
        return result;
    }

    Expression CompileConditional_Expression(BlueprintComponent blueprint, Node node, bool truePath, Expression target = null, LabelTarget endLabel = null)
    {
        List<Expression> expressions = new List<Expression>();

        Expression contextTarget = null;

        bool init = false;

        if (target != null)
            contextTarget = target;
                
        while (node != null)
        {
            //No recursion bombs on start
            if (!init)
            {
                if (node.nodeType == NodeType.Conditional)
                {
                    if (truePath)
                    {
                        if (node.nextNode == null)
                            break;
                        node = node.nextNode;
                        init = true;
                    }

                    else
                    {
                        if (node.falseNode == null)
                            break;
                        node = node.falseNode;
                        init = true;
                    }
                    continue;
                }
            }

            if (node.isContextual)
            {
                if (node.nodeType == NodeType.Function)                
                    expressions.Add(CallFunction(blueprint, node, out contextTarget, contextTarget, endLabel));
                
                if (node.nodeType == NodeType.Field_Get)                
                    expressions.Add(GetField(blueprint, node, out contextTarget, contextTarget));
                
                if (node.nodeType == NodeType.Field_Set)                
                    expressions.Add(SetField(blueprint, node, out contextTarget, contextTarget));
                
                if (node.nodeType == NodeType.Property_Get)                
                    expressions.Add(GetProperty(blueprint, node, out contextTarget, contextTarget));
                
                if (node.nodeType == NodeType.Property_Set)                
                    expressions.Add(SetProperty(blueprint, node, out contextTarget, contextTarget));

                if (node.nodeType == NodeType.Operation)                
                    expressions.Add(CompileOperation(blueprint, node, out contextTarget, contextTarget));
                

                if (node.nodeType == NodeType.Conditional)
                {
                    Expression trueBranch = CompileConditional_Expression(blueprint, node, true, null, endLabel);
                    Expression falseBranch = CompileConditional_Expression(blueprint, node, false, null, endLabel);
                    Expression check = null;
                    if (falseBranch != null)
                        check = Expression.IfThenElse(Expression.Convert(contextTarget, typeof(bool)), trueBranch, falseBranch);
                    else
                        check = Expression.IfThen(Expression.Convert(contextTarget, typeof(bool)), trueBranch);

                    if (check != null)
                        expressions.Add(check);
                }

                if (truePath)
                {
                    if (node.nextNode == null)
                        break;
                    node = node.nextNode;
                }

                else
                {
                    if (node.falseNode == null)
                        break;
                    node = node.falseNode;
                }
                continue;
            }

            if (node.nodeType == NodeType.Function)                
                expressions.Add(CallFunction(blueprint, node, out contextTarget, null, endLabel));

            if (node.nodeType == NodeType.Constructor)
                expressions.Add(CallConstructor(blueprint, node, out contextTarget));

            if (node.nodeType == NodeType.Field_Get)
                expressions.Add(GetField(blueprint, node, out contextTarget));

            if (node.nodeType == NodeType.Field_Set)
                expressions.Add(SetField(blueprint, node, out contextTarget));

            if (node.nodeType == NodeType.Property_Get)
                expressions.Add(GetProperty(blueprint, node, out contextTarget));

            if (node.nodeType == NodeType.Property_Set)
                expressions.Add(SetProperty(blueprint, node, out contextTarget));

            if (node.nodeType == NodeType.Operation)            
                expressions.Add(CompileOperation(blueprint, node, out contextTarget));
            

            if (node.nodeType == NodeType.Conditional)
            {
                Expression trueBranch = CompileConditional_Expression(blueprint, node, true, null, endLabel);
                Expression falseBranch = CompileConditional_Expression(blueprint, node, false, null, endLabel);

                //Expression check = Expression.IfThenElse(GetVariable(blueprint, (string)node.paramList[0].arg), trueBranch, falseBranch);
                //expressions.Add(check);
                Expression check = null;
                if (falseBranch != null)
                    check = Expression.IfThenElse(Expression.Convert(contextTarget, typeof(bool)), trueBranch, falseBranch);
                else
                    check = Expression.IfThen(Expression.Convert(contextTarget, typeof(bool)), trueBranch);

                if (check != null)
                    expressions.Add(check);

            }

            if (truePath)
            {
                if (node.nextNode == null)
                    break;
                node = node.nextNode;
            }

            else
            {
                if (node.falseNode == null)
                    break;
                node = node.falseNode;
            }
        }

        if (expressions.Count > 0)
        {
            return Expression.Block(expressions.ToArray());
        }

        else
            return null;
        
    }

    Expression CallFunction(BlueprintComponent blueprint, Node node, out Expression contextTarget, Expression target = null, LabelTarget endLabel = null)
    {
        //Get the function target --NEEDS TO BE AN EXPRESSION
        if (node.isSpecial && !node.hasCost)
        {
            target = Expression.Constant(blueprint);
            node.currentMethod = GetSpecialFunction(node.input, false);
        }

        else if (node.hasCost && !node.isSpecial)
        {
            target = null;
            node.currentMethod = GetSpecialFunction(node.input, node.hasCost, node.index);
        }

        else
        {
            //Load Method
            node.currentMethod = LoadMethod(node.input, node.type, node.assemblyPath, node.index, node.isContextual);
            
            if (target == null && !node.currentMethod.IsStatic)
            {
                Expression temp = (MemberExpression)GetVariable(blueprint, node.varName);
                target = Expression.Convert(temp, node.currentMethod.DeclaringType);
            }
           
        }
        List<Expression> argExpressions = new List<Expression>();

        ParameterInfo[] pars = node.currentMethod.GetParameters();

        //Determine which parameters are literals and which are calling functions
        for (int i = 0; i < node.paramList.Count; i++)
        {
            if (pars[i].ParameterType == typeof(BlueprintComponent))
            {
                argExpressions.Add(Expression.Constant(blueprint));
                continue;
            }

            if (node.paramList[i].inputVar)
                argExpressions.Add(Expression.Convert(GetVariable(blueprint, node.paramList[i].varInput), pars[i].ParameterType));

            else
                argExpressions.Add(Expression.Convert(Expression.Constant(node.paramList[i].arg), pars[i].ParameterType));
        }

        //Prep arguments                 
        Expression[] args = argExpressions.ToArray();

        //MethodCall apparently DOES not like Field Expressions but is fine with MemberExpressions
        MethodCallExpression call = Expression.Call
        (
            target,
            node.currentMethod,
            args
        );
        
        //If the function is returning set that to be the 
        if (node.returnInput != "")
        {
            Expression objAccess = GetVariable(blueprint, node.returnInput);
            Expression assignVar = Expression.Assign(objAccess, Expression.Convert(call, typeof(object)));

            if (node.hasCost)
            {
                Expression statusAccess = Expression.Field(Expression.Constant(blueprint), typeof(BlueprintComponent).GetField("status"));

                Expression check = Expression.Equal(statusAccess, Expression.Constant(ActionStatus.Failure));

                Expression eject = Expression.IfThen(check, Expression.Return(Expression.Label()));

                contextTarget = objAccess;
                return Expression.Block(assignVar, eject);

            }

            contextTarget = objAccess;
            return assignVar;

        }

        else
        {
            if (node.hasCost)
            {
                Expression statusAccess = Expression.Field(Expression.Constant(blueprint), typeof(BlueprintComponent).GetField("status"));

                Expression check = Expression.Equal(statusAccess, Expression.Constant(ActionStatus.Failure));

                MethodInfo debugLog = typeof(MonoBehaviour).GetMethod("print");

                var debug = Expression.Call(debugLog, Expression.Constant("Returning to end label"));

                Expression eject = Expression.IfThen(check, Expression.Block(debug, Expression.Return(endLabel)));

                contextTarget = call;
                return Expression.Block(call, eject);

            }
            
            contextTarget = call;
            return call;
        }
    }

    Expression CallConstructor(BlueprintComponent blueprint, Node node, out Expression contextTarget, Expression target = null)
    {
        //Load Method
        node.constructorMethod = LoadConstructor(node.input, node.type, node.assemblyPath, node.index, node.isContextual);
        //Expression temp = (MemberExpression)GetVariable(blueprint, node.varName);
        //target = Expression.Convert(temp, node.currentMethod.DeclaringType);

        List<Expression> argExpressions = new List<Expression>();

        ParameterInfo[] pars = node.constructorMethod.GetParameters();

        //Determine which parameters are literals and which are calling functions
        for (int i = 0; i < node.paramList.Count; i++)
        {
            if (node.paramList[i].inputVar)
                argExpressions.Add(Expression.Convert(GetVariable(blueprint, node.paramList[i].varInput), pars[i].ParameterType));

            else
                argExpressions.Add(Expression.Convert(Expression.Constant(node.paramList[i].arg), pars[i].ParameterType));
        }

        //Prep arguments                 
        Expression[] args = argExpressions.ToArray();

        //MethodCall apparently DOES not like Field Expressions but is fine with MemberExpressions
        Expression call = Expression.New(node.constructorMethod, args);

        //If the function is returning set that to be assigned
        if (node.returnInput != "")
        {
            Expression objAccess = GetVariable(blueprint, node.returnInput);
            Expression assignVar = Expression.Assign(objAccess, Expression.Convert(call, typeof(object)));
            //expressions.Add(assignVar);
            contextTarget = objAccess;
            return assignVar;
        }

        else
        {
            //expressions.Add(call);
            contextTarget = call;
            return call;
        }
    }

    Expression GetField(BlueprintComponent blueprint, Node node, out Expression contextTarget, Expression target = null)
    {
        node.fieldVar = LoadField(node.input, node.type, node.assemblyPath, node.isContextual);
        
        if (!node.fieldVar.IsStatic)
        {
            if (target == null)
            {
                Expression temp = (MemberExpression)GetVariable(blueprint, node.varName);
                target = Expression.Convert(temp, node.fieldVar.DeclaringType);
            }
        }

        Expression access = Expression.Field(target, node.fieldVar);

        if (node.returnInput != "")
        {
            Expression objAccess = GetVariable(blueprint, node.returnInput);
            Expression assignVar = Expression.Assign(objAccess, Expression.Convert(access, typeof(object)));

            //expressions.Add(assignVar);
            contextTarget = objAccess;
            return assignVar;
        }

        else
        {
            //expressions.Add(access);
            contextTarget = access;
            return access;
        }
    }

    Expression SetField(BlueprintComponent blueprint, Node node, out Expression contextTarget, Expression target = null)
    {
        node.fieldVar = LoadField(node.input, node.type, node.assemblyPath, node.isContextual);
        
        if (!node.fieldVar.IsStatic)
        {
            if (target == null)
            {
                Expression temp = (MemberExpression)GetVariable(blueprint, node.varName);
                target = Expression.Convert(temp, node.fieldVar.DeclaringType);
            }
        }

        Expression setArg = null;
        if (node.paramList[0].inputVar)
            setArg = Expression.Convert(GetVariable(blueprint, node.paramList[0].varInput), node.fieldVar.FieldType);
        else
            setArg = Expression.Convert(Expression.Constant(node.paramList[0].arg), node.fieldVar.FieldType);

        Expression access = Expression.Field(target, node.fieldVar);

        Expression assign = Expression.Assign(access, setArg);

        //expressions.Add(assign);
        contextTarget = access; //Should this be allowed?!
        return assign;
    }

    Expression GetProperty(BlueprintComponent blueprint, Node node, out Expression contextTarget, Expression target = null)
    {
        node.propertyVar = LoadProperty(node.input, node.type, node.assemblyPath, node.isContextual);
        
        if (!node.isStatic)
        {
            if (target == null)
            {
                Expression temp = (MemberExpression)GetVariable(blueprint, node.varName);
                target = Expression.Convert(temp, node.propertyVar.DeclaringType);
            }
        }
        
        Expression access = Expression.Property(target, node.propertyVar);

        if (node.returnInput != "")
        {            
            Expression objAccess = GetVariable(blueprint, node.returnInput);
            Expression assignVar = Expression.Assign(objAccess, Expression.Convert(access, typeof(object)));

            //expressions.Add(assignVar);
            contextTarget = objAccess;
            return assignVar;
        }

        else
        {
            //expressions.Add(access);
            contextTarget = access;
            return access;
        }
    }

    Expression SetProperty(BlueprintComponent blueprint, Node node, out Expression contextTarget, Expression target = null)
    {
        node.propertyVar = LoadProperty(node.input, node.type, node.assemblyPath, node.isContextual);
        
        if (!node.isStatic)
        {
            if (target == null)
            {
                Expression temp = (MemberExpression)GetVariable(blueprint, node.varName);
                target = Expression.Convert(temp, node.propertyVar.DeclaringType);
            }
        }

        Expression setArg = null;
        if (node.paramList[0].inputVar)
            setArg = Expression.Convert(GetVariable(blueprint, node.paramList[0].varInput), node.propertyVar.PropertyType);
        else
            setArg = Expression.Convert(Expression.Constant(node.paramList[0].arg), node.propertyVar.PropertyType);

        Expression access = Expression.Property(target, node.propertyVar);

        Expression assign = Expression.Assign(access, setArg);

        //expressions.Add(assign);
        //lastExpression = assign;
        contextTarget = access;
        return assign;
    }

    Expression GetVariable(BlueprintComponent blueprint, string varName)
    {
        //Access the field named variables
        //Expression accessDictionary = Expression.Field(variables, "variables");

        //Source: https://stackoverflow.com/questions/3085955/how-do-i-access-a-dictionary-item-using-linq-expressions

        //Make the key
        Expression key = Expression.Constant(varName);

        //Access dictionary
        Expression dictAccess = Expression.Property(Expression.Constant(blueprint.variables), "Item", key);

        //Access the true object
        Expression objAccess = Expression.Field(dictAccess, typeof(Var).GetField("obj"));

        return objAccess;
        //return Expression.Constant(objAccess);
    }

    Type GetVariableType(BlueprintComponent blueprint, string varName)
    {
        return blueprint.variables[varName].type;
    }

    //Expression Getters --------------------------

    public Expression NonVoidInstanceMethodExpression(MethodInfo method)
    {
        ParameterExpression instanceParameter = Expression.Parameter(typeof(object), "target");
        ParameterExpression argumentsParameter = Expression.Parameter(typeof(object[]), "arguments");

        MethodCallExpression call = Expression.Call(
           Expression.Convert(instanceParameter, method.DeclaringType),
           method,
           CreateMethodParameters(method, argumentsParameter));

        return call;

        //Expression<Func<object, object[], object>> lambda = Expression.Lambda<Func<object, object[], object>>(
        //    Expression.Convert(call, typeof(object)),
        //    instanceParameter,
        //    argumentsParameter);

        //return lambda;
    }

    public Expression NonVoidStaticMethodExpression(MethodInfo method)
    {
        ParameterExpression argumentsParameter = Expression.Parameter(typeof(object[]), "arguments");

        MethodCallExpression call = Expression.Call(
            method,
            CreateMethodParameters(method, argumentsParameter));

        Expression<Func<object[], object>> lambda = Expression.Lambda<Func<object[], object>>(
            Expression.Convert(call, typeof(object)),
            argumentsParameter);

        return lambda;
    }

    public Expression VoidInstanceMethodExpression(MethodInfo method)
    {
        ParameterExpression instanceParameter = Expression.Parameter(typeof(object), "target");
        ParameterExpression argumentsParameter = Expression.Parameter(typeof(object[]), "arguments");

        MethodCallExpression call = Expression.Call(
            Expression.Convert(instanceParameter, method.DeclaringType),
            method,
            CreateMethodParameters(method, argumentsParameter));

        Expression<Action<object, object[]>> lambda = Expression.Lambda<Action<object, object[]>>(
            call,
            instanceParameter,
            argumentsParameter);

        return lambda;
    }

    public Expression VoidStaticMethodExpression(MethodInfo method)
    {
        ParameterExpression argumentsParameter = Expression.Parameter(typeof(object[]), "arguments");

        MethodCallExpression call = Expression.Call(
            method,
            CreateMethodParameters(method, argumentsParameter));

        Expression<Action<object[]>> lambda = Expression.Lambda<Action<object[]>>(
            call,
            argumentsParameter);

        return lambda;
    }

    //End Expression Getters ----------------------

    //OLD UNUSED STUFF DONE BELOW KEEPING FOR REFERENCE/POSTERITY ---------------------------------------

    //WARNING THIS CANNOT FIND A FUNCTION IF IT IS FROM A BASE CLASS, ONLY IF THE CLASS DIRECTLY CONTAINS IT
    public MethodInfo[] GetFunctionDefinitions(string text, out string typeStr, out string asmPath)
    {        
        string[] raw = text.Split(' ');
        
        if (raw.Length <= 1)
        {
            typeStr = "";
            asmPath = "";            
            return null;
        }

        string name = raw[1];        
        
        Type type = null;
        Assembly ASM = null;
        int matchCount = 0;

        //FindType(raw[0], out type, out ASM);

        //WARNING, The only reason this finds the UnityEngine version is because it happens to be second and is overwriting System.Object
        foreach (Assembly asm in AppDomain.CurrentDomain.GetAssemblies())
        {
            Type[] types = asm.GetTypes();
        
            foreach (Type t in types)
            {
                if (t.Name == raw[0])
                {
                    type = t;
                    ASM = asm;
                    matchCount++;
                }
            }
        }
        
        if (type == null)
        {
            Debug.Log("Interpreter Error: No Component or Class defintion Found");
            typeStr = "";
            asmPath = "";            
            return null;
        }

        Debug.Log($"Match count for this node is {matchCount}");
        
        typeStr = type.ToString();
        asmPath = ASM.Location;
                              
        List<MethodInfo> target = new List<MethodInfo>();
        MethodInfo[] methods = type.GetMethods();

        //Assembly asm = type.Assembly;        
        //string test = type.ToString();

        //Sample testing: DOES WORK, JUST REMEMBER THIS WAS TESTING WHATEVER IS FIRST IN METHODS
        //if (methods.Length > 0)
        //{
        //    int token = methods[0].MetadataToken;
        //    Module mod = methods[0].Module;
        //    MethodBase getBack = mod.ResolveMethod(token);            
        //    Debug.Log($"Sample Test: {getBack.Name} + {getBack.ReflectedType} ");
        //}        

        //End testing

        foreach (MethodInfo m in methods)
        {
            if (m.Name == name)
                target.Add(m);

            if (name == "DebugList")
            {
                Debug.Log($"{type} {m.Name}");
            }            
        }
      
        if (target.Count == 0)
        {
            Debug.Log("Interpreter Warning: No definition found, the function may be in a base class");
        }
        
        return target.ToArray();
      
    }

    public object FindFields(string text, BlueprintData data)
    {        
        string[] raw = text.Split(' ');

        string member = raw[1];

        Type type = null;
        Assembly ASM = null;

        //FindType(raw[0], out type, out ASM);

        if (type == null)
        {
            Debug.Log("Interpreter Error: No Component or Class defintion Found");
            return null;
        }
                  
        FieldInfo[] members = type.GetFields();

        foreach(FieldInfo info in members)
        {
            if (info.Name == raw[1])
            {
                //return info.GetValue(variable);
            }
        }

        //typeStr = type.ToString();
        //asmPath = ASM.Location;


        return null;
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

