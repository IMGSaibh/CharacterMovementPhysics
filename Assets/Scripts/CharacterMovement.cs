using UnityEngine;
using UnityEngine.InputSystem;

public class CharacterMovement : MonoBehaviour
{
    private DeviceInput deviceInput;
    private Vector2 deviceInputMove;
    private Vector3 velocity;
    private Vector3 projectOnPlane;
    private Quaternion rotationToCamForward;



    [Header("Physics")]
    [SerializeField] public float gravity = -9.81f;

    [Header("ChracterCollider")]
    [SerializeField, Range(0f, 1f)] float radius = 0.5f;
    [SerializeField, Range(0f, 3f)] float height = 1f;

    [Header("Move Settings")]
    [SerializeField, Range(0f, 100f)] public float maxSpeed = 10f;
    [SerializeField, Range(0f, 300f)] public float maxAcceleration = 10f;
    protected float verticalSpeed = 1f;

    [Header("Jump settings")]
    [SerializeField, Range(0f, 10f)] public float gravityScale = 1f;
    [SerializeField, Range(0f, 50f)] public float jumpSpeed = 2f;
    [SerializeField, Range(0f, 5f)] public float fallMultiplier = 2f;
    [SerializeField, Range(0f, 5f)] public float lowJumpMultiplier = 2f;
    [SerializeField, Range(0f, 5f)] public float maxAirJumps = 2f;


    private float acceleration = 0;
    private bool requireJump = false;


    private Rigidbody rBody;
    public CapsuleCollider col;
    public LayerMask goundLayers;

    // for collisions
    // bool for closetPoint algorithmus debug
    private bool closestPointContact = false;

    private Vector3 capsuleCenterDown;
    private Vector3 capsuleCenterUp;
    private bool isGrounded = false;

    private Vector3 movementVertical;

    private void Awake()
    {
        this.deviceInput = new DeviceInput();
        this.rBody = GetComponent<Rigidbody>();
        this.col.GetComponent<Collider>();


    }

    private void OnEnable()
    {
        this.deviceInput.Enable();
        this.deviceInput.Player.Move.performed += this.OnMove;
        this.deviceInput.Player.Move.canceled += this.OnMove;

        this.deviceInput.Player.WASD.performed += this.OnMoveWASD;
        this.deviceInput.Player.WASD.canceled += this.OnMoveWASD;

        this.deviceInput.Player.Jump.performed += this.OnJump;
        this.deviceInput.Player.Jump.canceled += this.OnJump;

        this.deviceInput.Player.JumpSpace.performed += this.OnJumpSpace;
        this.deviceInput.Player.JumpSpace.canceled += this.OnJumpSpace;



     

    }

    void Start()
    {
        col.radius = this.radius;
        col.height = this.height;
    }

    // Update is called once per frame
    void Update()
    {
       
        //Applyforces();
        //UpdatePositions();
        CollisionDetectionUpdate();
        //SolveConstraints();
        //DisplayResults();

    }

    private void FixedUpdate()
    {
        this.Jump();
        this.Move();
        this.MoveVertical();

    }

    private void OnAnimatorMove()
    {
        movementVertical += verticalSpeed * Vector3.up * Time.deltaTime;
        rBody.MovePosition(movementVertical);


        // If character is not on the ground then send the vertical speed to the animator.
        // This is so the vertical speed is kept when landing so the correct landing animation is played.
        //if (!isGrounded)
        //    m_Animator.SetFloat(m_HashAirborneVerticalSpeed, verticalSpeed);


    }




    private void OnMoveWASD(InputAction.CallbackContext context)
    {
        deviceInputMove =  context.ReadValue<Vector2>();
    }


    private void OnMove(InputAction.CallbackContext context)
    {
        Debug.Log("OnMove");
        this.deviceInputMove = context.ReadValue<Vector2>();
    }

    private void OnJump(InputAction.CallbackContext context) 
    {
        Debug.Log("OnJump");
        this.requireJump = context.ReadValueAsButton();
    }

    private void OnJumpSpace(InputAction.CallbackContext context) 
    {
        Debug.Log("OnJumpSpace");
        this.requireJump = context.ReadValueAsButton();

    }

