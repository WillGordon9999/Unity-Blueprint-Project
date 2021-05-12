using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Game;
public class StateManager : MonoBehaviour
{
    public class InputListener
    {
        public enum Type { Enter, Exit, Toggle};
        public enum KeyState {Down, Up};
        public KeyState keyState;
        public KeyCode key;
        public Type type;
        public GameComponent component;
        public string stateName;

        //public InputListener(Type t, KeyCode code, KeyState state, GameComponent comp)
        public InputListener(string name, Type t, KeyCode code, KeyState state, GameComponent comp)
        {
            stateName = name;
            type = t;
            key = code;
            keyState = state;
            component = comp;
        }

    }
    
    public static StateManager Instance { get { return mInstance; } private set { } }
    private static StateManager mInstance;

    Dictionary<KeyCode, InputListener> inputs;
    Dictionary<string, GameComponent> states;
    GameComponent currentState;
    GameComponent defaultState;
    public List<InputListener> currentInputs;
    public List<InputListener> defaultInputs;

    private void Awake()
    {
        if (mInstance == null)
            mInstance = this;
    }

    private void OnEnable()
    {
        inputs = new Dictionary<KeyCode, InputListener>();
        states = new Dictionary<string, GameComponent>();

        defaultState = gameObject.AddComponent<GameComponent>();
        states["default"] = defaultState;
        currentState = defaultState;

        currentInputs = new List<InputListener>();
        defaultInputs = new List<InputListener>();
    }

    // Start is called before the first frame update
    void Start()
    {
        //Debug.Log("Start has fired");
    }

    // Update is called once per frame
    void Update()
    {
        //Safety in case active state gets recompiled and reloaded
        if (currentState == null)
            currentState = defaultState;

        List<InputListener> listeners = null;

        if (currentState == defaultState)
            listeners = defaultInputs;
        else
            listeners = currentInputs;

        foreach(InputListener input in listeners)
        {
            if (input.keyState == InputListener.KeyState.Down)
            {
                if (UnityEngine.Input.GetKeyDown(input.key))
                {
                    ChangeState(input.stateName);
                    break;
                }
            }

            if (input.keyState == InputListener.KeyState.Up)
            {
                if (UnityEngine.Input.GetKeyUp(input.key))
                {
                    ChangeState(input.stateName);
                    break;
                }
            }
        }


        //Debug.Log("Update has begun");
        //foreach(InputListener input in inputs.Values)
        //{
        //    if (input.keyState == InputListener.KeyState.Down)
        //    {
        //        if (Input.GetKeyDown(input.key))
        //        {
        //            if (input.type == InputListener.Type.Enter)
        //            {
        //                if (currentState != null)
        //                {
        //                    currentState.ExitState();
        //                    currentState.enabled = false;
        //                }
        //                input.component.enabled = true;
        //                input.component.EnterState();
        //                currentState = input.component;
        //            }
        //
        //            if (input.type == InputListener.Type.Exit)
        //            {                        
        //                input.component.ExitState();
        //                input.component.enabled = false;
        //                currentState = null;
        //            }
        //
        //            if (input.type == InputListener.Type.Toggle)
        //            {
        //                if (!input.component.enabled)
        //                {
        //                    if (currentState != null)
        //                    {
        //                        currentState.ExitState();
        //                        currentState.enabled = false;
        //                    }
        //
        //                    input.component.EnterState();
        //                    currentState = input.component;
        //                }
        //                else
        //                {
        //                    input.component.ExitState();
        //
        //                    if (currentState != null)
        //                        currentState = null;
        //                }
        //
        //                input.component.enabled = !input.component.enabled;
        //            }
        //        }
        //    }
        //
        //    if (input.keyState == InputListener.KeyState.Up)
        //    {
        //        if (Input.GetKeyUp(input.key))
        //        {
        //            if (input.type == InputListener.Type.Enter)
        //            {
        //                if (currentState != null)
        //                {
        //                    currentState.ExitState();
        //                    currentState.enabled = false;
        //                }
        //
        //                input.component.enabled = true;
        //                input.component.EnterState();
        //            }
        //
        //            if (input.type == InputListener.Type.Exit)
        //            {
        //                input.component.enabled = false;
        //                input.component.ExitState();
        //                currentState = null;
        //            }                   
        //        }
        //    }
        //}
    }

