using UnityEngine;

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
    public float offset;

    //components
    private Rigidbody _rb;

    private void Awake()
    {
        _rb = GetComponent<Rigidbody>();
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    

    private void FixedUpdate()
    {
        CalculateSuspensionForces();

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
                offset = _suspensionRestDistance - hit.distance;
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
}
