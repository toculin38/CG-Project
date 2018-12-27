using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Inflate : MonoBehaviour
{

    [SerializeField] MeshFilter meshFilter;

    // Update is called once per frame
    void Update()
    {
        Mesh mesh = meshFilter.mesh;
        mesh.vertices = UpdateVertices(mesh);
    }

    Vector3[] UpdateVertices(Mesh mesh)
    {
        Vector3[] vertices = mesh.vertices;
        Vector3[] normals = mesh.normals;

        for (var i = 0; i < vertices.Length; i++)
        {
            vertices[i] = vertices[i] + normals[i] * Mathf.Sin(Time.time) / 100;
        }

        return vertices;
    }

}
