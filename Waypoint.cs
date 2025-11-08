using UnityEngine;

public class Waypoint : MonoBehaviour
{
    [Header("Waypoint Type")]
    public bool isBrakeZone = false;
    
    [Header("Brake Zone Settings")]
    [Tooltip("Target speed percentage when braking (0.5 = 50% of normal speed)")]
    [Range(0.1f, 1.0f)]
    public float targetSpeedMultiplier = 0.5f;
    
    [Header("Visual Debug")]
    public bool showGizmo = true;
    public float gizmoSize = 0.5f;

    private void OnDrawGizmos()
    {
        if (!showGizmo) return;

        Gizmos.color = isBrakeZone ? Color.red : Color.green;
        Gizmos.DrawSphere(transform.position, gizmoSize);
        
        if (isBrakeZone)
        {
            Gizmos.color = new Color(1f, 0f, 0f, 0.3f);
            Gizmos.DrawWireSphere(transform.position, gizmoSize * 2f);
        }
    }
}
