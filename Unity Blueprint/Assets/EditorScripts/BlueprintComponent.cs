using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class BlueprintComponent : MonoBehaviour
{
    public string ComponentName;
    public Dictionary<string, Var> variables = new Dictionary<string, Var>();        
    Blueprint bp;
    public BlueprintData data;    
   
    [ExecuteInEditMode]
    public Component GetTargetComponent(Type type)
    {
        return GetComponent(type);        
    }
      
    public Component GetComponent(string type, string other)
    {
        if (other != "")
        {
            return (variables[other].obj as Component).GetComponent(type);
        }
                        
        return GetComponent(type);        
    }

    public Component AddComponent(string type, string other)
    {
        Type compType = GetComponent(type).GetType();

        if (other != "")
        {
            return (variables[other].obj as GameObject).AddComponent(compType);
        }

        return gameObject.AddComponent(compType);
    }
    
    //Where value can be a literal or a variable name
    public void Set(string varName, string value)
    {
        if (CheckVar(varName))
        {
            if (CheckVar(value))
            {
                if (variables[varName].type == variables[value].type)
                    variables[varName].obj = variables[value].obj;

                else
                    Debug.LogError("ERROR: Cannot assign two different types");
            }

            else
            {
                object result = Interpreter.Instance.ParseArgumentType(value);

                if (variables[varName].type == result.GetType())
                    variables[varName].obj = result;

                else
                    Debug.LogError("ERROR: Cannot assign two different types");
            }
        }

        else
            Debug.LogError("Error: No Variable Found In Set");
    }

    bool CheckVar(string name)
    {
        Var v;
        return variables.TryGetValue(name, out v);        
    }

    //Must be able to except checking against another variable or a literal if applicable
    //Can compare with literals for the common types, but otherwise must use a var for comparing classes
    public bool Equals(string v, string v2)
    {
        if (CheckVar(v))
        {
            if (CheckVar(v2))
                return HelperFunctions.Equals(variables[v], variables[v2]);
            else
            {
                object result = Interpreter.Instance.ParseArgumentType(v2);

                if (result != null)
                    return HelperFunctions.Equals(variables[v], new Var(result, result.GetType()));

                else
                    return false;
            }
        }

        else
            return false;       
    }
    public bool NotEquals(string v, string v2)
    {
        if (CheckVar(v))
        {
            if (CheckVar(v2))
                return HelperFunctions.NotEquals(variables[v], variables[v2]);
            else
            {
                object result = Interpreter.Instance.ParseArgumentType(v2);

                if (result != null)
                    return HelperFunctions.NotEquals(variables[v], new Var(result, result.GetType()));

                else
                    return false;
            }
        }

        else
            return false;
    }
    public bool LessThan(string v, string v2)
    {
        if (CheckVar(v))
        {
            if (CheckVar(v2))
                return HelperFunctions.LessThan(variables[v], variables[v2]);
            else
            {
                object result = Interpreter.Instance.ParseArgumentType(v2);

                if (result != null)
                    return HelperFunctions.LessThan(variables[v], new Var(result, result.GetType()));

                else
                    return false;
            }
        }

        else
            return false;
    }
    public bool GreaterThan(string v, string v2)
    {
        if (CheckVar(v))
        {
            if (CheckVar(v2))
                return HelperFunctions.GreaterThan(variables[v], variables[v2]);
            else
            {
                object result = Interpreter.Instance.ParseArgumentType(v2);

                if (result != null)
                    return HelperFunctions.GreaterThan(variables[v], new Var(result, result.GetType()));

                else
                    return false;
            }
        }

        else
            return false;
    }
    public bool LessThanOrEqual(string v, string v2)
    {
        if (CheckVar(v))
        {
            if (CheckVar(v2))
                return HelperFunctions.LessThanOrEqual(variables[v], variables[v2]);
            else
            {
                object result = Interpreter.Instance.ParseArgumentType(v2);

                if (result != null)
                    return HelperFunctions.LessThanOrEqual(variables[v], new Var(result, result.GetType()));

                else
                    return false;
            }
        }

        else
            return false;
    }
    public bool GreaterThanOrEqual(string v, string v2)
    {
        if (CheckVar(v))
        {
            if (CheckVar(v2))
                return HelperFunctions.GreaterThanOrEqual(variables[v], variables[v2]);
            else
            {
                object result = Interpreter.Instance.ParseArgumentType(v2);

                if (result != null)
                    return HelperFunctions.GreaterThanOrEqual(variables[v], new Var(result, result.GetType()));

                else
                    return false;
            }
        }

        else
            return false;
    }
    public bool And(string v, string v2)
    {
        if (CheckVar(v))
        {
            if (CheckVar(v2))
                return HelperFunctions.And(variables[v], variables[v2]);
            else
            {
                object result = Interpreter.Instance.ParseArgumentType(v2);

                if (result != null)
                    return HelperFunctions.And(variables[v], new Var(result, result.GetType()));

                else
                    return false;
            }
        }

        else
            return false;
    }
    public bool Or(string v, string v2)
    {
        if (CheckVar(v))
        {
            if (CheckVar(v2))
                return HelperFunctions.Or(variables[v], variables[v2]);
            else
            {
                object result = Interpreter.Instance.ParseArgumentType(v2);

                if (result != null)
                    return HelperFunctions.Or(variables[v], new Var(result, result.GetType()));

                else
                    return false;
            }
        }

        else
            return false;
    }

    [ExecuteAlways]
    //This function is called when the script is loaded or a value is changed in the Inspector (Called in the editor only).
    public void OnValidate()
    {
        if (BlueprintManager.blueprints == null)
        {
            BlueprintManager.blueprints = new Dictionary<BlueprintData, Blueprint>();
        }

        if (BlueprintManager.blueprints != null && !BlueprintManager.blueprints.TryGetValue(data, out bp))
        {
            if (!Application.isPlaying)
                print("In construction of blueprint component edit");
            else
                print("In construction of blueprint component play");

            Blueprint bp = new Blueprint();
            bp.name = ComponentName;
            BlueprintManager.blueprints[data] = bp;

            foreach (NodeData node in data.nodes)
            {
                Node newNode = new Node(node, null, null, null);

                if (node.isEntryPoint)
                {
                    //Debug.Log("Entry point confirmed");
                    bp.entryPoints[node.input] = newNode;
                }

                bp.nodes.Add(newNode);
            }

            //Prepare variables
            foreach (Var v in data.variables)
            {
                v.type = Interpreter.Instance.LoadVarType(v.strType, v.asmPath);
                variables[v.name] = v;
            }
           
            foreach (Node node in bp.nodes)
            {                
                if (node.nodeType == NodeType.Function)
                {
                    if (node.isSpecial)
                    {
                        node.currentMethod = Interpreter.Instance.GetSpecialFunction(node.input);
                        node.actualTarget = (object)this;
                        Interpreter.Instance.CompileNode(node);
                    }
                    else
                    {                        
                        //node.currentMethod = Interpreter.Instance.LoadMethod(node.input, node.type, node.assemblyPath, node.index, node.isContextual);
                        Interpreter.Instance.CompileNode(node);
                                              
                        if (node.varName != "" && !node.isStatic)
                            node.actualTarget = variables[node.varName].obj;                                                    
                    }
                }
                
                //Targets have to be set during main loop
                if (node.nodeType == NodeType.Field_Get || node.nodeType == NodeType.Field_Set)                
                    Interpreter.Instance.CompileNode(node);

                if (node.nodeType == NodeType.Property_Get || node.nodeType == NodeType.Property_Set)
                    Interpreter.Instance.CompileNode(node);


                //if (node.nodeType == NodeType.Field_Set || node.nodeType == NodeType.Field_Get)
                //{
                //    node.fieldVar = Interpreter.Instance.LoadField(node.input, node.type, node.assemblyPath);
                //}
                //
                //if (node.nodeType == NodeType.Property_Set || node.nodeType == NodeType.Property_Get)
                //{
                //    node.propertyVar = Interpreter.Instance.LoadProperty(node.input, node.type, node.assemblyPath);
                //}
               
                foreach (Node node1 in bp.nodes)
                {
                    if (node.nextID == node1.ID)
                    {
                        node.nextNode = node1;
                        node1.prevNode = node;

                        if (node.nodeType != NodeType.Conditional)
                            break;
                    }

                    if (node.nodeType ==  NodeType.Conditional)
                    {
                        if (node.falseID == node1.ID)
                        {
                            node.falseNode = node1;
                            node1.prevNode = node;

                            if (node.nextNode != null)
                                break;
                        }
                    }
                }
            }

            //BlueprintManager.blueprints[bp.name] = bp;            
            
        }
        else
        {
            print("blueprint already constructed");

            //Prepare variables
            foreach (Var v in data.variables)
            {
                v.type = Interpreter.Instance.LoadVarType(v.strType, v.asmPath);
                variables[v.name] = v;
            }

            foreach(Node node in bp.nodes)
            {
                if (node.varName != "")
                {
                    node.actualTarget = variables[node.varName].obj;
                }
            }
        }
    }

    //Initialization/Destroy
    //Awake is called when the script instance is being loaded.
    public void Awake()
    {             
    }

    //Start is called on the frame when a script is enabled just before any of the Update methods are called the first time.
    public void Start()
    {
        if (BlueprintManager.blueprints != null)
        {                        
            //Node current = BlueprintManager.blueprints[ComponentName].entryPoints["Start"];
            Node current = BlueprintManager.blueprints[data].entryPoints["Start"];

            while (current != null)
            {
                if (current.isContextual)
                {
                    current.actualTarget = current.prevNode.returnObj;
                }

                if (current.nodeType == NodeType.Function && current.function != null)
                {
                    if (current.isSpecial)
                    {
                        current.actualTarget = this;
                        current.returnObj = current.function.Invoke(current.actualTarget, current.passArgs);
                    }

                    else
                    {
                        if (!current.isStatic)
                        {
                            if (current.varName != "")
                                current.returnObj = current.function.Invoke(variables[current.varName].obj, current.passArgs);
                            else
                                current.returnObj = current.function.Invoke(current.actualTarget, current.passArgs);
                        }
                        else
                            current.returnObj = current.function.Invoke(current.actualTarget, current.passArgs);
                    }

                    if (current.isReturning)
                    {                                                   
                        if (current.returnInput != null)
                        {
                            if (current.returnInput != "")
                            {
                                if (!CheckVar(current.returnInput))
                                    variables[current.returnInput] = new Var(current.returnObj, current.returnType);

                                else                                    
                                    variables[current.returnInput].obj = current.returnObj;                                                                            
                            }
                        }                                             
                    }
                }
                 
                if (current.nodeType == NodeType.Field_Get)
                {
                    if (current.varName != "")
                    {                      
                        current.returnObj = current.function.Invoke(variables[current.varName], null);

                        if (!CheckVar(current.returnInput))
                            variables[current.returnInput] = new Var(current.returnObj, current.fieldVar.FieldType);
                        else
                        {
                            variables[current.returnInput].obj = current.returnObj;
                            variables[current.returnInput].type = current.fieldVar.FieldType;
                        }
                    }

                    else
                    {
                        current.returnObj = current.function.Invoke(current.actualTarget, null);
                    }
                }

                
                if (current.nodeType == NodeType.Property_Get)
                {
                    if (current.varName != "")
                    {                       
                        current.returnObj = current.function.Invoke(variables[current.varName].obj, null);

                        if (!CheckVar(current.returnInput))
                            variables[current.returnInput] = new Var(current.returnObj, current.propertyVar.PropertyType);
                        else
                        {
                            variables[current.returnInput].obj = current.returnObj;
                            variables[current.returnInput].type = current.propertyVar.PropertyType;
                        }

                    }

                    else
                    {
                        current.returnObj = current.propertyVar.GetValue(current.actualTarget);
                    }
                }
                
                if (current.nodeType == NodeType.Field_Set)
                {
                    Var v = variables[current.varName];
                    if (current.isVar)
                    {                        
                        Var other = variables[(string)current.varField.arg];
                        current.function.Invoke(v.obj, new object[] { other.obj });
                    }

                    else
                    {
                        //current.fieldVar.SetValue(v.obj, current.literalField.arg);
                        current.function.Invoke(v.obj, new object[] { current.literalField.arg });
                    }
                }

                if (current.nodeType == NodeType.Property_Set)
                {
                    Var v = variables[current.varName];
                    if (current.isVar)
                    {                        
                        Var other = variables[(string)current.varField.arg];                        
                        current.function.Invoke(v.obj, new object[] { other.obj });
                    }

                    else
                    {
                        //current.propertyVar.SetValue(v.obj, current.literalField.arg);
                        current.function.Invoke(v.obj, new object[] { current.literalField.arg });
                    }
                }

                if (current.nodeType == NodeType.Conditional)
                {
                    Var v = variables[(string)current.paramList[0].arg];

                    if (v.type == typeof(bool))
                    {
                        bool val = (bool)v.obj;

                        if (val)
                            current = current.nextNode;
                        else
                            current = current.falseNode;

                        continue;
                    }

                    else
                        Debug.Log("WARNING: variable passed in is not a bool");
                    
                }

                current = current.nextNode;
            }

        }
      
    }                                                 
    public void OnEnable() { }	                                            //This function is called when the object becomes enabled and active.
    public void OnDisable() { }                                             //This function is called when the behaviour becomes disabled.
    public void OnDestroy() { }                                             //Destroying the attached Behaviour will result in the game or Scene receiving OnDestroy.
    public void Reset() { }                                                 //Reset to default values.

       
    //Updates

    //Update is called every frame, if the MonoBehaviour is enabled.
    public void Update()
    {
       
    }                                                
    public void FixedUpdate() { }	                                        //Frame-rate independent MonoBehaviour.FixedUpdate message for physics calculations.
    public void LateUpdate() { }                                            //LateUpdate is called every frame, if the Behaviour is enabled.

    //Collision
    //OnCollisionEnter is called when this collider/rigidbody has begun touching another rigidbody/collider.
    public void OnCollisionEnter(Collision collision)
    {

    }	                
    public void OnCollisionEnter2D(Collision2D collision) { }	            //Sent when an incoming collider makes contact with this object's collider (2D physics only).
    public void OnCollisionExit(Collision collision) { }	                //OnCollisionExit is called when this collider/rigidbody has stopped touching another rigidbody/collider.
    public void OnCollisionExit2D(Collision2D collision) { }	            //Sent when a collider on another object stops touching this object's collider (2D physics only).
    public void OnCollisionStay(Collision collision) { }	                //OnCollisionStay is called once per frame for every collider/rigidbody that is touching rigidbody/collider.
    public void OnCollisionStay2D(Collision2D collision) { }	            //Sent each frame where a collider on another object is touching this object's collider (2D physics only).
            
    //Triggers                      
    public void OnTriggerEnter(Collider other) { }	                        //When a GameObject collides with another GameObject, Unity calls OnTriggerEnter.
    public void OnTriggerEnter2D(Collider2D other) { }	                    //Sent when another object enters a trigger collider attached to this object (2D physics only).
    public void OnTriggerExit(Collider other) { }	                        //OnTriggerExit is called when the Collider other has stopped touching the trigger.
    public void OnTriggerExit2D(Collider2D other) { }	                    //Sent when another object leaves a trigger collider attached to this object (2D physics only).
    public void OnTriggerStay(Collider other) { }	                        //OnTriggerStay is called once per physics update for every Collider other that is touching the trigger.
    public void OnTriggerStay2D(Collider2D other) { }                       //Sent each frame where another object is within a trigger collider attached to this object (2D physics only).

    //GUI
    //OnGUI is called for rendering and handling GUI events.
    [ExecuteAlways]
    public void OnGUI()
    {
        //GUILayout.Button("Press me");
    }	                                                

}

