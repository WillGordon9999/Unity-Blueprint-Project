using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public enum ActionStatus { Success, Failure }

public class BlueprintComponent : MonoBehaviour
{
    public string ComponentName;
    public Dictionary<string, Var> variables = new Dictionary<string, Var>();        
    public Blueprint bp;
    public BlueprintData data;
    public Dictionary<string, Action> functions = new Dictionary<string, Action>();
    public bool useGame;
    public ActionStatus status;
   
    [ExecuteInEditMode]
    public Component GetTargetComponent(Type type)
    {
        return GetComponent(type);        
    }
      
    //public Component GetComponent(string type, string other)
    //{
    //    print("In Custom Get Component");
    //    if (other != "")
    //    {
    //        return (variables[other].obj as Component).GetComponent(type);
    //    }
    //
    //    Component comp = GetComponent(type);
    //
    //    if (comp)
    //        print($"Comp is {comp.name} {comp.GetType()} ");
    //    else
    //        print("Comp is null in Custom Get Component");
    //
    //    return comp;
    //}

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

    //public void Sweep(GameObject obj, Vector3 targetPos, float speed)
    //{
    //    //float cost = 20.0f * Vector3.Distance(obj.transform.position, targetPos) * speed;
    //    float cost = 100.0f;
    //    print("Inside sweep");
    //
    //    if (cost > 50.0f)
    //    {
    //        print("Not enough Mana");
    //        status = ActionStatus.Failure;
    //        return;
    //    }
    //
    //    obj.transform.position = Vector3.Lerp(obj.transform.position, targetPos, speed * Time.deltaTime);
    //}

    public virtual void EnterState()
    {

    }

    public virtual void UpdateState()
    {

    }

    public virtual void FixedUpdateState()
    {

    }

    public virtual void ExitState()
    {

    }

    public virtual void StateSetup()
    {

    }
        
