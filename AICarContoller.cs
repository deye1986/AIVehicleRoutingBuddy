using UnityEngine;

public class AICarController : MonoBehaviour
{
    [Header("Waypoint Settings")]
    public Transform[] waypoints;
    private int currentWaypointIndex = 0;
    public float waypointReachDistance = 5f;

    [Header("AI Driving Settings")]
    public float targetSpeed = 15f;
    public float steeringPower = 1.5f;
    public float brakingDistance = 10f;
    public float throttleStrength = 1f;
    
    [Header("Corner Detection")]
    public float cornerLookahead = 20f;
    public float sharpCornerAngle = 45f;
    public float moderateCornerAngle = 25f;

    [Header("Stuck Recovery")]
    public float stuckSpeedThreshold = 1f;
    public float stuckTimeThreshold = 3f;
    public float reverseTime = 2.5f;

    [Header("Race Start")]
    public float startDelay = 5f;
    private float raceTimer = 0f;
    private bool raceStarted = false;

    [Header("Debug")]
    public bool showDebugLines = true;

    private Rigidbody rb;
    private CarControl carControl;
    private float stuckTimer = 0f;
    private bool isReversing = false;
    private float reverseTimer = 0f;

    // debug code, dont alter unless u want to remove the debug UI
    public bool IsOvertaking { get; private set; } = false;
    public bool IsRecovering => isReversing;
    public float CurrentSpeed => rb != null ? rb.linearVelocity.magnitude : 0f;
    public float CurrentThrottleInput => carControl != null ? carControl.aiVerticalInput : 0f;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        carControl = GetComponent<CarControl>();

        if (carControl == null)
        {
            Debug.LogError("AICarController: CarControl component not found!");
            return;
        }

        carControl.isAIControlled = true;

        if (waypoints == null || waypoints.Length == 0)
        {
            Debug.LogError("AICarController: No waypoints assigned!");
        }
    }

    void FixedUpdate()
    {
        if (waypoints == null || waypoints.Length == 0 || carControl == null) return;

        if (!raceStarted)
        {
            raceTimer += Time.fixedDeltaTime;
            
            if (raceTimer >= startDelay)
            {
                raceStarted = true;
                Debug.Log($"{gameObject.name} - Race Started!");
            }
            else
            {
                carControl.aiHorizontalInput = 0f;
                carControl.aiVerticalInput = 0f;
                return;
            }
        }

        CheckIfStuck();

        if (isReversing)
        {
            HandleReverse();
            return;
        }

        float distanceToWaypoint = Vector3.Distance(transform.position, waypoints[currentWaypointIndex].position);

        if (distanceToWaypoint < waypointReachDistance)
        {
            currentWaypointIndex++;
            if (currentWaypointIndex >= waypoints.Length)
                currentWaypointIndex = 0;
        }

        float steerInput = CalculateSteering();
        float throttleInput = CalculateThrottle(distanceToWaypoint);

        carControl.aiHorizontalInput = steerInput;
        carControl.aiVerticalInput = throttleInput;

        if (showDebugLines)
        {
            Debug.DrawLine(transform.position, waypoints[currentWaypointIndex].position, Color.green);
            Debug.DrawRay(transform.position, transform.forward * 5f, Color.blue);
        }
    }

    void CheckIfStuck()
    {
        float currentSpeed = rb.linearVelocity.magnitude;

        if (currentSpeed < stuckSpeedThreshold)
        {
            stuckTimer += Time.fixedDeltaTime;

            if (stuckTimer >= stuckTimeThreshold && !isReversing)
            {
                isReversing = true;
                reverseTimer = 0f;
                stuckTimer = 0f;
            }
        }
        else
        {
            stuckTimer = 0f;
        }
    }

    void HandleReverse()
    {
        reverseTimer += Time.fixedDeltaTime;

        float wiggleSteering = Mathf.Sin(reverseTimer * 3f) * 0.5f;
        carControl.aiHorizontalInput = wiggleSteering;
        carControl.aiVerticalInput = -1f;

        if (reverseTimer >= reverseTime)
        {
            isReversing = false;
            reverseTimer = 0f;
        }
    }

    private float currentSteerInput = 0f;

    float CalculateSteering()
    {
        Vector3 targetDirection = (waypoints[currentWaypointIndex].position - transform.position).normalized;
        targetDirection.y = 0;

        Vector3 carForward = transform.forward;
        carForward.y = 0;
        carForward.Normalize();

        float angleToTarget = Vector3.SignedAngle(carForward, targetDirection, Vector3.up);

        if (Mathf.Abs(angleToTarget) < 5f)
            angleToTarget = 0f;

        float targetSteerInput = Mathf.Clamp(angleToTarget / carControl.steeringRange, -1f, 1f);
        targetSteerInput *= steeringPower;

        currentSteerInput = Mathf.Lerp(currentSteerInput, targetSteerInput, Time.fixedDeltaTime * 5f);

        return Mathf.Clamp(currentSteerInput, -1f, 1f);
    }

    float GetUpcomingCornerAngle()
    {
        int nextWaypointIndex = currentWaypointIndex + 1;
        if (nextWaypointIndex >= waypoints.Length)
            nextWaypointIndex = 0;

        Vector3 directionToCurrent = (waypoints[currentWaypointIndex].position - transform.position).normalized;
        directionToCurrent.y = 0;

        Vector3 directionToNext = (waypoints[nextWaypointIndex].position - waypoints[currentWaypointIndex].position).normalized;
        directionToNext.y = 0;

        float cornerAngle = Vector3.Angle(directionToCurrent, directionToNext);

        return cornerAngle;
    }

    float CalculateThrottle(float distanceToWaypoint)
    {
        float currentSpeed = rb.linearVelocity.magnitude;
        float cornerAngle = GetUpcomingCornerAngle();

        float cornerProximity = 1f - Mathf.Clamp01(distanceToWaypoint / cornerLookahead);

        float cornerSpeedMultiplier = 1f;
        
        if (cornerAngle > sharpCornerAngle)
        {
            cornerSpeedMultiplier = 0.5f;
        }
        else if (cornerAngle > moderateCornerAngle)
        {
            cornerSpeedMultiplier = 0.7f;
        }
        else
        {
            cornerSpeedMultiplier = 1f;
        }

        float targetSpeedForCorner = targetSpeed * Mathf.Lerp(1f, cornerSpeedMultiplier, cornerProximity);

        if (currentSpeed > targetSpeedForCorner * 1.1f)
        {
            return Mathf.Lerp(0f, -0.2f, (currentSpeed - targetSpeedForCorner) / targetSpeed);
        }
        else if (currentSpeed < targetSpeedForCorner * 0.9f)
        {
            return throttleStrength;
        }
        else
        {
            return 0f; // In the sweet spot
        }
    }
}