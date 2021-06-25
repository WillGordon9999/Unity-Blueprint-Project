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
        public bool groundCheckEnabled = true;
        
        //So it is accessible from the inspector but protected by game-friendly functions
        [SerializeField] float moveSpeed = 10.0f;
        [SerializeField] float rotSpeed = 0.15f;
        [SerializeField] int maxJumpCount;
        [SerializeField] float jumpForce = 300.0f;        
        int jumpCount = 1;
        internal bool isGrounded;
        internal bool jump; //The input specifically
        int initMaxJumpCount;

        [Header("Ground Collision Checking")]
        [SerializeField] float radius = 0.5f;
        [SerializeField] float distance = 3.0f;
        [SerializeField] float jumpDist = 0.1f;
        RaycastHit groundHit;
        float origDist;

        [Header("Animation Smoothing")]
        [SerializeField] [Range(0, 1)] float animSmoothTimeX = 0.2f;
        [SerializeField] [Range(0, 1)] float animVerticalTime = 0.2f;
        [SerializeField] [Range(0, 1)] float animStartTime = 0.3f;
        [SerializeField] [Range(0, 1)] float animStopTime = 0.15f;
        [SerializeField] [Range(0, 1)] float allowPlayerRotation = 0.1f;

        [Header("Debug")]
        public Vector3 moveDebug;
        public Vector3 groundNormal;
        public Vector3 relativeVelocity;
        public Vector3 debugVelocity;
        public bool useNormal;
        public bool groundedDebug;

        [Header("Gravity")]
        public Vector3 customGravity = Vector3.one;
        [SerializeField] float gravityRotSpeed = 0.15f;
        [SerializeField] bool gravityRotation = false;
        GameObject gravityParent;
        [SerializeField] float verticalVelocity; //The relative vertical velocity of the character, for animation control

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
        Vector3 initGravity;

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

            //Ground Check
            origDist = distance;
            groundHit = new RaycastHit();

            //Gravity
            initGravity = Physics.gravity;
            
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
                JumpOnInput();
                MoveOnInput();
            }

            groundedDebug = isGrounded;

            if (Input.GetKeyDown(KeyCode.R))           
            {
                //Teleport(transform.TransformPoint(0, 0, 5));
                //TurnToFace(new Vector3(50, 10, 20), 0.1f);
                //TurnToFaceOverTime(new Vector3(50, 10, 20), 0.1f);
                //Glide(10.0f, RepeatOptions.Toggle);
                SetGravity(customGravity, RepeatOptions.Use);
                
                //Quaternion rot = Quaternion.LookRotation(transform.TransformDirection(Vector3.forward), -Physics.gravity);                
                transform.rotation = Quaternion.LookRotation(transform.TransformDirection(Vector3.forward), -Physics.gravity.normalized);
                //transform.rotation = Quaternion.Slerp(transform.rotation, rot, gravityRotSpeed);
            }
        }

        //Controlling Foot IK should be handled here in the Collision Callbacks
        private void OnCollisionEnter(Collision collision)
        {
            
        }

        private void OnCollisionExit(Collision collision)
        {           
            if (groundObj == collision.gameObject)
            {
                
            }
        }

        bool GroundCheck()
        {
            //if (Physics.SphereCast(transform.position, radius, Vector3.down, out groundHit, distance))
            //Debug.DrawLine(transform.position, transform.position + Vector3.down * distance, Color.red);
            Debug.DrawRay(transform.position, transform.TransformDirection(Vector3.down), Color.red);

            //if (Physics.Raycast(transform.position, Vector3.down, out groundHit, distance))
            if (Physics.Raycast(transform.position, transform.TransformDirection(Vector3.down), out groundHit, distance))

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

                groundNormal = groundHit.normal;

                if (moveInput)
                {
                    CameraOrbit orbit = camera.GetComponent<CameraOrbit>();
                    UnityEngine.Transform zeroAnchor = orbit.zeroAnchor.transform;
                    //Vector3 right = zeroAnchor.TransformDirection(Vector3.right) * x;
                    //Vector3 fwd = zeroAnchor.TransformDirection(Vector3.forward) * y;
                 
                    Vector3 dir = zeroAnchor.TransformDirection(new Vector3(x, 0.0f, y));
                    if (dir.magnitude > 1) dir.Normalize();

                    orbit.targetRot = Quaternion.LookRotation(dir, -Physics.gravity.normalized);
                    
                    transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(dir, -Physics.gravity.normalized), rotSpeed);
                    //Maybe try getting the cross product of the up direction or maybe try projecting gravity onto input vector?
                    Vector3 move = dir;

                    float dotVal = 1.0f;

                    if (isGrounded)
                    {
                        move = Vector3.ProjectOnPlane(move, groundHit.normal);

                        Vector3 cross = Vector3.Cross(groundHit.normal, -transform.right);
                        dotVal = Vector3.Dot(move, cross.normalized);                        

                        move = move * moveSpeed * dotVal;

                        moveDebug = move;
                        rb.position += move * Time.deltaTime;    
                        //rb.MovePosition(transform.position + move * Time.deltaTime);
                        //rb.velocity = move;
                    }

                    else
                    {
                        move *= moveSpeed;                        
                        moveDebug = move * Time.deltaTime;                        

                        rb.position += move * Time.deltaTime;
                        //rb.MovePosition(transform.position + move * Time.deltaTime);
                    }

                    return true;
                }
            }

            return false;
        }
                
        public bool JumpOnInput()
        {
            if (jumpEnabled)
            {
                if (jumpCount > 0)
                {                    
                    if (UnityEngine.Input.GetButtonDown("Jump"))
                    {                        
                        rb.AddRelativeForce(0.0f, jumpForce, 0.0f);
                        distance = jumpDist;
                        jumpCount--;
                        anim.SetTrigger("Jump");                        
                        anim.SetFloat("Y Speed", rb.velocity.y);
                        //anim.SetFloat("VerticalVelocity", rb.velocity.y);

                        GetVerticalVelocity();
                        anim.SetFloat("VerticalVelocity", verticalVelocity);

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
                    rb.AddRelativeForce(0.0f, jumpForce, 0.0f);
                    jumpCount--;
                    distance = jumpDist;
                    anim.SetTrigger("Jump");                    
                    anim.SetFloat("Y Speed", rb.velocity.y);
                    //anim.SetFloat("VerticalVelocity", rb.velocity.y);

                    GetVerticalVelocity();
                    anim.SetFloat("VerticalVelocity", verticalVelocity);
                }
            }
        }

        float GetVerticalVelocity()
        {
            relativeVelocity = rb.GetRelativePointVelocity(transform.up);
            Vector3 vel = Vector3.Project(relativeVelocity, -Physics.gravity.normalized);

            if (vel.x != 0.0f) verticalVelocity = vel.x;
            if (vel.y != 0.0f) verticalVelocity = vel.y;
            if (vel.z != 0.0f) verticalVelocity = vel.z;

            //if (Mathf.Abs(vel.x) >= 1.0f) verticalVelocity = vel.x;
            //if (Mathf.Abs(vel.y) >= 1.0f) verticalVelocity = vel.y;
            //if (Mathf.Abs(vel.z) >= 1.0f) verticalVelocity = vel.z;

            return verticalVelocity;
        }

        public void FallCheck()
        {
            if (fallCheckEnabled)
            {
                anim.SetFloat("Y Speed", rb.velocity.y);

                GetVerticalVelocity();

                anim.SetFloat("VerticalVelocity", verticalVelocity);

                //Reset raycast collision distance
                if (distance != origDist)
                {
                    if (verticalVelocity < 0)
                        distance = origDist;
                }
            }

            if (groundCheckEnabled)
                GroundCheck();

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

        public void SetGravity(Vector3 gravity, RepeatOptions options)
        {
            if (options == RepeatOptions.Use)
            {
                Physics.gravity = gravity;
                ManaManager.Instance.SetCostPerSecond("SetGravity", 10, RevertGravity);

                //if (gravityRotation) gravityRotation = false;
                //StartCoroutine(RotateToGravity(Physics.gravity));
            }

            if (options == RepeatOptions.Reset)
            {
                //if (gravityRotation) gravityRotation = false;
                ManaManager.Instance.StopCostPerSecond("SetGravity");
                //StartCoroutine(RotateToGravity(Physics.gravity)); //Return to original rotation
            }

            if (options == RepeatOptions.Toggle)
            {
                if (ManaManager.Instance.CheckToggle("SetGravity"))
                {
                    Physics.gravity = gravity;
                    ManaManager.Instance.SetCostPerSecond("SetGravity", 10, RevertGravity);
                    //if (gravityRotation) gravityRotation = false;
                    //StartCoroutine(RotateToGravity(gravity));
                }               
            }
        }

        void RevertGravity() { Physics.gravity = initGravity; }

        IEnumerator RotateToGravity(Vector3 gravity)
        {
            while (gravityRotation)
                print("Waiting on gravityRotation to unset");

            gravityRotation = true;
            gravity.Normalize();        
            gravityParent.transform.parent = null;
            gravityParent.transform.rotation = Quaternion.LookRotation(transform.TransformDirection(Vector3.up));
            transform.parent = gravityParent.transform;
            Quaternion newRot = Quaternion.LookRotation(-gravity).normalized;            

            //while (gravityRotation || Quaternion.Dot(transform.rotation, newRot) < 0.9f)

            while (gravityRotation || Quaternion.Dot(gravityParent.transform.rotation.normalized, newRot) < 0.9f)
            {
                print("Rotating with gravity");
                //transform.rotation = Quaternion.Slerp(transform.rotation, newRot, rotSpeed);
                gravityParent.transform.rotation = Quaternion.Slerp(gravityParent.transform.rotation, newRot, gravityRotSpeed);
                //camera.transform.rotation = Quaternion.Slerp(camera.transform.rotation, newRot, gravityRotSpeed);
                yield return null;
            }

            transform.parent = null;
            gravityParent.transform.parent = transform;
            gravityRotation = false;
            print("End gravity rotate");
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