    //This function is called when the script is loaded or a value is changed in the Inspector (Called in the editor only).
    public void OnEnable()
    {        
        if (BlueprintManager.blueprints == null)
        {
            BlueprintManager.blueprints = new Dictionary<BlueprintData, Blueprint>();
        }

        if (BlueprintManager.blueprints != null && !BlueprintManager.blueprints.TryGetValue(data, out bp))
        {
            if (!Application.isPlaying)
                return;
            //else
            //    print("In construction of blueprint component play");

            bp = new Blueprint();
            bp.name = ComponentName;
            BlueprintManager.blueprints[data] = bp;

            foreach (NodeData node in data.nodes)
            {
                Node newNode = new Node(node, null, null, null);

                if (node.isEntryPoint)
                {                    
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

            foreach (string key in bp.entryPoints.Keys)
            {
                functions[key] = Interpreter.Instance.FullCompile_Expression(this, key);
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

            foreach (string key in bp.entryPoints.Keys)
            {
                functions[key] = Interpreter.Instance.FullCompile_Expression(this, key);
            }
           
        }

        //Interpreter.Instance.FullCompile(this, typeof(MonoBehaviour));
        
        return;
    }
   
    public void Awake()
    {
        //ExecuteLoop("Awake");
        //functions["Awake"]?.Invoke();
        Action call;
        functions.TryGetValue("Awake", out call);
        call?.Invoke();
    }

    //Start is called on the frame when a script is enabled just before any of the Update methods are called the first time.
    public void Start()
    {
        //ExecuteLoop("Start");      
        //functions["Start"]?.Invoke();                
        Action call;
        functions.TryGetValue("Start", out call);
        call?.Invoke();
    }                                                 
                                             //This function is called when the object becomes enabled and active.
    public void OnDisable()
    {
        //ExecuteLoop("OnDisable");
        //functions["OnDisable"]?.Invoke();
        Action call;
        functions.TryGetValue("OnDisable", out call);
        call?.Invoke();
    }                                             //This function is called when the behaviour becomes disabled.
    public void OnDestroy()
    {
        //ExecuteLoop("OnDestroy");
        //functions["OnDestroy"]?.Invoke();
        Action call;
        functions.TryGetValue("OnDestroy", out call);
        call?.Invoke();
    }                                             //Destroying the attached Behaviour will result in the game or Scene receiving OnDestroy.
    public void Reset()
    {
        //ExecuteLoop("Reset");
        //functions["Reset"]?.Invoke();
        Action call;
        functions.TryGetValue("Reset", out call);
        call?.Invoke();
    }                                                 //Reset to default values.

       
    //Updates

    //Update is called every frame, if the MonoBehaviour is enabled.
    public void Update()
    {
        //ExecuteLoop("Update");
        //functions["Update"]?.Invoke();
        Action call;
        functions.TryGetValue("Update", out call);
        call?.Invoke();
    }                                                
    public void FixedUpdate()
    {
        //ExecuteLoop("FixedUpdate");
        //functions["FixedUpdate"]?.Invoke();
        Action call;
        functions.TryGetValue("FixedUpdate", out call);
        call?.Invoke();
    }	                                        //Frame-rate independent MonoBehaviour.FixedUpdate message for physics calculations.
    public void LateUpdate()
    {
        //ExecuteLoop("LateUpdate");
        //functions["LateUpdate"]?.Invoke();
        Action call;
        functions.TryGetValue("LateUpdate", out call);
        call?.Invoke();
    }                                            //LateUpdate is called every frame, if the Behaviour is enabled.

    //Collision
    //OnCollisionEnter is called when this collider/rigidbody has begun touching another rigidbody/collider.
    public void OnCollisionEnter(Collision collision)
    {
        if (CheckVar("collision"))
        {
            variables["collision"].obj = collision;
            variables["collision"].type = collision.GetType();
        }

        else
            variables["collision"] = new Var(collision, collision.GetType());


        //ExecuteLoop("OnCollisionEnter");
        //functions["OnCollisionEnter"]?.Invoke();
        Action call;
        functions.TryGetValue("OnCollisionEnter", out call);
        call?.Invoke();
    }

    //Sent when an incoming collider makes contact with this object's collider (2D physics only).
    public void OnCollisionEnter2D(Collision2D collision)
    {
        if (CheckVar("collision"))
        {
            variables["collision"].obj = collision;
            variables["collision"].type = collision.GetType();
        }

        else
            variables["collision"] = new Var(collision, collision.GetType());


        //ExecuteLoop("OnCollisionEnter2D");
        //functions["OnCollisionEnter2D"]?.Invoke();
        Action call;
        functions.TryGetValue("OnCollisionEnter2D", out call);
        call?.Invoke();
    }

    //OnCollisionExit is called when this collider/rigidbody has stopped touching another rigidbody/collider.
    public void OnCollisionExit(Collision collision)
    {
        if (CheckVar("collision"))
        {
            variables["collision"].obj = collision;
            variables["collision"].type = collision.GetType();
        }

        else
            variables["collision"] = new Var(collision, collision.GetType());

        //functions["OnCollisionExit"]?.Invoke();
        Action call;
        functions.TryGetValue("OnCollisionExit", out call);
        call?.Invoke();
    }

    //Sent when a collider on another object stops touching this object's collider (2D physics only).
    public void OnCollisionExit2D(Collision2D collision)
    {
        if (CheckVar("collision"))
        {
            variables["collision"].obj = collision;
            variables["collision"].type = collision.GetType();
        }

        else
            variables["collision"] = new Var(collision, collision.GetType());

        //functions["OnCollisionExit2D"]?.Invoke();
        Action call;
        functions.TryGetValue("OnCollisionExit2D", out call);
        call?.Invoke();
    }

    //OnCollisionStay is called once per frame for every collider/rigidbody that is touching rigidbody/collider.
    public void OnCollisionStay(Collision collision)
    {
        if (CheckVar("collision"))
        {
            variables["collision"].obj = collision;
            variables["collision"].type = collision.GetType();
        }

        else
            variables["collision"] = new Var(collision, collision.GetType());

        //functions["OnCollisionStay"]?.Invoke();
        Action call;
        functions.TryGetValue("OnCollisionStay", out call);
        call?.Invoke();
    }

    //Sent each frame where a collider on another object is touching this object's collider (2D physics only).
    public void OnCollisionStay2D(Collision2D collision)
    {
        if (CheckVar("collision"))
        {
            variables["collision"].obj = collision;
            variables["collision"].type = collision.GetType();
        }

        else
            variables["collision"] = new Var(collision, collision.GetType());

        //functions["OnCollisionStay2D"]?.Invoke();
        Action call;
        functions.TryGetValue("OnCollisionStay2D", out call);
        call?.Invoke();
    }

    //Triggers     

    //When a GameObject collides with another GameObject, Unity calls OnTriggerEnter.
    public void OnTriggerEnter(Collider other)
    {

    }

    //Sent when another object enters a trigger collider attached to this object (2D physics only).
    public void OnTriggerEnter2D(Collider2D other)
    {

    }

    //OnTriggerExit is called when the Collider other has stopped touching the trigger.
    public void OnTriggerExit(Collider other)
    {

    }

    //Sent when another object leaves a trigger collider attached to this object (2D physics only).
    public void OnTriggerExit2D(Collider2D other)
    {

    }

    //OnTriggerStay is called once per physics update for every Collider other that is touching the trigger.
    public void OnTriggerStay(Collider other)
    {

    }
    //Sent each frame where another object is within a trigger collider attached to this object (2D physics only).
    public void OnTriggerStay2D(Collider2D other)
    {

    }                       

    //GUI
    //OnGUI is called for rendering and handling GUI events.    
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

/*
public void ExecuteLoop(string entryPoint)
    {
        if (BlueprintManager.blueprints != null)
        {
            //Node current = BlueprintManager.blueprints[data].entryPoints[entryPoint];
            Blueprint blueprint;
            Node current = null;
            if (BlueprintManager.blueprints.TryGetValue(data, out blueprint))
                blueprint.entryPoints.TryGetValue(entryPoint, out current);

            while (current != null)
            {
                if (current.isContextual)
                {
                    current.actualTarget = current.prevNode.returnObj;
                }

                if (current.nodeType == NodeType.Function && current.function != null)
                {
                    for (int i = 0; i < current.paramList.Count; i++)
                    {
                        if (current.paramList[i].inputVar)
                        {
                            //DEBUG THIS
                            current.passArgs[i] = variables[current.paramList[i].varInput].obj;
                        }
                    }


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

                        if (!CheckVar(current.returnInput))
                            variables[current.returnInput] = new Var(current.returnObj, current.fieldVar.FieldType);
                        else
                        {
                            variables[current.returnInput].obj = current.returnObj;
                            variables[current.returnInput].type = current.fieldVar.FieldType;
                        }
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

                        current.returnObj = current.function.Invoke(current.actualTarget, null);

                        if (!CheckVar(current.returnInput))
                            variables[current.returnInput] = new Var(current.returnObj, current.fieldVar.FieldType);
                        else
                        {
                            variables[current.returnInput].obj = current.returnObj;
                            variables[current.returnInput].type = current.fieldVar.FieldType;
                        }
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
*/