using System.Collections;
using System.Collections.Generic;
//using UnityEngine;

public class GameFramework 
{
    //public static void Sweep(GameObject obj, Vector3 targetPos, float speed, BlueprintComponent comp)
    //{
    //    //float cost = 20.0f * Vector3.Distance(obj.transform.position, targetPos) * speed;
    //    float cost = 100.0f;
    //    //Debug.Log("Inside sweep");
    //
    //    if (cost > 500.0f)
    //    {
    //        //Debug.Log("Not enough Mana");
    //        comp.status = ActionStatus.Failure;
    //        return;
    //    }
    //
    //    //Debug.Log("Enough Mana");
    //    obj.transform.position = Vector3.Lerp(obj.transform.position, targetPos, speed * Time.deltaTime);
    //    comp.status = ActionStatus.Success;
    //}   
}

/*
TO-DO: Complete the transform, colliders, rigidbody, camera, material, GameObject, RaycastHit, Physics, Time, renderer, and mesh classes
*/

public interface IGameComponent
{
    //void SetOriginalComponent(UnityEngine.Component comp);

    System.Action<UnityEngine.Component> SetComponent { get; }
    //System.Func<UnityEngine.Component, object> GetObject { get; }
    
}

public class GameComponent : UnityEngine.MonoBehaviour
{
    public new Game.Transform transform { get { if (mTransform == null) mTransform = GetComponent<Game.Transform>(); return mTransform; } }
    private Game.Transform mTransform;    

    public bool active = true;
    public void Awake()
    {
        StateSetup();
    }

    public void OnEnable() //Be wary of this one
    {

    }

    public void Start()
    {

    }

    public void Update()
    {
        UpdateState();
    }

    public void FixedUpdate()
    {

    }

    public void LateUpdate()
    {

    }

    public void OnCollisionEnter(UnityEngine.Collision collision)
    {

    }

    public void OnCollisionStay(UnityEngine.Collision collision)
    {

    }

    public void OnCollisionExit(UnityEngine.Collision collision)
    {

    }

    public void OnTriggerEnter(UnityEngine.Collider other)
    {

    }

    public void OnTriggerStay(UnityEngine.Collider other)
    {

    }

    public void OnTriggerExit(UnityEngine.Collider other)
    {

    }

    public virtual void StateSetup()
    {

    }

    public virtual void EnterState()
    {
        //print("Original Enter State");
    }

    public virtual void UpdateState()
    {
        //print("Original Update State");
    }

    public virtual void ExitState()
    {
        //print("Original Exit State");
    }

    //Enforcing game types only shall also be handled on the node side
    //Only game types should be passed in
    new public T GetComponent<T>() where T : new()
    {
        if (typeof(T).Namespace != "Game" && typeof(T).BaseType != typeof(GameComponent))
        {
            UnityEngine.Debug.LogError("GetComponent trying to use class outside of Game namespace");
            return default;
        }

        //For getting basic types such as Transform, Rigidbody, Collider etc        
        if (typeof(T).GetInterface("IGameComponent") != null)
        {
            //Check to make sure the original component version exists
            UnityEngine.Component comp = base.GetComponent(typeof(T).BaseType);
            
            if (comp != null)
            {
                //So that we can get original game components and custom components without having to make custom components have IGameComponent stuff
                return GetNewGameComponent<T>(comp);
            }

            else
                return default;
        }

        //Major Concern over this for recompile
        //if (typeof(T).BaseType == typeof(GameComponent))
        //{
        //    //GetGameComponent<T>();
        //    System.Type type = ComponentInventory.Instance.SearchClass(typeof(T).Name);
        //    
        //    if (type != null)
        //    {
        //        GameComponent gameComp = (GameComponent)base.GetComponent(type);
        //        T comp = base.GetComponent<T>();
        //        object check1 = gameComp;
        //        object check2 = comp;
        //
        //        if (check1 == check2)
        //            return comp;
        //        else
        //            return (T)check1;
        //
        //    }
        //}
      
        return base.GetComponent<T>();
    }

    public GameComponent GetGameComponent(string name) 
    {
        System.Type type = ComponentInventory.Instance.SearchClass(name);

        if (type != null)
        {
            GameComponent check = (GameComponent)base.GetComponent(type);
            //T comp = base.GetComponent<T>();
            //
            //if (check == comp)
            //    return comp;
            return check;
        }

        return null;
    }

    private T GetNewGameComponent<T>(UnityEngine.Component comp) where T : new()
    {
        T gameComp = new T();
        IGameComponent iGame = (IGameComponent)gameComp;
        iGame.SetComponent(comp);
        return gameComp;
    }

