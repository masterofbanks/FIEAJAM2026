using System.Runtime.CompilerServices;
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

    [Header("Steering Physics")]
    [SerializeField] private Transform[] _frontWheels;
    [SerializeField] private float _tireGrip = 1;
    [SerializeField] private float _tireMass = 25;
    [SerializeField] private float _steeringRange = 30;
    [SerializeField] private float _steeringRangeAtMaxSpeed = 60;

    [Header("Braking and Reverse Physics")]
    [SerializeField] private float _brakeForce = 5f;
    [SerializeField] private float _minSpeedToReverse = 0.3f;
    //components
    private Rigidbody _rb;

    //input
    private Vector2 directionalInput;
    private InputAction move;
    private InputAction attack;
    public InputSystem_Actions ISAs;

    private Vector3 startPos;
    private void Awake()
    {
        _rb = GetComponent<Rigidbody>();
        ISAs = new InputSystem_Actions();
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        startPos = transform.position;
    }

    private void Update()
    {
        HandleInput();
    }

    private void HandleInput()
    {
        directionalInput = new Vector2(System.MathF.Sign(move.ReadValue<Vector2>().x), System.MathF.Sign(move.ReadValue<Vector2>().y));
    }

    private void FixedUpdate()
    {
        CalculateSuspensionForces();
        CalculateAccelerationForce();

        if(SpeedText != null)
            SpeedText.text = $"{(int)_rb.linearVelocity.magnitude} m/s";

        ChangeSteeringDirection();
        CalculateSteeringForces();
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
                //find the world space direction of the acceleration force
                Vector3 accelDir = wheel.forward;
                //if the move.y > 0 (W key or Up Arrow)
                if(directionalInput.y > 0)
                {
                    //find the current car speed as the dot product between the car body's forward transform and the rigidbody's velocity
                    float carSpeed = Vector3.Dot(_carBody.forward, _rb.linearVelocity);
                    //normalize that car speed with respect to the car's top speed
                    float normalizedSpeed = Mathf.Clamp01(Mathf.Abs(carSpeed) / _topSpeed);

                    if(normalizedSpeed < 1)
                    {
                        //evaulate the amount of torque needed for that speed and input value
                        float availableTorque = PowerCurve.Evaluate(normalizedSpeed) * _topPower;
                        //add torque to wheel using add force at position
                        _rb.AddForceAtPosition(accelDir * availableTorque, wheel.position);
                    }
                    
                }

                //braking or reversing
                else if(directionalInput.y < 0)
                {
                    //find the forward direction of the wheel
                    //get the velocity of the tire
                    Vector3 tireWorldVel = _rb.GetPointVelocity(wheel.position);
                    //find the speed of the tire in the wheel's forward direction
                    float forwardVel = Vector3.Dot(accelDir, tireWorldVel);
                    if(forwardVel > _minSpeedToReverse)
                    {
                        //find backwards velocity change needed to brake
                        float desiredVelChange = -forwardVel * _brakeForce;
                        //acceleration of said velocity change
                        float desiredAccel = desiredVelChange / Time.fixedDeltaTime;
                        //add braking force
                        _rb.AddForceAtPosition(accelDir * _tireMass * desiredAccel, wheel.position);
                    }

                    //reverse
                    else
                    {
                        //find the current car speed as the dot product between the car body's forward transform and the rigidbody's velocity
                        float carSpeed = Vector3.Dot(-1 * _carBody.forward, _rb.linearVelocity);
                        //normalize that car speed with respect to the car's top speed
                        float normalizedSpeed = Mathf.Clamp01(Mathf.Abs(carSpeed) / _topSpeed);

                        if (normalizedSpeed < 1)
                        {
                            //evaulate the amount of torque needed for that speed and input value
                            float availableTorque = PowerCurve.Evaluate(normalizedSpeed) * _topPower;
                            //add torque to wheel using add force at position
                            _rb.AddForceAtPosition(-1 * accelDir * availableTorque, wheel.position);
                        }
                    }
                    
                }

            }

        }
        
    }

    private void ChangeSteeringDirection()
    {
        float carSpeed = Vector3.Dot(_carBody.forward, _rb.linearVelocity);
        //normalize that car speed with respect to the car's top speed
        float normalizedSpeed = Mathf.Clamp01(Mathf.Abs(carSpeed) / _topSpeed);
        float currentSteerRange = Mathf.Lerp(_steeringRange, _steeringRangeAtMaxSpeed, normalizedSpeed);
        float rotationValue = directionalInput.x * currentSteerRange + transform.eulerAngles.y;

        foreach(Transform wheel in _frontWheels)
        {
            wheel.eulerAngles = new Vector3(0, rotationValue, 0);
        }
    }

    private void CalculateSteeringForces()
    {
        for (int i = 0; i < _wheelPositions.Length; i++)
        {
            //test whether each wheel is in contact with the ground via a raycast
            Transform wheel = _wheelPositions[i];
            RaycastHit hit;
            bool groundCheck = Physics.Raycast(wheel.position, -1 * wheel.up, out hit, _wheelRadius + _suspensionRestDistance, GroundLayer);
            //if it does have contact with the ground
            if (groundCheck)
            {
                //calculate the direction you dont want the tire to slide in
                Vector3 steeringDir = wheel.right;
                //get the velocity of the tire
                Vector3 tireWorldVel = _rb.GetPointVelocity(wheel.position);
                //find the speed of the tire in the wheel's steering direction
                float steeringVel = Vector3.Dot(steeringDir, tireWorldVel);
                //find the opposing direction of force against tire slippage
                float desiredVelChange = -steeringVel * _tireGrip;
                //acceleration of said velocity change
                float desiredAccel = desiredVelChange / Time.fixedDeltaTime;
                _rb.AddForceAtPosition(steeringDir * _tireMass * desiredAccel, wheel.position);

            }

        }
    }

    private void ResetCar(InputAction.CallbackContext cxt)
    {
        transform.position = startPos;
        transform.rotation = Quaternion.identity;
        _rb.linearVelocity = Vector3.zero;
        Debug.Log("Reset Car");
    }

    private void OnEnable()
    {
        move = ISAs.Player.Move;
        attack = ISAs.Player.Attack;
        move.Enable();
        attack.Enable();

        attack.performed += ResetCar;
    }

    private void OnDisable()
    {
        move.Disable();
        attack.Disable();
    }
}
