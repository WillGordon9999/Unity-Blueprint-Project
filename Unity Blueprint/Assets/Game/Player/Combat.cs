using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Game
{
    public class Combat : MonoBehaviour
    {
        public enum Side { Left, Right };
        public bool combatEnabled = true;
        Animator anim;        
        // Start is called before the first frame update
        void Start()
        {
            //anim = transform.GetChild(0).GetComponent<Animator>();
            if (anim == null)
            {
                Animator check;

                if (transform.TryGetComponent<Animator>(out check))
                    anim = check;
                else
                {
                    anim = transform.GetComponentInChildren<Animator>();
                }
            }
        }

        // Update is called once per frame
        void Update()
        {

        }

        //Slash(Side.Right, startPos, startRot, endPos, endRot);                

        public void AttackOnInput(float force)
        {
            if (Input.GetMouseButtonDown(0))
            {
                anim.SetTrigger("Attack");
                Vector3 pos = transform.TransformPoint(Vector3.forward);
                Vector3 dir = transform.TransformDirection(Vector3.forward);
                ForceShot(pos, dir, 2.0f, dir * force);
            }
        }

        public void Attack(float force)
        {
            anim.SetTrigger("Attack");
            Vector3 pos = transform.TransformPoint(Vector3.forward);
            Vector3 dir = transform.TransformDirection(Vector3.forward);
            ForceShot(pos, dir, 2.0f, dir * force);
        }

        public void StartStaggerTime(float time)
        {
            if (PlayerStateMachine.Instance.CheckState<DamageState>())
            {                
                StopCoroutine("DamageTimer");
                StartCoroutine("DamageTimer", time);
            }

            else
            {
                PlayerStateMachine.Instance.ChangeState<DamageState>();
                StartCoroutine("DamageTimer", time);
            }
        }

        IEnumerator DamageTimer(float time)
        {
            yield return new WaitForSeconds(time);            
            PlayerStateMachine.Instance.ChangeState<MainState>();
            print("Exiting Damage State");
        }

        public void ForceShot(Vector3 origin, Vector3 direction, float distance, Vector3 force)
        {
            RaycastHit hit;
            if (Physics.Raycast(origin, direction, out hit, distance))
            {
                if (hit.collider != null)
                {
                    Rigidbody rb = hit.collider.gameObject.GetComponent<Rigidbody>();
                    if (rb != null) rb.AddForce(force);
                    rb.gameObject.SendMessage("AddStaggerTime", 2.0f);
                }
            }
        }

        public void ForceWave(Vector3 origin, Vector3 direction, float radius, float distance, Vector3 force)
        {
            RaycastHit hit;
            if (Physics.SphereCast(origin, radius, direction, out hit, distance))
            {
                if (hit.collider != null)
                {
                    Rigidbody rb = hit.collider.gameObject.GetComponent<Rigidbody>();
                    if (rb != null) rb.AddForce(force);
                }
            }
        }

        public void ExplosionShot(Vector3 origin, Vector3 direction, float radius, float distance, float force)
        {
            RaycastHit hit;
            if (Physics.Raycast(origin, direction, out hit, distance))
            {
                if (hit.collider != null)
                {
                    Collider[] hits = Physics.OverlapSphere(hit.point, radius);

                    foreach(Collider col in hits)
                    {
                        Rigidbody rb = col.gameObject.GetComponent<Rigidbody>();

                        if (rb != null) rb.AddExplosionForce(force, hit.point, radius);
                    }
                }
            }
        }

        public void CreateExplosion(Vector3 position, float radius, float force)
        {
            Collider[] hits = Physics.OverlapSphere(position, radius);

            foreach (Collider col in hits)
            {
                Rigidbody rb = col.gameObject.GetComponent<Rigidbody>();

                if (rb != null) rb.AddExplosionForce(force, position, radius);
            }
        }
    }
}
