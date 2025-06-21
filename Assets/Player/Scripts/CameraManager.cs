using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraManager : MonoBehaviour
{
    [SerializeField] private Vector3 _delta = new Vector3(0f, 6f, -5f);
    [SerializeField] private GameObject player;
    [SerializeField] private Material transparent;

    private Material original;
    private MeshRenderer lastRenderer;
    void LateUpdate()
    {
        RaycastHit hit;
        if (Physics.Raycast(player.transform.position, _delta, out hit, _delta.magnitude, LayerMask.GetMask("Wall")))
        {
            MeshRenderer renderer = hit.collider.gameObject.GetComponent<MeshRenderer>();
            lastRenderer = renderer;
            original = renderer.material;
            renderer.material = transparent;
        }
        else
        {
            if (lastRenderer)
                lastRenderer.material = original;
            transform.position = player.transform.position + _delta; transform.LookAt(player.transform);
        }
    }
}
