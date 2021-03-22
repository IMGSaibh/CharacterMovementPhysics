using UnityEngine;
using UnityEngine.InputSystem;

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
        //if (rBody.velocity.y < 0)
        //{
        //    //rBody.AddForce(new Vector3(1, Physics.gravity.y * fallMultiplier, 1), ForceMode.Acceleration);
        //    velocity.y += Physics.gravity.y * gravityScale * fallMultiplier;
        //    rBody.velocity = velocity;
        //}
        //if (rBody.velocity.y > 5)
        //{
        //    rBody.velocity = new Vector3(0, -1, 0);
        //}
        //if (rBody.velocity.y < 0)
        //{
        //    rBody.AddForce(new Vector3(1, Physics.gravity.y * lowJumpMultiplier, 1), ForceMode.Acceleration);

        //}



    }

    private void FixedUpdate()
    {
        this.Jump();
        this.Move();
    }

    private void OnMove(InputAction.CallbackContext context)
    {
        Debug.Log("OnMove");
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

    private bool isGrounded()
    {
        return Physics.CheckCapsule(col.bounds.max, col.bounds.min, col.bounds.extents.x, goundLayers);
    }

    private void OnDisable()
    {
        this.deviceInput.Disable();
    }
}