    //public void SetInput(InputListener.Type type, KeyCode code, InputListener.KeyState state, GameComponent comp)
    //{
    //    inputs[code] = new InputListener(type, code, state, comp);        
    //}

    public void SetInput(string stateName, KeyCode code, InputListener.Type type, InputListener.KeyState state)
    {
        //GameComponent comp;
        //if (states.TryGetValue(stateName, out comp))
        //{
        //    Debug.Log($"Adding a new input for {stateName}");
        //    inputs[code] = new InputListener(stateName, type, code, state, comp);
        //}
        foreach(InputListener input in currentInputs)
        {
            if (input.key == code)
            {
                input.stateName = stateName;
                input.type = type;
                input.keyState = state;
                return;
            }
        }

        //Debug.Log("Adding to current inputs");
        currentInputs.Add(new InputListener(stateName, type, code, state, states[stateName]));       
    }

    public void SetDefaultInput(string stateName, KeyCode code, InputListener.Type type, InputListener.KeyState state)
    {
        foreach (InputListener input in defaultInputs)
        {
            if (input.key == code)
            {
                input.stateName = stateName;
                input.type = type;
                input.keyState = state;
                return;
            }
        }

        //Debug.Log("Adding to Default Inputs");
        defaultInputs.Add(new InputListener(stateName, type, code, state, states[stateName]));
    }
   
    public void AddState(string name, GameComponent comp)
    {
        Debug.Log($"Adding state {name}");
        states[name] = comp;      
    }

    public void ChangeToDefaultState()
    {
        if (currentState == defaultState)
            return;

        if (currentState != null)
        {
            currentState.ExitState();
            currentState.enabled = false;
        }

        currentState = defaultState;
        currentState.enabled = true;
        currentInputs.Clear(); 
        //Doesn't need to enter state it's empty
    }

    public void ChangeState(string name)
    {
        GameComponent comp;

        if (states.TryGetValue(name, out comp))
        {
            if (currentState != null)
            {
                currentState.ExitState();
                currentState.enabled = false;
            }
           
            currentInputs.Clear();                                       
            currentState = comp;
            currentState.enabled = true;
            currentState.EnterState();
        }
    }


    //Warning, if the state in question is recompiled, any other script calling this function 
    //will have to be recompiled as well to call the correct version   
    //public void ChangeState<T>() where T : GameComponent
    //{
    //    GameComponent comp;
    //    System.Type type = typeof(T);
    //
    //    if (states.TryGetValue(type.Name, out comp))
    //    {
    //        if (currentState != null)
    //        {
    //            currentState.ExitState();
    //            currentState.enabled = false;
    //        }
    //
    //        currentInputs.Clear();
    //        currentState = comp;
    //        currentState.enabled = true;
    //        currentState.EnterState();
    //    }
    //
    //    else
    //    {
    //        //states[type.Name] = (GameComponent)gameObject.AddComponent(type);
    //        ComponentInventory.Instance.AddClassToInventoryFromState<T>();
    //        states[type.Name] = gameObject.AddComponent<T>();
    //        currentInputs.Clear();
    //        currentState = states[type.Name];
    //        currentState.enabled = true;
    //        currentState.EnterState();
    //    }
    //}

    public void ChangeState<T>() where T: GameComponent
    {
        System.Type type = ComponentInventory.Instance.SearchClass(typeof(T).Name);

        if (type != null)
        {
            if (currentState != null)
            {
                currentState.ExitState();
                currentState.enabled = false;
            }

            GameComponent comp = (GameComponent)GetComponent(type);

            if (comp != null)
            {
                currentInputs.Clear(); //Just in case
                currentState = comp;
                currentState.enabled = true;
                currentState.EnterState();
            }

            else
            {
                ComponentInventory.Instance.AddClassToInventoryFromType(type);
                currentState = (GameComponent)gameObject.AddComponent(type);
                currentState.enabled = true;
                currentState.EnterState();
            }

        }
    }

    //For use on Recompile
    public void RemoveState(string name)
    {
        GameComponent comp = (GameComponent)GetComponent(name);

        if (comp != null)
        {
            if (comp == currentState)
                ChangeToDefaultState();
        }

        //if (states.TryGetValue(name, out comp))
        //{
        //    if (comp == currentState)
        //    {
        //        ChangeToDefaultState();                
        //    }
        //    print("Removing State from StateManager");
        //    states.Remove(name);
        //}
    }

}
