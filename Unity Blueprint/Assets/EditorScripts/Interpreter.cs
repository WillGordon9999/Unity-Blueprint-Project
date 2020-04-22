using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Reflection;
using System.Linq;
using System.Linq.Expressions;
using System;
using System.IO;

[Serializable]
public class Var
{
    public string name;
    public ParameterData data; //This will be primarily for initial values, I don't think I should worry about this now
    public string input;
    public string strType;
    public string asmPath;

    public enum VarType { Field, Property };
    public VarType varType;

    //To be set up during runtime
    public object obj; //For safety this should probably be the containing object of the property/field
    public Type type; //Type of class     
    //public FieldInfo field;
    //public PropertyInfo property;
   
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

    public bool isKeyWord;
    public bool isStatic; //If class name is called directly and var is not used    
    public string varName; //The target
    public Var varRef;

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
    
    //This is only for processing code nodes, defining variables should be handled by CreateVariable
    public void Compile(string input, BlueprintData blueprint, ref InterpreterData data, Node node = null)
    {
        if (input == null)
            return;

        string[] args = input.Split(' ');

        if (args.Length < 1)
            return;

        if (data != null)
        {
            if (input != data.input)
            {
                data = null;
                Debug.Log($"Resetting interpreter data input is {input}");                
            }
        }

        if (data == null)
        {
            data = new InterpreterData();
            data.input = input;

            if (node.isContextual)
            {
                if (node.prevNode != null && node.prevNode.isReturning)
                {
                    data.selectedType = node.prevNode.returnType;
                    data.selectedAsm = data.selectedType.Assembly;                    
                }
            }

            else
            { 
                if (ParseKeywords(args[0], node))
                {
                    return;
                }

                Type varType = null;

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
        
        if (data.selectedType != null)
        {           
            MethodInfo[] methods = data.selectedType.GetMethods();
            List<MethodInfo> methodInfo = new List<MethodInfo>();

            string name = "";

            if (node.isContextual)
                name = args[0];
            else
                name = args[1];

            foreach (MethodInfo m in methods)
            {
                 if (data.isStatic)
                 {
                     if (m.Name == name && m.IsStatic)
                         methodInfo.Add(m);
                 }
                 else
                 {
                     if (m.Name == name && !m.IsStatic)
                         methodInfo.Add(m);
                 }                              
            }

            data.methods = methodInfo.ToArray();
            
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
                FieldInfo result = data.selectedType.GetField(name);
                if (result != null && result.IsPublic)
                {
                    data.fields[0] = result;
                    data.access = InterpreterData.AccessType.Both;
                }

                data.properties = new PropertyInfo[1];                
                PropertyInfo prop = data.selectedType.GetProperty(name);

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
                FieldInfo result = data.selectedType.GetField(name);

                if (result != null && result.IsPublic)
                {
                    data.fields[0] = result;
                    data.access = InterpreterData.AccessType.Both;
                }

                data.properties = new PropertyInfo[1];
                PropertyInfo prop = data.selectedType.GetProperty(name);

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
        }                
    }

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
                //Debug.Log("Attempting to construct methodinfo");
                node.currentMethod = LoadMethod(node.input, node.type, node.assemblyPath, node.index);

                if (node.currentMethod == null)
                {
                    Debug.Log("Node has no method or is entry point");
                    return;
                }
            }

            if (node.paramList.Count > 0)
            {
                node.passArgs = new object[node.paramList.Count];

                for (int i = 0; i < node.paramList.Count; i++)
                {
                    if (node.isSpecial && node.paramList[i].noType)
                    {
                        node.passArgs[i] = node.actualTarget;
                        node.actualTarget = null;
                        continue;
                    }

                    node.passArgs[i] = node.paramList[i].arg;
                }
            }
            
            if (target != null)
            {
                //Debug.Log("passed in target is not null");
                node.actualTarget = target;
            }

            node.function = node.currentMethod.Bind();
            return;
        }

