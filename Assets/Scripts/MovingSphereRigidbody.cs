using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections.Generic;

/// <summary>
/// /https://www.gamasutra.com/view/feature/131790/simple_intersection_tests_for_games.php?print=1
/// </summary>

public class MovingSphereRigidbody : MonoBehaviour
{
    private DeviceInput deviceInput;

    private Vector2 gamepadInputStick;
    private Vector3 velocity;
    private Vector3 projectOnPlane;
    private Quaternion rotationToCamForward;

    [SerializeField] public float gravity = -9.81f;


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
    public Collider col;
    public LayerMask goundLayers;

    // for collisions
    // bool for closetPoint algorithmus
    private bool closestPointContact = false;
    [SerializeField] float radius = 0.5f;

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

        this.deviceInput.Player.Jump.performed += this.OnJump;
        this.deviceInput.Player.Jump.canceled += this.OnJump;

    }
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        CollisionDetection();
    }

    private void FixedUpdate()
    {
        this.Jump();
        this.Move();
    }

    private void OnMove(InputAction.CallbackContext context)
    {
        //Debug.Log("OnMove");
        this.gamepadInputStick = context.ReadValue<Vector2>();


    }

    private void OnJump(InputAction.CallbackContext context) 
    {
        Debug.Log("OnJump");
        this.requireJump = context.ReadValueAsButton();
    }
    public void Move()
    {
        this.gamepadInputStick = Vector2.ClampMagnitude(this.gamepadInputStick, 1f) * this.maxSpeed;

        // displacement so movement will not be teleportation anymore e.g Controlling Velocity
        // see <see href="https://catlikecoding.com/unity/tutorials/movement/sliding-a-sphere/">HERE</see>

        this.acceleration = this.maxAcceleration * Time.deltaTime;

        this.velocity.x = Mathf.MoveTowards(this.velocity.x, gamepadInputStick.x, acceleration);
        this.velocity.z = Mathf.MoveTowards(this.velocity.z, gamepadInputStick.y, acceleration);

        // camera indepandent movement
        this.projectOnPlane = Vector3.ProjectOnPlane(Camera.main.transform.forward, Vector3.up);
        this.rotationToCamForward = Quaternion.LookRotation(this.projectOnPlane);


        Vector3 movementSteps = this.velocity * Time.deltaTime;
        movementSteps = this.rotationToCamForward * movementSteps;
        rBody.MovePosition(transform.position + movementSteps);
    }

    public void Jump()
    {
        if (requireJump && isGrounded())
        {
            // jumping is all about overcome gravity
            //velocity.y += Mathf.Sqrt(-2f * Physics.gravity.y * jumpHeight);
            //rBody.velocity = velocity;

            //rBody.velocity = new Vector3(rBody.velocity.x, 0, rBody.velocity.z);
            //rBody.AddForce(Vector3.up * jumpSpeed, ForceMode.Impulse);
        }


    }

    #region collision detection
    private bool isGrounded()
    {
        return Physics.CheckCapsule(col.bounds.max, col.bounds.min, col.bounds.extents.x, goundLayers);
    }

    /// <summary>
    /// performs all needed collsion detections
    /// </summary>
    private void CollisionDetection() 
    {

        closestPointContact = false;

        foreach (Collider col in Physics.OverlapSphere(transform.position, radius))
        {
            Vector3 contactPoint = Vector3.zero;

            // for different colliders
            if (col is BoxCollider)
            {
                contactPoint = ClosestPointOn((BoxCollider)col, transform.position);
            }
            else if (col is SphereCollider)
            {
                contactPoint = ClosestPointOn((SphereCollider)col, transform.position);
            }

            // debug purposes
            DebugDraw.DrawMarker(contactPoint, 2.0f, Color.red, 0.0f, false);

            // result 
            Vector3 distance = transform.position - contactPoint;

            transform.position += Vector3.ClampMagnitude(distance, Mathf.Clamp(radius - distance.magnitude, 0, radius));

            closestPointContact = true;

        }
    }

    /// <summary>
    /// performs an closestPointAlgorithmn with different obstacle rotation
    /// </summary>
    /// <param name="collider"></param>
    /// <param name="to"></param>
    /// <returns></returns>
    Vector3 ClosestPointOn(BoxCollider collider, Vector3 to)
    {
        if (collider.transform.rotation == Quaternion.identity)
        {
            return collider.ClosestPointOnBounds(to);
        }

        return closestPointOnOBB(collider, to);
    }

    /// <summary>
    /// closest point algorithm for spherecollider
    /// </summary>
    /// <param name="collider"></param>
    /// <param name="to"></param>
    /// <returns></returns>
    private Vector3 ClosestPointOn(SphereCollider collider, Vector3 to) 
    {
        Vector3 p;

        p = to - collider.transform.position;
        p.Normalize();

        p *= collider.radius * collider.transform.localScale.x;
        p += collider.transform.position;

        return p;
    }

    /// <summary>
    /// Oriented Bounding Box interscetion Test
    /// for geometric rotated obstacles 
    /// </summary>
    /// <param name="collider"></param>
    /// <param name="to"></param>
    /// <returns></returns>
    Vector3 closestPointOnOBB(BoxCollider collider, Vector3 to)
    {
        // Cache the collider transform
        Transform colTransform = collider.transform;

        // Firstly, transform the point into the space of the collider
        var local = colTransform.InverseTransformPoint(to);

        // Now, shift it to be in the center of the box
        local -= collider.center;

        // Inverse scale it by the colliders scale
        var localNorm = new Vector3(
            Mathf.Clamp(local.x, -collider.size.x * 0.5f, collider.size.x * 0.5f),
            Mathf.Clamp(local.y, -collider.size.y * 0.5f, collider.size.y * 0.5f),
            Mathf.Clamp(local.z, -collider.size.z * 0.5f, collider.size.z * 0.5f)
        );

        // Now we undo our transformations
        localNorm += collider.center;

        // Return resulting point
        return colTransform.TransformPoint(localNorm);
    }

    #endregion

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
