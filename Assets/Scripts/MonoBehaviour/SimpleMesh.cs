using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SimpleMesh : MonoBehaviour
{
    [SerializeField] MeshFilter meshFilter;
    // Use this for initialization
    void Start()
    {
        //畫初始mesh
        Mesh mesh = new Mesh();
        mesh.vertices = new Vector3[] {
            new Vector3(-3, -2, 0), new Vector3(-1, 2, 0), new Vector3(2, 2, 0),
            new Vector3(0, -2, 0), new Vector3(-5, -4, 0), new Vector3(-2, -4, 0),
            new Vector3(3, -4, 0), new Vector3(3, -2, 0), new Vector3(10, 0, 0),
            new Vector3(5, -8, 0)
        };
        mesh.triangles = new int[] {
            0, 1, 2,
            2, 3, 0,
            0, 3, 4,
            4, 3, 5,
            5, 3, 6,
            6, 3, 7,
            7, 3, 8,
            8, 3, 2,
            5, 6, 9
        };
        mesh.RecalculateNormals();
        meshFilter.mesh = mesh;
    }
}
