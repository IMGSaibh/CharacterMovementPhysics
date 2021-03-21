using UnityEngine;
using UnityEngine.InputSystem;

public class MovingSphereRigidbody : MonoBehaviour
{
    private DeviceInput deviceInput;
    private bool triggerJump = false;


    private Vector2 gamepadInputStick;
    private Vector3 velocity;
    private Vector3 inputVelocity;
    private Vector3 projectOnPlane;
    private Quaternion rotationToCamForward;

    [SerializeField, Range(0f, 100f)] public float maxSpeed = 10f;
    [SerializeField, Range(0f, 300f)] public float maxAcceleration = 10f;
    [SerializeField, Range(0.0f, 2f)] public float jumpHeight = 2f;
    [SerializeField, Range(0f, 5f)] public float maxAirJumps = 2f;

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

    private void OnMove(InputAction.CallbackContext context)
    {
        Debug.Log("OnMove");

        this.gamepadInputStick = context.ReadValue<Vector2>();
        this.gamepadInputStick = Vector2.ClampMagnitude(this.gamepadInputStick, 1f);



    }

    private void OnJump(InputAction.CallbackContext context) 
    {
        Debug.Log("OnJump");
        this.triggerJump = context.ReadValueAsButton();
    }

    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    private void FixedUpdate()
    {
        this.Jump();
        this.Move();
    }

    public void Move()
    {
        // get velocity first of rigidbody
        velocity = rBody.velocity;

        // displacement so movement will not be teleportation anymore e.g Controlling Velocity
        // see <see href="https://catlikecoding.com/unity/tutorials/movement/sliding-a-sphere/">HERE</see>
        inputVelocity = new Vector3(this.gamepadInputStick.x, 0f, this.gamepadInputStick.y) * this.maxSpeed;

        float maxSpeedChange = this.maxAcceleration * Time.deltaTime;
        this.velocity.x = Mathf.MoveTowards(this.velocity.x, inputVelocity.x, maxSpeedChange);
        this.velocity.z = Mathf.MoveTowards(this.velocity.z, inputVelocity.z, maxSpeedChange);
        //Vector3 movementSteps = this.velocity * Time.deltaTime;

        // camera indepandent movement
        this.projectOnPlane = Vector3.ProjectOnPlane(Camera.main.transform.forward, Vector3.up);
        this.rotationToCamForward = Quaternion.LookRotation(this.projectOnPlane);


        //movementSteps = rotationToCamForward * movementSteps;
        //this.transform.localPosition += movementSteps;

        rBody.velocity = velocity;
    }

    public void Jump()
    {
        if (triggerJump && isGrounded())
        {
            // jumping is all about overcome gravity
            velocity.y += Mathf.Sqrt(-2f * Physics.gravity.y * jumpHeight);
            rBody.velocity = velocity;


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