/*
    public void OnAnimatorIK(int layerIndex) { }	                        //Callback for setting up animation IK (inverse kinematics).
    public void OnAnimatorMove() { }	                                    //Callback for processing animation movements for modifying root motion.
    public void OnApplicationFocus(bool hasFocus) { }	                    //Sent to all GameObjects when the player gets or loses focus.
    public void OnApplicationPause(bool pauseStatus) { }	                //Sent to all GameObjects when the application pauses.
    public void OnApplicationQuit() { }	                                    //Sent to all game objects before the application quits.
    public void OnAudioFilterRead(float[] data, int channels) { }	        //If OnAudioFilterRead is implemented, Unity will insert a custom filter into the audio DSP chain.
   
    public void OnParticleCollision(GameObject other) { }	                //OnParticleCollision is called when a particle hits a Collider.    
    public void OnParticleTrigger() { }	                                    //OnParticleTrigger is called when any particles in a Particle System meet the conditions in the trigger module.
    public void OnControllerColliderHit(ControllerColliderHit hit) { }	    //OnControllerColliderHit is called when the controller hits a collider while performing a Move.

    //Rendering
    public void OnPostRender() { }	                                        //OnPostRender is called after a camera finished rendering the Scene.
    public void OnPreCull() { }	                                            //OnPreCull is called before a camera culls the Scene.
    public void OnPreRender() { }	                                        //OnPreRender is called before a camera starts rendering the Scene.
    public void OnRenderImage(RenderTexture src, RenderTexture dest) { }	//OnRenderImage is called after all rendering is complete to render image.
    public void OnRenderObject() { }	                                    //OnRenderObject is called after camera has rendered the Scene.     
    public void OnWillRenderObject() { }	                                //OnWillRenderObject is called for each camera if the object is visible and not a UI element.
    public void OnBecameInvisible() { }	                                    //OnBecameInvisible is called when the renderer is no longer visible by any camera.
    public void OnBecameVisible() { }	                                    //OnBecameVisible is called when the renderer became visible by any camera.

    public void OnTransformChildrenChanged() { }	                        //This function is called when the list of children of the transform of the GameObject has changed.
    public void OnTransformParentChanged() { }	                            //This function is called when the parent property of the transform of the GameObject has changed.

    //Mouse Input
    public void OnMouseDown() { }	                                        //OnMouseDown is called when the user has pressed the mouse button while over the Collider.
    public void OnMouseDrag() { }	                                        //OnMouseDrag is called when the user has clicked on a Collider and is still holding down the mouse.
    public void OnMouseEnter() { }	                                        //Called when the mouse enters the Collider.
    public void OnMouseExit() { }	                                        //Called when the mouse is not any longer over the Collider.
    public void OnMouseOver() { }	                                        //Called every frame while the mouse is over the Collider.
    public void OnMouseUp() { }	                                            //OnMouseUp is called when the user has released the mouse button.
    public void OnMouseUpAsButton() { }	                                    //OnMouseUpAsButton is only called when the mouse is released over the same Collider as it was pressed.

    public void OnDrawGizmos() { }	                                        //Implement OnDrawGizmos if you want to draw gizmos that are also pickable and always drawn.
    public void OnDrawGizmosSelected() { }	                                //Implement OnDrawGizmosSelected to draw a gizmo if the object is selected.

    public void OnJointBreak(float breakForce) { }	                        //Called when a joint attached to the same game object broke.
    public void OnJointBreak2D(Joint2D brokenJoint) { }	                    //Called when a Joint2D attached to the same game object breaks.
 */
