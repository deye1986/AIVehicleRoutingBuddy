using UnityEngine;
using UnityEngine.AI;

public class EnemyAIRoute : MonoBehaviour
{
    private NavMeshAgent navMeshAgent;
    public Transform[] goals;
    private int currentGoalIndex = 0;
    public float waypointReachThreshold = 2f;
    
    public float groundCheckDistance = 10f;
    public float steeringSpeed = 3f;
    public LayerMask groundLayer;
    public bool showDebugRays = true;

    void Start()
    {
        navMeshAgent = GetComponent<NavMeshAgent>();
        navMeshAgent.updateRotation = false;
        
        if (goals.Length > 0)
        {
            navMeshAgent.destination = goals[currentGoalIndex].position;
        }
        
        Debug.Log("Ground Layer Mask value: " + groundLayer.value);
    }

    void Update()
    {
        if (navMeshAgent.remainingDistance <= waypointReachThreshold && !navMeshAgent.pathPending)
        {
            MoveToNextGoal();
        }
        
        HandleCarRotation();
    }

    void HandleCarRotation()
    {
        Vector3 targetDirection = navMeshAgent.velocity.normalized;
        
        if (targetDirection.sqrMagnitude < 0.01f)
            return;
        
        RaycastHit hit;
        Vector3 rayStart = transform.position + Vector3.up * 2f;
        
        Debug.DrawRay(rayStart, Vector3.down * groundCheckDistance, Color.red, 0.1f);
        
        bool hitWithMask = Physics.Raycast(rayStart, Vector3.down, out hit, groundCheckDistance, groundLayer);
        
        RaycastHit hit2;
        bool hitWithoutMask = Physics.Raycast(rayStart, Vector3.down, out hit2, groundCheckDistance);
        
        Debug.Log($"Raycast WITH mask: {hitWithMask} | WITHOUT mask: {hitWithoutMask}");
        
        if (hitWithoutMask)
        {
            Debug.Log($"Hit object: {hit2.collider.gameObject.name} on layer: {LayerMask.LayerToName(hit2.collider.gameObject.layer)}");
            Debug.DrawLine(rayStart, hit2.point, Color.yellow, 0.1f);
        }
        
        if (hitWithMask)
        {
            Debug.DrawLine(rayStart, hit.point, Color.green, 0.1f);
            Debug.DrawRay(hit.point, hit.normal * 2f, Color.blue, 0.1f);
            
            Vector3 projectedForward = Vector3.ProjectOnPlane(targetDirection, hit.normal).normalized;
            Quaternion targetRotation = Quaternion.LookRotation(projectedForward, hit.normal);
            
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, steeringSpeed * Time.deltaTime);
        }
        else
        {
            //JUST STEER FLAT IF IT ALL GOES TITS UP
            Quaternion targetRotation = Quaternion.LookRotation(targetDirection, Vector3.up);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, steeringSpeed * Time.deltaTime);
        }
    }

    void MoveToNextGoal()
    {
        currentGoalIndex++;
        
        if (currentGoalIndex >= goals.Length)
        {
            currentGoalIndex = 0;
        }
        
        navMeshAgent.destination = goals[currentGoalIndex].position;
    }
}