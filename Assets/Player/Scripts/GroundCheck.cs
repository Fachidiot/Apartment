using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GroundCheck : MonoBehaviour
{
    [Header("Boxcast Property")]
    [SerializeField] private Vector3 _boxSize = new Vector3(0.5f, -0.1f, 0.5f);
    [SerializeField] private float _maxDistance = 0.11f;
    [SerializeField] private LayerMask _groundLayer;

    [Header("Debug")]
    [SerializeField] private bool _drawGizmo;
    [SerializeField] private bool _debugConsole;

    private void OnDrawGizmos()
    {
        if (!_drawGizmo) return;

        Gizmos.color = Color.cyan;
        Gizmos.DrawCube(transform.position - transform.up * _maxDistance, _boxSize);
    }

    public bool IsGrounded()
    {
        RaycastHit hit;
        bool isHit = Physics.BoxCast(transform.position, _boxSize, -transform.up, out hit, transform.rotation, _maxDistance, _groundLayer);
#if UNITY_EDITOR
        if (_debugConsole)
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
