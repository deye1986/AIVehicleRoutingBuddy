using UnityEngine;

public class CarControl : MonoBehaviour
{
    [Header("Control Mode")]
    public bool isAIControlled = false;
    [HideInInspector] public float aiVerticalInput = 0f;
    [HideInInspector] public float aiHorizontalInput = 0f;

    [Header("Car Properties")]
    public float motorTorque = 2000f;
    public float brakeTorque = 2000f;
    public float maxSpeed = 20f;
    public float steeringRange = 30f;
    public float steeringRangeAtMaxSpeed = 10f;
    public float centreOfGravityOffset = -1f;

    [Header("Throttle Settings")]
    public float throttleDeadzone = 0.05f; 
    
    [Header("Steering Settings")]
    public float steeringDeadzone = 0.05f; 

    [Header("Third Person Camera Settings")]
    public float cameraDistance = 6f;
    public float cameraHeight = 2f;
    public float cameraFollowSpeed = 5f;
    public float cameraRotationSpeed = 3f;
    public float mouseSensitivity = 100f;
    public float lookAtHeightOffset = 1f;

    private WheelControl[] wheels;
    private Rigidbody rigidBody;
    private float cameraYaw = 0f;

    public ParticleSystem breakSmokeLeft;
    public ParticleSystem breakSmokeRight;

    public Camera firstPersonCameraView;
    public Camera thirdPersonCameraView;
    public Camera wheelcam;
    public Camera topDownCameraView;
    public Camera rearViewMirror;

    public Camera orange; //cars names, seperate on board camera for each opponent driver handled by the computer
    public Camera apexgrip;
    public Camera gForce;

    public Camera gridflyoverCam;

    public Camera finishLineCam;
    

    void Start()
    {
        rigidBody = GetComponent<Rigidbody>();

        Vector3 centerOfMass = rigidBody.centerOfMass;
        centerOfMass.y += centreOfGravityOffset;
        rigidBody.centerOfMass = centerOfMass;

        wheels = GetComponentsInChildren<WheelControl>();

        if (!isAIControlled)
        {
            firstPersonCameraView.enabled = true; 
            thirdPersonCameraView.enabled = false;
            topDownCameraView.enabled = false;
            wheelcam.enabled = false;
            rearViewMirror.enabled = false;
        }
    }

    void FixedUpdate()
    {
        float vInput = isAIControlled ? aiVerticalInput : Input.GetAxis("Vertical");
        float hInput = isAIControlled ? aiHorizontalInput : Input.GetAxis("Horizontal");

        float forwardSpeed = Vector3.Dot(transform.forward, rigidBody.linearVelocity);
        float speedFactor = Mathf.InverseLerp(0, maxSpeed, Mathf.Abs(forwardSpeed));

        float currentMotorTorque = Mathf.Lerp(motorTorque, 0, speedFactor);
        float currentSteerRange = Mathf.Lerp(steeringRange, steeringRangeAtMaxSpeed, speedFactor);

        foreach (var wheel in wheels)
        {
            if (wheel.steerable)
            {
                float adjustedHInput = Mathf.Abs(hInput) > steeringDeadzone ? hInput : 0f;
                wheel.WheelCollider.steerAngle = adjustedHInput * currentSteerRange;
            }

            if (Mathf.Abs(vInput) > throttleDeadzone)
            {
                bool isAccelerating = Mathf.Sign(vInput) == Mathf.Sign(forwardSpeed);

                if (isAccelerating)
                {
                    // Apply throttle
                    if (wheel.motorized)
                    {
                        wheel.WheelCollider.motorTorque = vInput * currentMotorTorque;
                    }
                    wheel.WheelCollider.brakeTorque = 0f;
                }
                else
                {
                    wheel.WheelCollider.motorTorque = 0f;
                    wheel.WheelCollider.brakeTorque = Mathf.Abs(vInput) * brakeTorque;
                }
            }
            else
            {
                wheel.WheelCollider.motorTorque = 0f;
                wheel.WheelCollider.brakeTorque = 0f;
            }
        }

        if (!isAIControlled)
        {
            HandlePlayerControls();
        }
    }

    void LateUpdate()
    {
        if (!isAIControlled && thirdPersonCameraView != null && thirdPersonCameraView.enabled)
        {
            HandleThirdPersonCamera();
        }
    }

    void HandleThirdPersonCamera()
    {
        float mouseX = Input.GetAxis("Mouse X");
        cameraYaw += mouseX * mouseSensitivity * Time.deltaTime;

        Quaternion rotation = Quaternion.Euler(0, transform.eulerAngles.y + cameraYaw, 0);
        Vector3 offset = rotation * new Vector3(0, cameraHeight, -cameraDistance);
        Vector3 desiredPosition = transform.position + offset;

        thirdPersonCameraView.transform.position = Vector3.Lerp(
            thirdPersonCameraView.transform.position,
            desiredPosition,
            Time.deltaTime * cameraFollowSpeed
        );

        Vector3 lookAtPoint = transform.position + Vector3.up * lookAtHeightOffset;
        Quaternion targetRotation = Quaternion.LookRotation(lookAtPoint - thirdPersonCameraView.transform.position);
        
        thirdPersonCameraView.transform.rotation = Quaternion.Slerp(
            thirdPersonCameraView.transform.rotation,
            targetRotation,
            Time.deltaTime * cameraRotationSpeed
        );
    }