    public void Move()
    {
        this.deviceInputMove = Vector2.ClampMagnitude(this.deviceInputMove, 1f) * this.maxSpeed;

        // displacement so movement will not be teleportation anymore e.g Controlling Velocity
        // see <see href="https://catlikecoding.com/unity/tutorials/movement/sliding-a-sphere/">HERE</see>

        this.acceleration = this.maxAcceleration * Time.deltaTime;

        this.velocity.x = Mathf.MoveTowards(this.velocity.x, deviceInputMove.x, acceleration);
        this.velocity.z = Mathf.MoveTowards(this.velocity.z, deviceInputMove.y, acceleration);

        // camera indepandent movement
        this.projectOnPlane = Vector3.ProjectOnPlane(Camera.main.transform.forward, Vector3.up);
        this.rotationToCamForward = Quaternion.LookRotation(this.projectOnPlane);


        Vector3 movement = this.velocity * Time.deltaTime;
        movement = this.rotationToCamForward * movement;
        rBody.MovePosition(transform.position + movement);
    }

    public void MoveVertical() 
    {

        if (requireJump && isGrounded)
        {
            verticalSpeed = jumpSpeed;
        }
        else
        {
            // If character is airborne, apply gravity.
            verticalSpeed += gravity * Time.deltaTime;
        }

    }


    public void Jump()
    {
        if (isGrounded)
        {

        }
        // jumping is all about overcome gravity
        //velocity.y += Mathf.Sqrt(-2f * Physics.gravity.y * jumpHeight);
        //rBody.velocity = velocity;

        //rBody.velocity = new Vector3(rBody.velocity.x, 0, rBody.velocity.z);
        //rBody.AddForce(Vector3.up * jumpSpeed, ForceMode.Impulse);


    }   

    /// <summary>
    /// performs all needed collsion detections
    /// </summary>
    private void CollisionDetectionUpdate() 
    {

        isGrounded = IsGrounded();

        closestPointContact = false;

        capsuleCenterUp = new Vector3(transform.position.x, transform.position.y + col.radius, transform.position.z);
        capsuleCenterDown = new Vector3(transform.position.x, transform.position.y - col.radius, transform.position.z);


        //DebugDraw.DrawMarker(capsuleCenterUp, 2.0f, Color.yellow, 0.0f, false);
        //DebugDraw.DrawMarker(capsuleCenterDown, 2.0f, Color.blue, 0.0f, false);

        foreach (Collider col in Physics.OverlapCapsule(capsuleCenterDown, capsuleCenterUp, radius))
        {
            Vector3 contactPoint = Vector3.zero;

            // for different colliders
            if (col is BoxCollider)
            {
                contactPoint = CollisionDetection.ClosestPointOn((BoxCollider)col, transform.position);
                // result of new chracter collision after collision detection
                Vector3 distance = transform.position - contactPoint;
                transform.position += Vector3.ClampMagnitude(distance, Mathf.Clamp(radius - distance.magnitude, 0, radius));
            }
            else if (col is SphereCollider)
            {
                contactPoint = CollisionDetection.ClosestPointOn((SphereCollider)col, capsuleCenterDown);
                // result of new chracter collision after collision detection
                Vector3 distance = capsuleCenterDown - contactPoint;
                transform.position += Vector3.ClampMagnitude(distance, Mathf.Clamp(radius - distance.magnitude, 0, radius));


            }
            else if(col is CapsuleCollider)
            {
                //TODO: implement logic for other players
            }


            // debug purposes
            if (contactPoint != Vector3.zero)
            {
                DebugDraw.DrawMarker(contactPoint, 2.0f, Color.red, 0.0f, false);
            }





            closestPointContact = true;
        }
    }

    private bool IsGrounded()
    {
        return Physics.CheckCapsule(capsuleCenterDown,capsuleCenterUp, radius, goundLayers);
    }

    private void OnDisable()
    {
        this.deviceInput.Disable();
    }

    void OnDrawGizmos()
    {
        if (Application.isPlaying)
        {
            Gizmos.color = closestPointContact ? Color.cyan : Color.yellow;
            Gizmos.DrawWireSphere(transform.position, radius);

            Gizmos.color = closestPointContact ? Color.red : Color.yellow;
            //Gizmos.DrawWireSphere(capsuleCenterUp, radius);
            Gizmos.DrawWireSphere(capsuleCenterDown, radius);
        }
    }
}
