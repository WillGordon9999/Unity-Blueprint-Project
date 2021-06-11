using System.Collections;
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
        public bool useUpdate = false;
        
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
        [SerializeField] float exitDist = 1.0f;
        RaycastHit groundHit;

        [Header("Animation Smoothing")]
        [SerializeField] [Range(0, 1)] float animSmoothTimeX = 0.2f;
        [SerializeField] [Range(0, 1)] float animVerticalTime = 0.2f;
        [SerializeField] [Range(0, 1)] float animStartTime = 0.3f;
        [SerializeField] [Range(0, 1)] float animStopTime = 0.15f;
        [SerializeField] [Range(0, 1)] float allowPlayerRotation = 0.1f;

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

            groundHit = new RaycastHit();
            //cameraAnchor = new GameObject("Camera Anchor");
            //MoveReference move = cameraAnchor.AddComponent<MoveReference>(); //I don't think this is really needed anymore
            //move.cameraTarget = camera.transform;
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
            if (useUpdate)
            {

                FallCheck();
                MoveOnInput();
                JumpOnInput();
            }
            
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
            //if (GroundCheck())
            //{
            //    jumpCount = maxJumpCount;
            //}

            //RaycastHit hit;
                       
            //if (Physics.SphereCast(transform.position, radius, Vector3.down, out hit, distance))
            //{
            //    
            //    if (hit.collider != null)
            //    {                    
            //        if (hit.collider.gameObject == gameObject)
            //            Debug.LogWarning("Ground Collision Check Colliding with Player");
            //
            //        jumpCount = maxJumpCount;
            //        isGrounded = true;                    
            //        anim.SetBool("Grounded", true);
            //        anim.SetBool("IsGrounded", true);
            //        anim.SetFloat("LandingVelocity", anim.GetFloat("VerticalVelocity"));
            //        //Player.Instance.SetFootIK(true);
            //        groundObj = hit.collider.gameObject;
            //    }
            //}

        }

        private void OnCollisionExit(Collision collision)
        {           
            if (groundObj == collision.gameObject)
            {
                //Vector3 close = collision.collider.ClosestPointOnBounds(transform.position);
                //
                //RaycastHit hit;
                ////Physics.SphereCast(Player.Instance.GroundCheck.position, radius, Vector3.down, out hit, exitDist);
                //Physics.SphereCast(transform.position, radius, Vector3.down, out hit, exitDist);
                //
                //Debug.DrawLine(transform.position, transform.position + Vector3.down * exitDist, Color.red);
                //
                //if (hit.collider != null)
                //{
                //    print($"exit {hit.collider.gameObject.name}");
                //}
                //
                //else
                //{
                //    print("Sucessful ground exit");
                //    isGrounded = false;
                //    anim.SetBool("Grounded", false);
                //    anim.SetBool("IsGrounded", false);
                //    //Player.Instance.SetFootIK(false);
                //}                
            }

        }

        bool GroundCheck()
        {            
            //if (Physics.SphereCast(transform.position, radius, Vector3.down, out groundHit, distance))
            if (Physics.Raycast(transform.position, Vector3.down, out groundHit, distance))
            {
                //print($"Ground Check {groundHit.collider.gameObject.name}");
                isGrounded = true;
                jumpCount = maxJumpCount;
                anim.SetBool("Grounded", true);
                anim.SetBool("IsGrounded", true);
                anim.SetFloat("LandingVelocity", anim.GetFloat("VerticalVelocity"));
                return true;
            }

            else
            {
                isGrounded = false;
                anim.SetBool("IsGrounded", false);
                return false;
            }
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
                anim.SetFloat("InputHorizontal", x, animSmoothTimeX, Time.deltaTime * 2f);
                anim.SetFloat("InputVertical", y, animVerticalTime, Time.deltaTime * 2f);
                float mag = new Vector2(x, y).magnitude;

                //Pretty Animation Smoothing stuff
                if (mag > allowPlayerRotation)
                    anim.SetFloat("InputMagnitude", mag, animStartTime, Time.deltaTime);
                else
                    anim.SetFloat("InputMagnitude", mag, animStopTime, Time.deltaTime);

                if (moveInput)
                {
                    Vector3 dir = camera.GetComponent<CameraOrbit>().zeroAnchor.transform.TransformDirection(new Vector3(x, 0.0f, y));
                    //Vector3 dir = cameraAnchor.transform.TransformDirection(new Vector3(x, 0.0f, y));
                    transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(new Vector3(dir.x, 0, dir.z)), rotSpeed);

                    float dotVal = 1.0f; //Initially

                    if (isGrounded)
                    {
                        Vector3 cross = Vector3.Cross(groundHit.normal, -transform.right);
                        dotVal = Vector3.Dot(dir.normalized, cross.normalized);
                    }

                    Vector3 move = (dir * norm * moveSpeed) * dotVal;
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
                    if (UnityEngine.Input.GetButtonDown("Jump"))
                    {                        
                        rb.AddForce(0.0f, jumpForce, 0.0f);                        
                        jumpCount--;
                        anim.SetTrigger("Jump");                        
                        anim.SetFloat("Y Speed", rb.velocity.y);
                        anim.SetFloat("VerticalVelocity", rb.velocity.y);
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
                    anim.SetFloat("VerticalVelocity", rb.velocity.y);
                }
            }
        }

        public void FallCheck()
        {
            if (fallCheckEnabled)
            {
                anim.SetFloat("Y Speed", rb.velocity.y);
                anim.SetFloat("VerticalVelocity", rb.velocity.y);

                GroundCheck();

                //if (!isGrounded && rb.velocity.y < -1.0f)
                //{                   
                //    return true;
                //}                
            }
          
            //return false
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
