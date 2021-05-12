using System.Collections;
using System.Collections.Generic;
using UnityEngine;
//using Rewired;

namespace Game
{
    public class Movement : MonoBehaviour
    {
        [Header("Core")]
        public bool movementEnabled = true;
        public bool jumpEnabled = true;
        public bool fallCheckEnabled = true;

        //So it is accessible from the inspector but protected by game-friendly functions
        [SerializeField] float moveSpeed = 10.0f;
        [SerializeField] float rotSpeed = 0.15f;
        [SerializeField] int jumpCount = 1;
        [SerializeField] float jumpForce = 300.0f;        
        internal bool isGrounded;
        int maxJumpCount;
        int initMaxJumpCount;

        [Header("Ground Collision Checking")]
        [SerializeField] float radius = 0.5f;
        [SerializeField] float distance = 3.0f;

        const float norm = 0.707f;
        new GameObject camera;
        Rigidbody rb;        
        GameObject cameraAnchor;
        Animator anim;
        bool moveInput;

        //Coroutine bools
        bool sweep = false;
        bool turn = false;

        //Init values
        float initMoveSpeed;
        float initDrag;
        float initAngularDrag;

        GameObject groundObj;

        // Start is called before the first frame update
        void Start()
        {            
            camera = GameObject.FindGameObjectWithTag("MainCamera");
            rb = GetComponent<Rigidbody>();

            initDrag = rb.drag;
            initAngularDrag = rb.angularDrag;
            initMoveSpeed = moveSpeed;
            maxJumpCount = jumpCount;
            initMaxJumpCount = maxJumpCount;

            cameraAnchor = new GameObject("Camera Anchor");
            MoveReference move = cameraAnchor.AddComponent<MoveReference>(); //I don't think this is really needed anymore
            move.cameraTarget = camera.transform;
            anim = transform.GetChild(0).GetComponent<Animator>();

        }
        
        // Update is called once per frame
        void Update()
        {
            //FallCheck();
            //MoveOnInput();
            //JumpOnInput();
            //
            //if (Input.GetKeyDown(KeyCode.F))           
            //{
            //    //Teleport(transform.TransformPoint(0, 0, 5));
            //    //TurnToFace(new Vector3(50, 10, 20), 0.1f);
            //    //TurnToFaceOverTime(new Vector3(50, 10, 20), 0.1f);
            //    Glide(10.0f, RepeatOptions.Toggle);
            //}
           
        }

        //Controlling Foot IK should be handled here in the Collision Callbacks
        private void OnCollisionEnter(Collision collision)
        {            
            RaycastHit hit;
            if (Physics.SphereCast(transform.position, radius, Vector3.down, out hit, distance))
            {
                if (hit.collider != null)
                {
                    if (hit.collider.gameObject == gameObject)
                        Debug.LogWarning("Ground Collision Check Colliding with Player");

                    jumpCount = maxJumpCount;
                    isGrounded = true;
                    anim.SetBool("Grounded", true);
                    Player.Instance.SetFootIK(true);
                    groundObj = hit.collider.gameObject;
                }
            }

        }

        private void OnCollisionExit(Collision collision)
        {           
            if (groundObj == collision.gameObject)
            {
                isGrounded = false;
                anim.SetBool("Grounded", false);
                Player.Instance.SetFootIK(false);
            }

        }

        private void OnAnimatorIK(int layerIndex)
        {
            print("Inside OnAnimatorIK");
        }

        //Returns true if input is not zero
        public bool MoveOnInput()
        {
            float x = Input.GetAxis("Horizontal");
            float y = Input.GetAxis("Vertical");

            if (movementEnabled)
            {
                moveInput = x != 0.0f || y != 0.0f;
                anim.SetBool("Move", moveInput);

                if (moveInput)
                {
                    //Vector3 dir = camera.GetComponent<CameraOrbit>().zeroAnchor.transform.TransformDirection(new Vector3(x, 0.0f, y));
                    Vector3 dir = cameraAnchor.transform.TransformDirection(new Vector3(x, 0.0f, y));
                    transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(new Vector3(dir.x, 0, dir.z)), rotSpeed);

                    Vector3 move = dir * norm * moveSpeed;
                    rb.velocity = new Vector3(move.x, rb.velocity.y, move.z);
                    return true;
                }
            }

            return false;
        }
        
        //I THINK IT WOULD BE BEST TO SET UP THE MOVEMENT SPECIFIC ANIMATOR PARAMETERS TO NOT USE TRIGGERS IF POSSIBLE
        public bool JumpOnInput()
        {
            if (jumpEnabled)
            {
                if (jumpCount > 0)
                {
                    if (Input.GetButtonDown("Jump"))
                    {
                        rb.AddForce(0.0f, jumpForce, 0.0f);                        
                        jumpCount--;
                        anim.SetTrigger("Jump");                        
                        anim.SetFloat("Y Speed", rb.velocity.y);
                        return true;
                    }
                }
            }
            return false;
        }

        //Jump with the current settings of jumpForce and jumpCount
        public void Jump()
        {
            if (jumpEnabled)
            {
                if (jumpCount > 0)
                {
                    rb.AddForce(0.0f, jumpForce, 0.0f);
                    jumpCount--;
                    anim.SetTrigger("Jump");                    
                    anim.SetFloat("Y Speed", rb.velocity.y);
                }
            }
        }

        public bool FallCheck()
        {
            if (fallCheckEnabled)
            {
                anim.SetFloat("Y Speed", rb.velocity.y);

                if (!isGrounded && rb.velocity.y < -1.0f)
                {                   
                    return true;
                }                
            }

            return false;
        }

