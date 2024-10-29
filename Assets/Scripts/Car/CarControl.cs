using UnityEngine;

public class CarControl : MonoBehaviour
{
    [SerializeField] float motorTorque = 2000;
    [SerializeField] float brakeTorque = 2000;
    [SerializeField] float maxSpeed = 20;
    [SerializeField] float steeringRange = 30;
    [SerializeField] float steeringRangeAtMaxSpeed = 10;
    [SerializeField] float centreOfGravityOffset = -1f;

    WheelControl[] wheels;
    Rigidbody rigidBody;
    float upsideDownTime;
    float vInput, hInput, forwardSpeed, currentMotorTorque, currentSteerRange;

    public float GetRPM()
    {
        return wheels[0].GetRPM();
    }
    public float GetSpeed()
    {
        // Calculate the speed in meters per second
        float speed = rigidBody.velocity.magnitude;

        // Convert the speed to kilometers per hour (or miles per hour if you prefer)
        float speedKPH = speed * 3.6f;  // For KPH

        return speedKPH;
    }
    void Start()
    {
        rigidBody = GetComponent<Rigidbody>();
        rigidBody.centerOfMass += Vector3.up * centreOfGravityOffset;
        wheels = GetComponentsInChildren<WheelControl>();
    }

    void Update()
    {
        HandleInput(out vInput, out hInput, out forwardSpeed, out currentMotorTorque, out currentSteerRange);

        //Reposition if upside down
        if (Vector3.Dot(transform.up, Vector3.down) > 0)
        {
            upsideDownTime += Time.deltaTime;
            if (upsideDownTime >= 2f)
            {
                // transform.position = new Vector3(0, 5, -130);
                transform.rotation = Quaternion.identity;
            }
        }
        else
        {
            upsideDownTime = 0.0f; // Reset the timer if the car is not upside down
        }
    }
    void FixedUpdate()
    {
        Move(vInput, hInput, forwardSpeed, currentMotorTorque, currentSteerRange);
    }
    void HandleInput(out float vInput, out float hInput, out float forwardSpeed, out float currentMotorTorque, out float currentSteerRange)
    {
#if UNITY_EDITOR
        vInput = Input.GetAxis("Vertical");
        hInput = Input.GetAxis("Horizontal");
#elif UNITY_ANDROID
        vInput = SimpleInput.GetAxis("Vertical");
        hInput = SimpleInput.GetAxis("Horizontal");
#endif
        // Calculate current speed in relation to the forward direction of the car
        // (this returns a negative number when traveling backwards)
        forwardSpeed = Vector3.Dot(transform.forward, rigidBody.velocity);


        // Calculate how close the car is to top speed
        // as a number from zero to one
        float speedFactor = Mathf.InverseLerp(0, maxSpeed, forwardSpeed);

        // Use that to calculate how much torque is available 
        // (zero torque at top speed)
        currentMotorTorque = Mathf.Lerp(motorTorque, 0, speedFactor);

        // …and to calculate how much to steer 
        // (the car steers more gently at top speed)
        currentSteerRange = Mathf.Lerp(steeringRange, steeringRangeAtMaxSpeed, speedFactor);
    }

    void Move(float vInput, float hInput, float forwardSpeed, float currentMotorTorque, float currentSteerRange)
    {
        // Check whether the user input is in the same direction 
        // as the car's velocity
        bool isAccelerating = Mathf.Sign(vInput) == Mathf.Sign(forwardSpeed);

        foreach (var wheel in wheels)
        {
            // Apply steering to Wheel colliders that have "Steerable" enabled
            if (wheel.steerable)
            {
                wheel.WheelCollider.steerAngle = hInput * currentSteerRange;
            }

            if (isAccelerating)
            {
                // Apply torque to Wheel colliders that have "Motorized" enabled
                if (wheel.motorized)
                {
                    wheel.WheelCollider.motorTorque = vInput * currentMotorTorque;
                }
                wheel.WheelCollider.brakeTorque = 0;
            }
            else
            {
                // If the user is trying to go in the opposite direction
                // apply brakes to all wheels
                wheel.WheelCollider.brakeTorque = Mathf.Abs(vInput) * brakeTorque;
                wheel.WheelCollider.motorTorque = 0;
            }
        }
    }
}