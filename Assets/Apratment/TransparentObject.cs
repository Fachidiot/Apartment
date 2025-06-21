using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class TransparentObject : MonoBehaviour
{
    [SerializeField] private GameObject[] gameObjects;
    [SerializeField] private Material transparentMaterial;
    [SerializeField] private Material originalMaterial;

    void OnTriggerEnter(Collider other)
    {
        foreach (GameObject obj in gameObjects)
        {
            MeshRenderer renderer = obj.GetComponent<MeshRenderer>();
            renderer.material = transparentMaterial;
        }
    }

    void OnTriggerExit(Collider other)
    {
        foreach (GameObject obj in gameObjects)
        {
            MeshRenderer renderer = obj.GetComponent<MeshRenderer>();
            renderer.material = originalMaterial;
        }
    }
}
