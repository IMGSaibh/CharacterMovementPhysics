using UnityEngine;
using UnityEngine.InputSystem;

public class CharacterMovement : MonoBehaviour
{
    private DeviceInput deviceInput;
    private Vector2 deviceInputMove;
    private Vector3 projectOnPlane;
    private Quaternion rotationToCamForward;

    
    public LayerMask goundLayers;
    private Rigidbody rBody;
    public CapsuleCollider capsuleCollider;
    private Vector3 capsuleCenterDown;
    private Vector3 capsuleCenterUp;

    
    private bool requireJump = false;
    private bool isGrounded = false;


    [Header("Physics")]
    [SerializeField] public float gravity;

    [Header("ChracterCollider")]
    [SerializeField, Range(0f, 1f)] float radius = 0.5f;
    [SerializeField, Range(0f, 3f)] float height = 1f;

    [Header("Move Settings")]
    [SerializeField, Range(0f, 100f)] public float maxHorizontalSpeed = 10f;
    [SerializeField, Range(0f, 300f)] public float maxAcceleration = 10f;
    private float acceleration = 0;
    public float jumpSpeed = 10f;                                                       // How fast character takes off when jumping.
    protected float verticalSpeed;                                                      // How fast character is currently moving up or down.
    
    private Vector3 horizontalMovement;
    private Vector3 verticalMovement;


    [Header("Jump settings")]
    //[SerializeField, Range(0f, 10f)] public float gravityScale = 1f;
    //[SerializeField, Range(0f, 50f)] public float jumpSpeed = 2f;
    //[SerializeField, Range(0f, 5f)] public float fallMultiplier = 2f;
    //[SerializeField, Range(0f, 5f)] public float lowJumpMultiplier = 2f;
    //[SerializeField, Range(0f, 5f)] public float maxAirJumps = 2f;




    // don't change them without fully understanding what they do in code.
    const float c_StickingGravityProportion = 0.3f;
    const float c_JumpAbortSpeed = 10f;


    // bool for closetPoint algorithmus debug
    private bool closestPointContact = false;


    private void Awake()
    {
        this.deviceInput = new DeviceInput();
        this.rBody = GetComponent<Rigidbody>();
        this.capsuleCollider.GetComponent<Collider>();

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
        capsuleCollider.radius = this.radius;
        capsuleCollider.height = this.height;
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
        this.MoveHorizontal();
        this.MoveVertical();

        if (isGrounded)
        {
        }
        else
        {
            verticalMovement += verticalSpeed * Vector3.up * Time.deltaTime;
            rBody.MovePosition(transform.position + verticalMovement);
        }
    }

    private void OnAnimatorMove()
    {
        Debug.Log("OnAnimatorMove");
    }

    public void MoveHorizontal()
    {
      
        deviceInputMove = Vector2.ClampMagnitude(deviceInputMove, 1f) * maxHorizontalSpeed;

        acceleration = this.maxAcceleration * Time.deltaTime;

        horizontalMovement.x = Mathf.MoveTowards(horizontalMovement.x, deviceInputMove.x, acceleration);
        horizontalMovement.z = Mathf.MoveTowards(horizontalMovement.z, deviceInputMove.y, acceleration);

        // camera indepandent movement
        projectOnPlane = Vector3.ProjectOnPlane(Camera.main.transform.forward, Vector3.up);
        rotationToCamForward = Quaternion.LookRotation(projectOnPlane);

        Vector3 movement = horizontalMovement * Time.deltaTime;
        movement = rotationToCamForward * movement;
        rBody.MovePosition(transform.position + movement);
    }

