using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GroundCheck : MonoBehaviour
{
    [Header("Boxcast Property")]
    [SerializeField] private Vector3 boxSize = new Vector3(0.5f, 0.25f, 0.5f);
    [SerializeField] private float maxDistance = 1f;
    [SerializeField] private LayerMask groundLayer;

    [Header("Debug")]
    [SerializeField] private bool drawGizmo;
    [SerializeField] private bool debugConsole;

    private void OnDrawGizmos()
    {
        if (!drawGizmo) return;

        Gizmos.color = Color.cyan;
        Gizmos.DrawCube(transform.position - transform.up * maxDistance, boxSize);
    }

    public bool IsGrounded()
    {
        RaycastHit hit;
        bool isHit = Physics.BoxCast(transform.position, boxSize, -transform.up, out hit, transform.rotation, maxDistance, groundLayer);
#if UNITY_EDITOR
        if (debugConsole)
        {
            if (isHit)
                Debug.Log($"Hit: {hit.collider.name} on layer {LayerMask.LayerToName(hit.collider.gameObject.layer)}");
            else
                Debug.Log("No ground detected.");
        }
#endif
        return isHit;
        // return Physics.BoxCast(transform.position, boxSize, -transform.up, transform.rotation, maxDistance, groundLayer);
    }
}
