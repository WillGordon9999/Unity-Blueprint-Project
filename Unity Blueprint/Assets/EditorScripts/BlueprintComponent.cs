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
    
    //public static Component GetComponent(string varName, string input, Dictionary<string, Var> dict)
    //{
    //    Component comp = (Component)dict[varName].obj;
    //    return comp.GetComponent(input);
    //}

    public Component GetComponent(string type, string other)
    {
        if (other != "")
        {
            return (variables[other].obj as Component).GetComponent(type);
        }
        return GetComponent(type);
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
                //Interpreter.Instance.CompileNode(node);
                //if (node.currentMethod == null && !node.isEntryPoint)
                //{
                //    node.currentMethod = Interpreter.Instance.LoadMethod(node.input, node.type, node.assemblyPath, node.index);
                //
                //    if (node.currentMethod.DeclaringType.BaseType == typeof(UnityEngine.Component))
                //    {                        
                //        Interpreter.Instance.CompileNode(node, (object)GetComponent(node.currentMethod.DeclaringType));
                //    }
                //
                //    else
                //        Interpreter.Instance.CompileNode(node);
                //
                //    if (node.actualTarget == null)
                //        print("Node's target is null!");
                //}

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
                        node.currentMethod = Interpreter.Instance.LoadMethod(node.input, node.type, node.assemblyPath, node.index, node.isContextual);
                        Interpreter.Instance.CompileNode(node);

                        //if (node.isContextual)
                        //{
                        //    node.actualTarget = node.prevNode.returnObj;
                        //}
                        
                        if (node.varName != "" && !node.isStatic)
                            node.actualTarget = variables[node.varName].obj;                                                    
                    }
                }

                if (node.nodeType == NodeType.Field_Set || node.nodeType == NodeType.Field_Get)
                {
                    node.fieldVar = Interpreter.Instance.LoadField(node.input, node.type, node.assemblyPath);
                }

                if (node.nodeType == NodeType.Property_Set || node.nodeType == NodeType.Property_Get)
                {
                    node.propertyVar = Interpreter.Instance.LoadProperty(node.input, node.type, node.assemblyPath);
                }
               
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
                        if (current.retType == Node.ReturnVarType.Var)
                        {
                            if (current.returnInput != null)
                            {
                                if (current.returnInput != "")
                                {
                                    Var test;

                                    if (!variables.TryGetValue(current.returnInput, out test))
                                        variables[current.returnInput] = new Var(current.returnObj, current.returnType);

                                    else
                                    {
                                        variables[current.returnInput].obj = current.returnObj;
                                        //variables[current.returnInput].type = current.returnObj;
                                    }
                                }
                            }
                        }                     
                    }
                }

                    /*
                    Get Rigidbody
                    Get prev node on next
                    Get object 
                    Then call.  
                    */

                /*
                 Need to be able to determine whether you are:
                    - setting to a variable
                    - getting a variable
                    - Or getting something contextually from the previous node

                 */
                if (current.nodeType == NodeType.Field_Get)
                {                    
                    current.returnObj = current.fieldVar.GetValue(current.actualTarget);
                }

                
                if (current.nodeType == NodeType.Property_Get)
                {
                    if (current.varName != "")
                    {
                        current.returnObj = current.propertyVar.GetValue(variables[current.varName].obj);

                        if (current.returnInput != "")
                        {
                            Var test;
                            //need try get value
                            if (!variables.TryGetValue(current.returnInput, out test))
                                variables[current.returnInput] = new Var(current.returnObj, current.propertyVar.PropertyType);

                            else
                            {
                                variables[current.returnInput].obj = current.returnObj;
                                variables[current.returnInput].type = current.propertyVar.PropertyType;
                            }
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
                        //Var other = BlueprintManager.blueprints[data].variables[(string)current.varField.arg];
                        Var other = variables[(string)current.varField.arg];
                        current.fieldVar.SetValue(v.obj, other.name);
                    }

                    else
                    {
                        current.fieldVar.SetValue(v.obj, current.literalField.arg);
                    }
                }

                if (current.nodeType == NodeType.Property_Set)
                {
                    Var v = variables[current.varName];
                    if (current.isVar)
                    {
                        //Var other = BlueprintManager.blueprints[data].variables[(string)current.varField.arg];
                        Var other = variables[(string)current.varField.arg];
                        current.propertyVar.SetValue(v.obj, other.name);
                    }

                    else
                    {
                        current.propertyVar.SetValue(v.obj, current.literalField.arg);
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
    public void LateUpdate() { }	                                        //LateUpdate is called every frame, if the Behaviour is enabled.
    
    //Collision
    public void OnCollisionEnter(Collision collision) { }	                //OnCollisionEnter is called when this collider/rigidbody has begun touching another rigidbody/collider.
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
