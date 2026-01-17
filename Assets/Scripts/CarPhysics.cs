using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public class CarPhysics : MonoBehaviour
{
    [Header("Wheel Physics")]
    [SerializeField] private Transform[] _wheelPositions;
    [SerializeField] private float _wheelRadius = 4f;
    [SerializeField] private LayerMask GroundLayer;

    [Header("Suspension Physics")]
    [SerializeField] private float _springStrength= 35f;
    [SerializeField] private float _springDamper = 5f;
    [SerializeField] private float _suspensionRestDistance = 3f;

    [Header("Acceleration Physics")]
    public AnimationCurve PowerCurve;
    [SerializeField] private Transform _carBody;
    [SerializeField] private float _topSpeed = 30f;
    [SerializeField] private float _topPower = 300f;
    [SerializeField] private TextMeshProUGUI SpeedText;
    public float currentForce;
    //components
    private Rigidbody _rb;

    //input
    private Vector2 directionalInput;
    private InputAction move;
    public InputSystem_Actions ISAs;
    private void Awake()
    {
        _rb = GetComponent<Rigidbody>();
        ISAs = new InputSystem_Actions();
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

    }

    private void Update()
    {
        directionalInput = move.ReadValue<Vector2>();
    }

    private void FixedUpdate()
    {
        CalculateSuspensionForces();
        CalculateAccelerationForce();
        SpeedText.text = $"{(int)_rb.linearVelocity.magnitude} MPH";
    }

    private void CalculateSuspensionForces()
    {
        //loop through each wheel position to calculate the suspension force at each wheel
        for(int i = 0; i < _wheelPositions.Length; i++)
        {
            Transform wheel = _wheelPositions[i];
            RaycastHit hit;
            //cast a ray downwards for a certain distance (how far off the ground I want the car to float)
            bool groundCheckRay = Physics.Raycast(wheel.position, -1 * wheel.up, out hit, _wheelRadius + _suspensionRestDistance, GroundLayer);
            //if the ray hits ->
            if (groundCheckRay)
            {
                //find the world space direction of the spring force
                Vector3 springDir = wheel.up;
                //find the world space velocity of the tire
                Vector3 tireWorldVel = _rb.GetPointVelocity(wheel.position);
                //calculate delta x for Hooke's Law for Spring Force
                float deltaX = _suspensionRestDistance - (hit.distance - _wheelRadius);
                //calculate the speed of the spring in the direction of the spring movement direction
                float vel = Vector3.Dot(springDir, tireWorldVel);
                //calculate spring force -> F= kx - vs
                float force = (_springStrength * deltaX) - vel * _springDamper;
                //add force to the car's rigidbody at the position of the wheel
                _rb.AddForceAtPosition(springDir * force, wheel.position);
            }

        }


    }

    private void CalculateAccelerationForce()
    {
        for(int i = 0; i < _wheelPositions.Length; i++)
        {
            //test whether each wheel is in contact with the ground via a raycast
            Transform wheel = _wheelPositions[i];
            RaycastHit hit;
            bool groundCheck = Physics.Raycast(wheel.position, -1 * wheel.up, out hit, _wheelRadius + _suspensionRestDistance, GroundLayer);
            //if it does have contact with the ground
            if (groundCheck)
            {
                //find the world space direction of the acceleration/braking force
                Vector3 accelDir = wheel.forward;
                //if the move.y > 0 (W key or Up Arrow)
                if(directionalInput.y > 0)
                {
                    //Debug.Log("directional input up");
                    //find the current car speed as the dot product between the car body's forward transform and the rigidbody's velocity
                    float carSpeed = Vector3.Dot(_carBody.forward, _rb.linearVelocity);
                    //normalize that car speed with respect to the car's top speed
                    float normalizedSpeed = Mathf.Clamp01(Mathf.Abs(carSpeed) / _topSpeed);
                    //evaulate the amount of torque needed for that speed and input value
                    float availableTorque = PowerCurve.Evaluate(normalizedSpeed) * _topPower;
                    currentForce = availableTorque;
                    //add torque to wheel using add force at position
                    _rb.AddForceAtPosition(accelDir * availableTorque, wheel.position);
                }

            }

        }
        
    }

    private void OnEnable()
    {
        move = ISAs.Player.Move;
        move.Enable();
    }

    private void OnDisable()
    {
        move.Disable();
    }
}