    void HandlePlayerControls()
    {
        if (Input.GetKey(KeyCode.Space))
        {
            HitTheBrakes();
        }
        else
        {
            rigidBody.linearDamping = 0.05f;
            if (breakSmokeLeft != null && breakSmokeLeft.isEmitting)
            {
                breakSmokeLeft.Stop();
            }
            if (breakSmokeRight != null && breakSmokeRight.isEmitting)
            {
                breakSmokeRight.Stop();
            }
        }

        if (Input.GetKeyDown(KeyCode.Alpha0)) // i wish i would have used functions for this and the below but its too late now!!!!
        {
            Debug.Log("Change to top down view");
            firstPersonCameraView.enabled = false;
            thirdPersonCameraView.enabled = false;
            topDownCameraView.enabled = true;
            wheelcam.enabled = false;
            rearViewMirror.enabled = false;
        }
        if (Input.GetKeyDown(KeyCode.Alpha9))
        {
            Debug.Log("Change to first person front bumper view");
            firstPersonCameraView.enabled = true;
            thirdPersonCameraView.enabled = false;
            topDownCameraView.enabled = false;
            wheelcam.enabled = false;
            rearViewMirror.enabled = false;
        }
        if (Input.GetKeyDown(KeyCode.Alpha8))
        {
            Debug.Log("Change to 3rd person view");
            firstPersonCameraView.enabled = false;
            thirdPersonCameraView.enabled = true;
            topDownCameraView.enabled = false;
            wheelcam.enabled = false;
            rearViewMirror.enabled = false;
            cameraYaw = 0f; // Reset camera when switching to 3rdPerso
        }
        if (Input.GetKeyDown(KeyCode.Alpha7))
        {
            Debug.Log("Change to car wheel cam view");
            firstPersonCameraView.enabled = false;
            thirdPersonCameraView.enabled = false;
            topDownCameraView.enabled = false;
            wheelcam.enabled = true;
            rearViewMirror.enabled = false;
        }
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            Debug.Log("On board with Orange");
            firstPersonCameraView.enabled = false;
            thirdPersonCameraView.enabled = false;
            topDownCameraView.enabled = false;
            wheelcam.enabled = false;
            rearViewMirror.enabled = false;
            apexgrip.enabled = false;
            gForce.enabled = false;
            finishLineCam.enabled = false;
            orange.enabled = true;
            
        }
        if (Input.GetKeyDown(KeyCode.C))
        {
            Debug.Log("Change to REAR VIEW MIRROR view");
            firstPersonCameraView.enabled = false;
            thirdPersonCameraView.enabled = false;
            topDownCameraView.enabled = false;
            wheelcam.enabled = false;
            finishLineCam.enabled = false;
            rearViewMirror.enabled = true;
        }
        if (Input.GetKeyUp(KeyCode.C))
        {
            Debug.Log("Change to back to the last view");
            firstPersonCameraView.enabled = true;
            thirdPersonCameraView.enabled = false;
            topDownCameraView.enabled = false;
            wheelcam.enabled = false;
            rearViewMirror.enabled = false;
        }
        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            Debug.Log("On board with apexgrip ");
            firstPersonCameraView.enabled = false;
            thirdPersonCameraView.enabled = false;
            topDownCameraView.enabled = false;
            wheelcam.enabled = false;
            rearViewMirror.enabled = false;
            gForce.enabled = false;
            orange.enabled = false;
            finishLineCam.enabled = false;
            gridflyoverCam.enabled = false;
            apexgrip.enabled = true;
        }
        if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            Debug.Log("On board with gForce");
            firstPersonCameraView.enabled = false;
            thirdPersonCameraView.enabled = false;
            topDownCameraView.enabled = false;
            wheelcam.enabled = false;
            rearViewMirror.enabled = false;
            apexgrip.enabled = false;
            orange.enabled = false;
            finishLineCam.enabled = false;
            gridflyoverCam.enabled = false;
            gForce.enabled = true;
        }
        if (Input.GetKeyDown(KeyCode.Alpha4))
        {
            Debug.Log("Finish Line Camera");
            firstPersonCameraView.enabled = false;
            thirdPersonCameraView.enabled = false;
            topDownCameraView.enabled = false;
            wheelcam.enabled = false;
            rearViewMirror.enabled = false;
            apexgrip.enabled = false;
            orange.enabled = false;
            gForce.enabled = false;
            gridflyoverCam.enabled = false;
            finishLineCam.enabled = true;
        }

        if (Input.GetKeyDown(KeyCode.Alpha5))
        {
            Debug.Log("grid fly over");
            firstPersonCameraView.enabled = false;
            thirdPersonCameraView.enabled = false;
            topDownCameraView.enabled = false;
            wheelcam.enabled = false;
            rearViewMirror.enabled = false;
            apexgrip.enabled = false;
            orange.enabled = false;
            gForce.enabled = false;
            finishLineCam.enabled = false;
            gridflyoverCam.enabled = true;
        }
    }

    void HitTheBrakes()
    {
        Debug.Log("Brake smoke triggered");
        float sharpBrakeForce = brakeTorque * 7.5f;
        rigidBody.linearDamping = 0.8f;

        foreach (var wheel in wheels)
        {
            wheel.WheelCollider.motorTorque = 0f;
            wheel.WheelCollider.brakeTorque = sharpBrakeForce;
        }

        if (breakSmokeLeft != null && !breakSmokeLeft.isEmitting)
        {
            breakSmokeLeft.Play();
        }
        if (breakSmokeRight != null && breakSmokeRight.isEmitting)
        {
            breakSmokeRight.Play();
        }
    }
}