        //BEGIN CUSTOM FUNCTIONS 

        public void LookAt(Vector3 position)
        {
            transform.LookAt(position);
        }

        public void TurnToFace(Vector3 position, float turnSpeed)
        {
            Vector3 dir = (position - transform.position).normalized;
            transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.Euler(dir), turnSpeed);            
        }

        public void TurnToFaceOverTime(Vector3 position, float turnSpeed)
        {
            if (turn)
                StopCoroutine("Turn");

            (Vector3 pos, float speed) tuple = (position, turnSpeed);

            StartCoroutine("Turn", tuple);
        }

        IEnumerator Turn((Vector3 pos, float speed) tuple)
        {
            turn = true;
            Vector3 dir = (tuple.pos - transform.position).normalized;
            Quaternion target = Quaternion.Euler(dir).normalized;

            while (true)
            {
                transform.rotation = Quaternion.Slerp(transform.rotation, target, tuple.speed);

                if (Quaternion.Dot(transform.rotation.normalized, target) > 0.9)
                    break;

                yield return null;
            }

            turn = false;
        }

        public void Launch(Vector3 force)
        {
            //Cost code to use
            //ManaManager.Instance.CheckCost(10 * (int)force.magnitude);
            rb.AddForce(force);                        
            anim.SetFloat("Y Speed", rb.velocity.y);
        }

        public void SetMoveSpeed(float speed, RepeatOptions options)
        {
            if (options == RepeatOptions.Use)
            {
                moveSpeed = speed;
                ManaManager.Instance.SetCostPerSecond("MoveSpeed", 2, RevertMoveSpeed);
            }

            if (options == RepeatOptions.Reset)
            {
                ManaManager.Instance.StopCostPerSecond("MoveSpeed");
            }

            if (options == RepeatOptions.Toggle)
            {
                if (ManaManager.Instance.CheckToggle("MoveSpeed"))
                {
                    moveSpeed = speed;
                    ManaManager.Instance.SetCostPerSecond("MoveSpeed", 2, RevertMoveSpeed);
                }
            }
        }

        void RevertMoveSpeed() { moveSpeed = initMoveSpeed; }

        public void SetMaxJumpCount(int count, RepeatOptions options)
        {
            if (options == RepeatOptions.Use)
            {
                maxJumpCount = count;
                ManaManager.Instance.SetCostPerSecond("JumpCount", 1, RevertJumpCount);
            }
            
            if (options == RepeatOptions.Reset)
            {
                ManaManager.Instance.StopCostPerSecond("JumpCount");                
            }

            if (options == RepeatOptions.Toggle)
            {
                if (ManaManager.Instance.CheckToggle("JumpCount"))
                {
                    maxJumpCount = count;
                    ManaManager.Instance.SetCostPerSecond("JumpCount", 1, RevertJumpCount);
                }
            }
        }

        void RevertJumpCount() { maxJumpCount = initMaxJumpCount; }

        public void Glide(float airResistance, RepeatOptions options)
        {
            if (options == RepeatOptions.Use)
            {
                rb.drag = airResistance;
                rb.angularDrag = airResistance;
                ManaManager.Instance.SetCostPerSecond("Glide", 1, RevertGlide);
            }
            
            if (options == RepeatOptions.Reset)
            {
                ManaManager.Instance.StopCostPerSecond("Glide");                
            }

            if (options == RepeatOptions.Toggle)
            {
                if (ManaManager.Instance.CheckToggle("Glide"))
                {
                    rb.drag = airResistance;
                    rb.angularDrag = airResistance;
                    ManaManager.Instance.SetCostPerSecond("Glide", 1, RevertGlide);
                }
            }
        }

        void RevertGlide()
        {
            rb.drag = initDrag;
            rb.angularDrag = initAngularDrag;
        }

        public void Fly(RepeatOptions options)
        {
            if (options == RepeatOptions.Use)
            {
                rb.useGravity = false;
                ManaManager.Instance.SetCostPerSecond("Fly", 10, RevertFly);
            }

            if (options == RepeatOptions.Reset)
            {
                ManaManager.Instance.StopCostPerSecond("Fly");
            }

            if (options == RepeatOptions.Toggle)
            {
                if (ManaManager.Instance.CheckToggle("Fly"))
                {
                    rb.useGravity = false;
                    ManaManager.Instance.SetCostPerSecond("Fly", 10, RevertFly);
                }
            }
        }

        void RevertFly() { rb.useGravity = true; }

        public void Teleport(Vector3 position)
        {
            Vector3 cameraPos = transform.InverseTransformPoint(Camera.main.transform.position);
            transform.position = position;
            Camera.main.transform.position = transform.TransformPoint(cameraPos);
        }
        
        public void MoveTo(Vector3 pos)
        {
            //Implement some cost here
            if (sweep)
                StopCoroutine("Sweep");

            StartCoroutine("Sweep", pos);
        }

        IEnumerator Sweep(Vector3 pos)
        {
            sweep = true;
            //float lerpSpeed = Vector3.Distance(transform.position, pos) * 0.1f;

            while (Vector3.Distance(transform.position, pos) > 1.0)
            {
                transform.position = Vector3.MoveTowards(transform.position, pos, 0.2f); //was 0.2f                                
                yield return null;
            }

            sweep = false;
        }
    }
}

/*
Rewired Reference
Declaration:
Rewired.Player player;

OnStart:
player = ReInput.players.GetPlayer(0);

OnUpdate:

float LSX = player.GetAxis("LSX");
float LSY = player.GetAxis("LSY");

//if (LSX != 0.0f || LSY != 0.0f)
*/