        if (node.nodeType == NodeType.Field_Set)
        {
            //Find the base type
            node.fieldVar = LoadField(node.input, node.type, node.assemblyPath);

            if (node.literalField != null && node.varField == null)
            {
                node.isVar = false;
                //node.targetVar = blueprint.variables[node.varName];
                //node.SetVariable = delegate { node.fieldVar.SetValue(node.targetVar.obj, node.literalField.arg); };
            }

            if (node.varField != null && node.literalField == null)
            {
                if ((string)node.varField.arg != "")
                {
                    node.isVar = true;
                }
            }

            if (node.literalField != null && node.varField != null)
            {
                if ((string)node.varField.arg != "")
                    Debug.LogError("ERROR: Both types assigned");
            }
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

        if (isContextual && args.Length == 1)
            name = args[0];

        else if (args.Length > 1 && !isContextual)
            name = args[1];
        else
        {
            Debug.Log("No function name found returning null");
            return null;
        }

        if (TestASM != null)
        {
            Type typeTest = TestASM.GetType(type);
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

    public Type LoadVarType(string type, string asmPath)
    {
        if (asmPath == "")
            return null;

        Assembly TestASM = Assembly.LoadFile(asmPath);
        
        if (TestASM != null)
        {
            return TestASM.GetType(type);
        }

        return null;
    }

    public FieldInfo LoadField(string input, string type, string path)
    {
        if (path == "")
            return null;

        Assembly TestASM = Assembly.LoadFile(path);

        string[] args = input.Split(' ');
        string name = "";

        if (args.Length > 1)
            name = args[1];
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

    public PropertyInfo LoadProperty(string input, string type, string path)
    {
        if (path == "")
            return null;

        Assembly TestASM = Assembly.LoadFile(path);

        string[] args = input.Split(' ');
        string name = "";

        if (args.Length > 1)
            name = args[1];
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

    public bool ParseKeywords(string text, Node node)
    {
        MethodInfo info = null;
        //MethodInfo[] infos;
        //List<MethodInfo> methods;

        try
        {
            info = typeof(BlueprintComponent).GetMethod(text);
        }

        catch (AmbiguousMatchException e)
        {            
            if (text == "GetComponent")
            {                
                info = typeof(BlueprintComponent).GetMethod(text, new Type[] { typeof(string), typeof(string) });
            }
        }
        
        if (info != null)
        {
            if (info.Name == "GetComponent")
            {
                node.isSpecial = true;
                //node.ChangeToMethod(info);
                node.ChangeToSpecialMethod(info);
                return true;
            }
        
            Debug.Log($"Method found for {text}");
            
            node.isDefined = true;
            node.isEntryPoint = true;
            return true;
        }

        if (text == "Branch")
        {
            node.ChangeToConditional();
            node.isDefined = true;
            return true;
        }

        return false;

        //string[] args = text.Split(' ');
        //
        //if (info != null && info.IsStatic)
        //{
        //    if (args.Length > 1)
        //        node.ChangeToSpecialMethod(info, args[1], text);            
        //}
    }

    public MethodInfo GetSpecialFunction(string input)
    {
        MethodInfo info = null;

        if (input == "GetComponent")
        {
            info = typeof(BlueprintComponent).GetMethod(input, new Type[] { typeof(string), typeof(string) });
        }

        //try
        //{
        //    info = typeof(BlueprintComponent).GetMethod(input);
        //}
        //
        //catch (AmbiguousMatchException e)
        //{
        //    
        //}


        if (info != null)        
            return info;
        
        return null;
    }

    public bool ParseKeywords(string input)
    {
        MethodInfo info = typeof(BlueprintComponent).GetMethod(input);
        if (info != null)        
            return true;
    
        return false;
    }

    //Return true if multiple types are found
    bool FindType(string input, out Type[] type, out Assembly[] ASM)
    {
        //Source: https://stackoverflow.com/questions/8499593/c-sharp-how-to-check-if-namespace-class-or-method-exists-in-c
        //type = (from assembly in AppDomain.CurrentDomain.GetAssemblies() from type2 in assembly.GetTypes() where type2.Name == raw[0] select type2).FirstOrDefault();

        //To account for multiple namespaces
        int matchCount = 0;
        List<Type> typeList = new List<Type>();
        List<Assembly> asms = new List<Assembly>();

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
        
        if (meta.selectedType != null)
        {
            Debug.Log("type selected and var successfully created");
            Var newVar = new Var(meta.selectedType, raw[1]);
            newVar.input = input;
            data.variables.Add(newVar);
        }
        
    }
  
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

