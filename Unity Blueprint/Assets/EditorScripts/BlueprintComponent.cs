using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class BlueprintComponent : MonoBehaviour
{
    public string ComponentName;
    public Dictionary<string, RealTimeVar> variables = new Dictionary<string, RealTimeVar>();
    public Dictionary<string, Node> entryPoints = new Dictionary<string, Node>();
    [HideInInspector] public object returnObj;
    bool isDefined = false;
    public BlueprintData data;
    public Node entry;
    public Func<object, object[], object> func;
    public object test;
    public Action actionTest;
    public ConnectionPointType type;
    bool added;

    [ExecuteInEditMode]
    public Component GetTargetComponent(Type type)
    {
        return GetComponent(type);
    }

    [ExecuteInEditMode]
    public void SetUp()
    {
        //print("Set up is being called!");
        //
        //if (ComponentName == "Test")
        //{
        //    print("got name");
        //}
        //if (BlueprintData.RuntimeTest == null)
        //{
        //    print("Instantiating runtime test at edit mode");
        //
        //    BlueprintData.RuntimeTest = new Dictionary<string, Vector3>();
        //
        //    print("Adding a value to it");
        //
        //    BlueprintData.RuntimeTest["Test"] = Vector3.one * 2.0f;
        //    BlueprintData.added = true;
        //}        

        //if (BlueprintManager.blueprints == null)
        //{
        //    print("Instantiating new blueprint dictionary for manager");
        //    BlueprintManager.blueprints = new Dictionary<string, Blueprint>();
        //}
    }

    public void OnValidate()
    {
        SetUp();
        //BlueprintData newData = Interpreter.Instance.LoadBlueprint(ComponentName);
        //
        //if (newData != null && data == null)
        //{
        //    data = newData;
        //    Debug.Log("Found blueprint on component side");
        //    isDefined = true;
        //}
       
    }

    //Initialization/Destroy
    //Awake is called when the script instance is being loaded.
    public void Awake()
    {
        //if (variables == null)
        //    print("Variables is null");
        //
        //if (entryPoints == null)
        //    print("entry Points is null");
    }

    //Start is called on the frame when a script is enabled just before any of the Update methods are called the first time.
    public void Start()
    {
        if (BlueprintManager.blueprints != null)
        {
            Node current = BlueprintManager.blueprints[ComponentName].entryPoints["Start"];

            if (current.function != null)
            {
                print($"current node function in Start is not null");

                if (current.currentMethod.DeclaringType.BaseType == typeof(UnityEngine.Component))
                {
                    print("current method is from a component");
                    current.actualTarget = (object)GetComponent(current.currentMethod.DeclaringType);
                }

                returnObj = current.function.Invoke(current.actualTarget, current.passArgs);

                //if (returnObj != null)
                //{
                //
                //}
            }

            while (current.nextNode != null)
            {
                if (current.nextNode != current)
                {
                    current = current.nextNode;

                    if (!current.isEntryPoint)
                    {
                        if (current.currentMethod == null)
                        {
                            current.currentMethod = Interpreter.Instance.LoadMethod(current.input, current.type, current.assemblyPath, current.index);

                            if (current.currentMethod.DeclaringType.BaseType == typeof(UnityEngine.Component))
                            {
                                print("current method is from a component");
                                Interpreter.Instance.CompileNode(current, (object)GetComponent(current.currentMethod.DeclaringType));
                            }
                        }
                    }

                    if (current.function != null)
                    {
                        print("Current function is not null");
                        returnObj = current.function.Invoke(current.actualTarget, current.passArgs);
                    }

                }
                else
                {
                    print("current node equals out point");
                }


            }
        }

        else
            print("Blueprint manager is null");

        //if (data != null)
        //{
        //    //if (data.nodes == null)
        //    //{
        //    //    print("nodes is null adjusting");
        //    //    JsonUtility.FromJsonOverwrite(data.json, data);                
        //    //}
        //    //if (data.nodes != null)
        //    //{
        //    //    foreach(Node node in data.nodes)
        //    //    {
        //    //        print($"Node: {node.input}");
        //    //        if (node.currentMethod != null)
        //    //            print($"Node method {node.currentMethod.Name}");
        //    //        if (node.function != null)
        //    //        {
        //    //            print("calling function");
        //    //            object result = node.function.Invoke(null, node.passArgs);
        //    //            print($"result is type {result.GetType()}");
        //    //        }
        //    //    }
        //    //}
        //
        //    
        //    //if (data.entryPoints == null)
        //    //{
        //    //    Debug.Log("entry points is null");                
        //    //    //test.Find((x) => x.tex)
        //    //                    
        //    //    return;
        //    //}
        //    //
        //    //Node current = data.entryPoints["Start"];
        //
        //    //if (current != null)
        //    //{
        //    //    if (current.function != null)
        //    //    {
        //    //        returnObj = current.function.Invoke(this, current.passArgs);
        //    //
        //    //        if (returnObj != null)
        //    //        {
        //    //            Debug.Log($"Return object: {returnObj.ToString()}");
        //    //            Debug.Log($"Return object type: {returnObj.GetType()}");
        //    //        }
        //    //    }
        //    //}
        //    //
        //    //while (current.outPoint.node != null)
        //    //{
        //    //    current = current.outPoint.node;
        //    //
        //    //    if (current != null)
        //    //    {
        //    //        if (current.function != null)
        //    //        {
        //    //            returnObj = current.function.Invoke(this, current.passArgs);
        //    //
        //    //            if (returnObj != null)
        //    //            {
        //    //                Debug.Log($"Return object: {returnObj.ToString()}");
        //    //                Debug.Log($"Return object type: {returnObj.GetType()}");
        //    //            }
        //    //        }
        //    //    }
        //    //}
        //}
        //
        //else
        //    Debug.Log("blueprint data is null in component");

        

    }                                                 
    public void OnEnable() { }	                                            //This function is called when the object becomes enabled and active.
    public void OnDisable() { }                                             //This function is called when the behaviour becomes disabled.
    public void OnDestroy() { }                                             //Destroying the attached Behaviour will result in the game or Scene receiving OnDestroy.
    public void Reset() { }                                                 //Reset to default values.

    //This function is called when the script is loaded or a value is changed in the Inspector (Called in the editor only).
    

    //Updates

    //Update is called every frame, if the MonoBehaviour is enabled.
    public void Update()
    {
        //Node current = data.entryPoints["Update"];
        //
        //if (current != null)
        //{
        //    if (current.function != null)
        //        current.function.Invoke(this, current.passArgs);
        //}
        //
        //while (current.outPoint.node != null)
        //{
        //    current = current.outPoint.node;
        //
        //    if (current != null)
        //    {
        //        if (current.function != null)
        //            current.function.Invoke(this, current.passArgs);
        //    }
        //}



        //if (data != null)
        //{
        //    if (BlueprintData.RuntimeTest == null)
        //    {
        //        Debug.Log("Instantiating new runtime test");
        //        BlueprintData.RuntimeTest = new Dictionary<string, Vector3>();
        //    }
        //
        //    else
        //    {
        //        if (!BlueprintData.added)
        //        {
        //            print("Adding to runtime test");
        //            BlueprintData.RuntimeTest["Test"] = new Vector3(2, 2, 2);
        //            added = true;
        //        }
        //
        //        else
        //        {
        //            print($"Data runtime test: {BlueprintData.RuntimeTest["Test"].ToString()}");
        //        }
        //    }
        //}
        //Debug.Log(Interpreter.Instance.GetType());

        //if (BlueprintManager.blueprints == null)
        //{
        //    print("Instantiating new Blueprint manager at runtime");
        //    BlueprintManager.blueprints = new Dictionary<string, Blueprint>();
        //}
        //
        //else
        //{
        //    print("Blueprint manager dictionary is not null");
        //}


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
