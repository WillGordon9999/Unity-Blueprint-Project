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

    public bool returnBool;
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

    List<Expression> expressions = new List<Expression>();

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

        Type varType = null;

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
                if (ParseKeywords(args[0], node))
                {
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
            string name = "";

            if (node != null && node.isContextual)
                name = args[0];
            else
                name = args[1];

            ConstructorInfo[] constructors = data.selectedType.GetConstructors();
            
            if (args.Length > 1)
            {
                if (args[0] == args[1])
                    data.constructors = constructors;
            }          

            MethodInfo[] methods = data.selectedType.GetMethods();
            List<MethodInfo> methodInfo = new List<MethodInfo>();

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

            if (node.currentMethod != null)
            {
                if (node.isReturning && node.isStatic)
                    //Instance.expressions.Add(NonVoidStaticMethodExpression(node.currentMethod));
                    node.expressionBody.Add(NonVoidStaticMethodExpression(node.currentMethod));

                else if (node.isReturning && !node.isStatic)
                    //Instance.expressions.Add(NonVoidInstanceMethodExpression(node.currentMethod));
                    node.expressionBody.Add(NonVoidInstanceMethodExpression(node.currentMethod));

                else if (!node.isReturning && node.isStatic)
                    //Instance.expressions.Add(VoidStaticMethodExpression(node.currentMethod));
                    node.expressionBody.Add(VoidStaticMethodExpression(node.currentMethod));

                else if (!node.isReturning && !node.isStatic)
                    //Instance.expressions.Add(VoidInstanceMethodExpression(node.currentMethod));
                    node.expressionBody.Add(VoidInstanceMethodExpression(node.currentMethod));

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
            
            if ((string)node.varField.arg != "")
            {
                  node.isVar = true;
            }                                  
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
          
            if ((string)node.varField.arg != "")            
                node.isVar = true;
            
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
        if (asmPath == "")
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

    public bool ParseKeywords(string text, Node node)
    {
        MethodInfo info = null;
        
        try
        {
            info = typeof(BlueprintComponent).GetMethod(text);
        }

        catch
        {            
            if (text == "GetComponent" || text == "AddComponent" || text == "Set")
            {                
                info = typeof(BlueprintComponent).GetMethod(text, new Type[] { typeof(string), typeof(string) });
            }        
        }
        
        if (info != null)
        {
            if (info.Name == "GetComponent" || info.Name == "AddComponent" || info.Name == "Set")
            {
                node.isSpecial = true;                
                node.ChangeToSpecialMethod(info);
                return true;
            }

            Debug.Log($"Method found for {text}");

            ParameterInfo[] pars = info.GetParameters();

            if (node.passInParams == null)
                node.passInParams = new List<string>();

            foreach (ParameterInfo p in pars)
            {
                string final = p.ParameterType + " " + p.Name;
                node.passInParams.Add(final);
            }

            float initHeight = node.rect.height;

            if (node.passInParams.Count == 1)
                node.rect = new Rect(node.rect.x, node.rect.y, node.rect.width, initHeight * (node.passInParams.Count + 1));


            else if (node.passInParams.Count > 1)
                node.rect = new Rect(node.rect.x, node.rect.y, node.rect.width, initHeight * (node.passInParams.Count));

            node.isDefined = true;
            node.isEntryPoint = true;
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

    public MethodInfo GetSpecialFunction(string input)
    {
        MethodInfo info = null;
        
        //This will probably have to be altered later
        info = typeof(BlueprintComponent).GetMethod(input, new Type[] { typeof(string), typeof(string) });

        if (info != null)        
            return info;
        
        return null;
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

            bool isPrimitive = false;

            //Primitive Check + string
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

    public Action FullCompile(BlueprintComponent blueprint, string funcName)
    {
        if (blueprint.bp == null)
            return null;

        Node node = blueprint.bp.entryPoints[funcName];
        List<Expression> expressions = new List<Expression>();
        Expression contextTarget = null;

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
                    expressions.Add(CallFunction(blueprint, node, out contextTarget, contextTarget));                    
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
                    Expression trueBranch = CompileConditional(blueprint, node, true);
                    Expression falseBranch = CompileConditional(blueprint, node, false);

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
                expressions.Add(CallFunction(blueprint, node, out contextTarget));               
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
                Expression trueBranch = CompileConditional(blueprint, node, true);
                Expression falseBranch = CompileConditional(blueprint, node, false);

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

    Expression CompileConditional(BlueprintComponent blueprint, Node node, bool truePath, Expression target = null)
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
                    expressions.Add(CallFunction(blueprint, node, out contextTarget, contextTarget));
                
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
                    Expression trueBranch = CompileConditional(blueprint, node, true);
                    Expression falseBranch = CompileConditional(blueprint, node, false);
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
                expressions.Add(CallFunction(blueprint, node, out contextTarget));

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
                Expression trueBranch = CompileConditional(blueprint, node, true);
                Expression falseBranch = CompileConditional(blueprint, node, false);

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

    Expression CallFunction(BlueprintComponent blueprint, Node node, out Expression contextTarget, Expression target = null)
    {
        //Get the function target --NEEDS TO BE AN EXPRESSION
        if (node.isSpecial)
        {
            target = Expression.Constant(blueprint);
            node.currentMethod = GetSpecialFunction(node.input);
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

