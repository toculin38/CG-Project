using ComputerGraphic;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class MeshOperation : MonoBehaviour
{
    [Header("Parameters")]
    [SerializeField]
    float areaThreshold;

    void Start()
    {

    }

    void Update()
    {

    }

    public void Subdivision(MeshFilter meshFilter)
    {
        CG_Mesh cgMesh = new CG_Mesh(meshFilter.mesh);
        cgMesh.SubdivideAlgorithm(areaThreshold);

        print(meshFilter.mesh.vertices.Length);

        meshFilter.mesh.vertices = cgMesh.GetVertices();
        meshFilter.mesh.normals = cgMesh.GetNormals();
        meshFilter.mesh.triangles = cgMesh.GetTriangles();

        print(meshFilter.mesh.vertices.Length);
    }

}


