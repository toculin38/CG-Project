using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SinLoop : MonoBehaviour
{

    private float devideValue = 100;

    // Update is called once per frame
    void Update()
    {
        Mesh mesh = GetComponent<MeshFilter>().mesh;
        Vector3[] vertices = mesh.vertices;
        Vector3[] normals = mesh.normals;

        for (var i = 0; i < vertices.Length; i++)
        {
            vertices[i] += normals[i] * Mathf.Sin(Time.time) / devideValue;
        }

        mesh.vertices = vertices;
    }
}
