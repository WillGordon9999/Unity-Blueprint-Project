using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Right now this will just be used for animations
public class PlayerStateMachine : MonoBehaviour
{
    StateMachine<PlayerStateMachine> stateMachine;
    [HideInInspector] public Animator animator;    
    [HideInInspector] public Game.Movement move;
    [HideInInspector] public Rigidbody rb;
    [HideInInspector] public Game.Combat combat;

    public static PlayerStateMachine Instance { get { return mInstance; } private set { } }
    private static PlayerStateMachine mInstance;

    private void Awake()
    {
        if (mInstance == null)
            mInstance = this;

        stateMachine = new StateMachine<PlayerStateMachine>(this);
        animator = transform.GetChild(0).GetComponent<Animator>();        
        move = GetComponent<Game.Movement>();
        rb = GetComponent<Rigidbody>();
        combat = GetComponent<Game.Combat>();
    }

    // Start is called before the first frame update
    void Start()
    {
        stateMachine.ChangeState<MainState>();        
    }

    // Update is called once per frame
    void Update()
    {
        stateMachine.Update();        
    }

    public void OnCollisionEnter(Collision collision)
    {        
         stateMachine.OnCollisionEnter(this, collision);
    }

    public void OnCollisionStay(Collision collision)
    {       
        stateMachine.OnCollisionStay(this, collision);
    }
    public void OnCollisionExit(Collision collision)
    {
        stateMachine.OnCollisionExit(this, collision);
    }

    public void OnTriggerEnter(Collider collider)
    {
        stateMachine.OnTriggerEnter(this, collider);
    }

    public void OnTriggerStay(Collider collider)
    {
        stateMachine.OnTriggerStay(this, collider);
    }

    public void OnTriggerExit(Collider collider)
    {
        stateMachine.OnTriggerExit(this, collider);
    }

    public void ChangeState<T>() where T : State<PlayerStateMachine>, new()
    {
        //In case we want to include extra guards here later
        stateMachine.ChangeState<T>();
    }

    public bool CheckState<T>() where T : State<PlayerStateMachine>
    {
        return stateMachine.CheckState<T>();
    }    
}
