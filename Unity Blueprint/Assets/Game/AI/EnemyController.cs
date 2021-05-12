using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyController : MonoBehaviour
{
    public StateMachine<EnemyController> stateMachine;
    [HideInInspector] public Rigidbody rb;
    [HideInInspector] public EnemySense sense;
    [HideInInspector] public GameObject target;

    [Header("Movement")]
    public float moveSpeed = 5.0f;
    public float rotateSpeed = 0.25f;

    [Header("Combat")]
    public float attackRange = 2.0f;
    public float attackCooldown = 0.5f;
    public float attackForce = 100.0f;
    public float timeUntilLost = 5.0f;    

    [Header("Patrol")]
    public bool patrol = true;
    public List<GameObject> patrolPoints;
    public float waitTimeAtPoint = 3.0f;
    public float timeUntilAlert = 1.0f;

    //Stagger
    [HideInInspector] public bool canAct = true;
   
    // Start is called before the first frame update
    void Start()
    {
        stateMachine = new StateMachine<EnemyController>(this);
        rb = GetComponent<Rigidbody>();
        sense = GetComponent<EnemySense>();
        target = Player.Instance.gameObject;
        stateMachine.ChangeState<EnemyPatrolState>();
    }

    // Update is called once per frame
    void Update()
    {
        stateMachine.Update();
    }

    private void OnCollisionEnter(Collision collision)
    {
        stateMachine.OnCollisionEnter(this, collision);
    }

    private void OnCollisionStay(Collision collision)
    {
        stateMachine.OnCollisionStay(this, collision);
    }

    private void OnCollisionExit(Collision collision)
    {
        stateMachine.OnCollisionExit(this, collision);
    }

    private void OnTriggerEnter(Collider other)
    {
        stateMachine.OnTriggerEnter(this, other);
    }

    private void OnTriggerStay(Collider other)
    {
        stateMachine.OnTriggerStay(this, other);
    }

    private void OnTriggerExit(Collider other)
    {
        stateMachine.OnTriggerExit(this, other);
    }

    public void Attack(GameObject target, float knockForce)
    {
        Rigidbody rb = target.GetComponent<Rigidbody>();

        if (rb != null)
        {
            rb.AddForce(transform.TransformDirection(Vector3.forward) * knockForce);
            rb.gameObject.GetComponent<Game.Combat>().StartStaggerTime(0.1f);
        }

    }

    public void AddStaggerTime(float time)
    {
        if (stateMachine.CheckState<EnemyStaggerState>())
        {
            StopCoroutine(StaggerTimer(time));
            StartCoroutine(StaggerTimer(time));
        }

        else
        {            
            stateMachine.ChangeState<EnemyStaggerState>();
            StartCoroutine(StaggerTimer(time));
        }
    }

    IEnumerator StaggerTimer(float time)
    {
        canAct = false;
        yield return new WaitForSeconds(time);
        canAct = true;
    }
}