    public void MoveVertical() 
    {

        if (isGrounded)
        {
            // When grounded we apply a slight negative vertical speed to make character "stick" to the ground.
            verticalSpeed = -gravity * c_StickingGravityProportion;

            if (requireJump)
            {
                // override the previously set vertical speed and make sure she cannot jump again.
                verticalSpeed = jumpSpeed;
                isGrounded = false;
                Debug.Log("requireJump");

            }

        }
        else
        {
            // If Character is airborne, the jump button is not held and Ellen is currently moving upwards...
            if (requireJump && verticalSpeed > 0.0f)
            {
                // ... decrease Character's vertical speed.
                // This is what causes holding jump to jump higher that tapping jump.
                verticalSpeed -= c_JumpAbortSpeed * Time.deltaTime;
                Debug.Log("requireJump && verticalSpeed > 0.0f");

            }

            // If a jump is approximately peaking, make it absolute.
            if (Mathf.Approximately(verticalSpeed, 0f))
            { 
                verticalSpeed = 0f;
                Debug.Log("Mathf.Approximately(verticalSpeed, 0f)");

            }
            // If Chracter is airborne, apply gravity.
            verticalSpeed -= gravity * Time.deltaTime;
            Debug.Log("isGrounded is false");


        }

    }


    /// <summary>
    /// performs all needed collsion detections
    /// </summary>
    private void CollisionDetectionUpdate() 
    {

        isGrounded = IsGrounded();

        //Only for debug
        closestPointContact = false;

        Vector3 contactPoint = Vector3.zero;

        //capsuleCenterUp = new Vector3(transform.position.x, transform.position.y + capsuleCollider.radius, transform.position.z);
        //capsuleCenterDown = new Vector3(transform.position.x, transform.position.y - capsuleCollider.radius, transform.position.z);
        capsuleCenterUp = new Vector3(transform.position.x, transform.position.y + capsuleCollider.height - capsuleCollider.radius, transform.position.z);
        capsuleCenterDown = new Vector3(transform.position.x, transform.position.y + capsuleCollider.radius, transform.position.z);

        //transform.position += CollisionDetection.OverlappingSphere(capsuleCenterUp, radius, ref contactPoint);
        transform.position += CollisionDetection.OverlappingSphere(capsuleCenterDown, radius, ref contactPoint);

        // debug purposes
        if (contactPoint != Vector3.zero)
        {
            DebugDraw.DrawMarker(contactPoint, 2.0f, Color.red, 0.0f, false);
            closestPointContact = true;
        }

        //foreach (Collider col in Physics.OverlapSphere(capsuleCenterDown, radius))
        //{
        //     for different colliders
        //    if (col is BoxCollider)
        //    {
        //        contactPoint = CollisionDetection.ClosestPointOn((BoxCollider)col, capsuleCenterDown);
        //         result of new chracter collision after collision detection
        //        Vector3 distance = capsuleCenterDown - contactPoint;
        //        transform.position += Vector3.ClampMagnitude(distance, Mathf.Clamp(radius - distance.magnitude, 0, radius));
        //    }
        //    else if (col is SphereCollider)
        //    {
        //        contactPoint = CollisionDetection.ClosestPointOn((SphereCollider)col, capsuleCenterDown);
        //         result of new chracter collision after collision detection
        //        Vector3 distance = capsuleCenterDown - contactPoint;
        //        transform.position += Vector3.ClampMagnitude(distance, Mathf.Clamp(radius - distance.magnitude, 0, radius));


        //    }

        //     debug purposes
        //    if (contactPoint != Vector3.zero)
        //    {
        //        DebugDraw.DrawMarker(contactPoint, 2.0f, Color.red, 0.0f, false);
        //        closestPointContact = true;
        //    }

        //}
    }

    private bool IsGrounded()
    {
        return Physics.CheckCapsule(capsuleCenterDown,capsuleCenterUp, radius, goundLayers);
    }

    private void OnDisable()
    {
        this.deviceInput.Disable();
    }

    private void OnMoveWASD(InputAction.CallbackContext context)
    {
        deviceInputMove = context.ReadValue<Vector2>();
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


    void OnDrawGizmos()
    {
        if (Application.isPlaying)
        {
            //Gizmos.color = closestPointContact ? Color.red : Color.yellow;
            //Gizmos.DrawWireSphere(transform.position, radius);

            Gizmos.color = closestPointContact ? Color.red : Color.yellow;
            //Gizmos.DrawWireSphere(capsuleCenterUp, radius);
            Gizmos.DrawWireSphere(capsuleCenterDown, radius);

            Gizmos.color = closestPointContact ? Color.red : Color.yellow;
            //Gizmos.DrawWireSphere(capsuleCenterUp, radius);
            Gizmos.DrawWireSphere(capsuleCenterUp, radius);
        }
    }
}
