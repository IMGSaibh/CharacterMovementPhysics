using UnityEngine;
using UnityEngine.InputSystem;

public class MovingSphereRigidbody : MonoBehaviour
{
    private DeviceInput deviceInput;
    private Vector2 deviceInputMove;
    private Vector3 velocity;
    private Vector3 projectOnPlane;
    private Quaternion rotationToCamForward;

    [Header("Physics")]
    [SerializeField] public float gravity = -9.81f;
    [SerializeField, Range(0f, 1f)] float radius = 0.5f;


    [Header("Move Settings")]
    [SerializeField, Range(0f, 100f)] public float maxSpeed = 10f;
    [SerializeField, Range(0f, 300f)] public float maxAcceleration = 10f;

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
    // bool for closetPoint algorithmus
    private bool closestPointContact = false;

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

    }

    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
       
        //Applyforces();
        //UpdatePositions();
        CollisionDetection();
        //SolveConstraints();
        //DisplayResults();

    }

    private void FixedUpdate()
    {


        this.Jump();
        this.Move();

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


        Vector3 movementSteps = this.velocity * Time.deltaTime;
        movementSteps = this.rotationToCamForward * movementSteps;
        rBody.MovePosition(transform.position + movementSteps);
    }

    public void Jump()
    {
        
        // jumping is all about overcome gravity
        //velocity.y += Mathf.Sqrt(-2f * Physics.gravity.y * jumpHeight);
        //rBody.velocity = velocity;

        //rBody.velocity = new Vector3(rBody.velocity.x, 0, rBody.velocity.z);
        //rBody.AddForce(Vector3.up * jumpSpeed, ForceMode.Impulse);


    }   

    /// <summary>
    /// performs all needed collsion detections
    /// </summary>
    private void CollisionDetection() 
    {

        closestPointContact = false;
        DebugDraw.DrawMarker(col.center, 2.0f, Color.yellow, 0.0f, false);
        DebugDraw.DrawMarker(col.bounds.center * 0.5f, 2.0f, Color.blue, 0.0f, false);

        foreach (Collider col in Physics.OverlapCapsule(col.bounds.max, col.bounds.min, radius))
        {
            Vector3 contactPoint = Vector3.zero;

            // for different colliders
            if (col is BoxCollider)
            {
                contactPoint = ColliosionDetection.ClosestPointOn((BoxCollider)col, transform.position);
            }
            else if (col is SphereCollider)
            {
                contactPoint = ColliosionDetection.ClosestPointOn((SphereCollider)col, transform.position);
            }

            // debug purposes
            //DebugDraw.DrawMarker(contactPoint, 2.0f, Color.red, 0.0f, false);

            // result 
            Vector3 distance = transform.position - contactPoint;

            transform.position += Vector3.ClampMagnitude(distance, Mathf.Clamp(radius - distance.magnitude, 0, radius));

            closestPointContact = true;

        }
    }

    private void OnDisable()
    {
        this.deviceInput.Disable();
    }

    void OnDrawGizmos()
    {
        Gizmos.color = closestPointContact ? Color.cyan : Color.yellow;
        Gizmos.DrawWireSphere(transform.position, radius);
    }
}
