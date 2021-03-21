using UnityEngine;
using UnityEngine.InputSystem;

public class MovingSphere : MonoBehaviour
{
    private DeviceInput deviceInput;

    private Vector3 velocity;
    private Vector2 gamepadInputStick;
    private Vector3 projectOnPlane;
    private Quaternion rotationToCamForward;


    [SerializeField, Range(0f, 100f)] public float maxSpeed = 10f;
    [SerializeField, Range(0f, 300f)] public float maxAcceleration = 10f;


    private void Awake()
    {
        this.deviceInput = new DeviceInput();
    }

    private void OnEnable()
    {
        this.deviceInput.Enable();
        this.deviceInput.Player.Move.performed += this.OnMove;
        this.deviceInput.Player.Move.canceled += this.OnMove;



    }

    private void OnMove(InputAction.CallbackContext context)
    {
        Debug.Log("OnMove");

        this.gamepadInputStick = context.ReadValue<Vector2>();
        this.gamepadInputStick = Vector2.ClampMagnitude(this.gamepadInputStick, 1f);



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
        this.Move();
    }

    public void Move()
    {
        // displacement so movement will not be teleportation anymore e.g Controlling Velocity
        // see <see href="https://catlikecoding.com/unity/tutorials/movement/sliding-a-sphere/">HERE</see>
        Vector3 inputVelocity = new Vector3(this.gamepadInputStick.x, 0f, this.gamepadInputStick.y) * this.maxSpeed;

        float maxSpeedChange = this.maxAcceleration * Time.deltaTime;
        this.velocity.x = Mathf.MoveTowards(this.velocity.x, inputVelocity.x, maxSpeedChange);
        this.velocity.z = Mathf.MoveTowards(this.velocity.z, inputVelocity.z, maxSpeedChange);
        Vector3 movementSteps = this.velocity * Time.deltaTime;

        // camera indepandent movement
        this.projectOnPlane = Vector3.ProjectOnPlane(Camera.main.transform.forward, Vector3.up);
        this.rotationToCamForward = Quaternion.LookRotation(this.projectOnPlane);
        movementSteps = rotationToCamForward * movementSteps;


        this.transform.localPosition += movementSteps;
    }

    private void OnDisable()
    {

        this.deviceInput.Disable();
    }
}