    /* Old Reference for Custom Get Component
    //Search for my special private constructor in game version
    //var flags = System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.DeclaredOnly | System.Reflection.BindingFlags.Instance;
    //
    //System.Reflection.ConstructorInfo[] constructors = typeof(T).GetConstructors(flags);
    //
    //if (constructors != null)
    //    if (constructors.Length == 1)
    //        return (T)constructors[0].Invoke(new object[] { comp });
    //else
    //    return default; 
   */

    public void SetActive(bool active)
    {
        enabled = active;
    }

    //public void AddStateInput(UnityEngine.KeyCode key, StateManager.InputListener.Type type, StateManager.InputListener.KeyState state)
    //public void SetStateInput(UnityEngine.KeyCode key, string type, string state)
    public void SetStateInput(string stateName, UnityEngine.KeyCode key, StateManager.InputListener.Type type, StateManager.InputListener.KeyState state)
    {
        base.GetComponent<StateManager>().SetInput(stateName, key, type, state);
    }

    public void ChangeToDefaultInput(UnityEngine.KeyCode key, StateManager.InputListener.Type type, StateManager.InputListener.KeyState state)
    {
        base.GetComponent<StateManager>().SetInput("default", key, type, state);
    }

    public void SetDefaultInput(string stateName, UnityEngine.KeyCode key, StateManager.InputListener.Type type, StateManager.InputListener.KeyState state)
    {
        base.GetComponent<StateManager>().SetDefaultInput(stateName, key, type, state);
    }

    public void AddState(string name)
    {
        base.GetComponent<StateManager>().AddState(name, this);
        enabled = false;
    }

    public void ChangeState(string name)
    {
        base.GetComponent<StateManager>().ChangeState(name);
    }

    public void ChangeState<T>() where T: GameComponent
    {
        base.GetComponent<StateManager>().ChangeState<T>();
    }

    public void ChangeToDefaultState()
    {
        base.GetComponent<StateManager>().ChangeToDefaultState();
    }

    new public void print(object message)
    {
        UnityEngine.Debug.Log(message.ToString());
    }

    //The Decoupled Safe Way to set variables from other classes
    public virtual void SetVariable(string name, object val) 
    {
      
    }
    
    public virtual T GetVariable<T>(string name)
    {
        return default;
    }

}

namespace Game
{    
    public class Input : UnityEngine.Input
    {
        new public static UnityEngine.AccelerationEvent GetAccelerationEvent(int index) { return UnityEngine.Input.GetAccelerationEvent(index); }
        new public static float GetAxis(string axisName) { return UnityEngine.Input.GetAxis(axisName);  }
        new public static float GetAxisRaw(string axisName) { return UnityEngine.Input.GetAxisRaw(axisName); }
        new public static bool GetButton(string buttonName) { return UnityEngine.Input.GetButton(buttonName); }
        new public static bool GetButtonDown(string buttonName) { return UnityEngine.Input.GetButtonDown(buttonName); }
        new public static bool GetButtonUp(string buttonName) { return UnityEngine.Input.GetButtonUp(buttonName); }
        new public static string[] GetJoystickNames() { return UnityEngine.Input.GetJoystickNames(); }
        new public static bool GetKey(string name) { return UnityEngine.Input.GetKey(name); }
        new public static bool GetKey(UnityEngine.KeyCode key) { return UnityEngine.Input.GetKey(key); }
        new public static bool GetKeyDown(UnityEngine.KeyCode key) { return UnityEngine.Input.GetKeyDown(key); }
        new public static bool GetKeyDown(string name) { return UnityEngine.Input.GetKeyDown(name); }
        new public static bool GetKeyUp(UnityEngine.KeyCode key) { return UnityEngine.Input.GetKeyUp(key); }
        new public static bool GetKeyUp(string name) { return UnityEngine.Input.GetKeyUp(name); }
        new public static bool GetMouseButton(int button) { return UnityEngine.Input.GetMouseButton(button); }
        new public static bool GetMouseButtonDown(int button) { return UnityEngine.Input.GetMouseButtonDown(button); }
        new public static bool GetMouseButtonUp(int button) { return UnityEngine.Input.GetMouseButtonUp(button); }
        new public static UnityEngine.Touch GetTouch(int index) { return UnityEngine.Input.GetTouch(index); }
        //new public static bool IsJoystickPreconfigured(string joystickName) { return UnityEngine.Input.IsJoystickPreconfigured(joystickName); }
        new public static void ResetInputAxes() { }
    }
     
}
