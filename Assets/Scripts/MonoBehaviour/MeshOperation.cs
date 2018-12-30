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
    float deltaT = 0.5f;

    void Start()
    {

    }

    void Update()
    {

    }

    public void Subdivision(MeshFilter meshFilter)
    {
        CG_Mesh cgMesh = new CG_Mesh(meshFilter.mesh);
        cgMesh.Subdivision(areaThreshold);

        meshFilter.mesh.vertices = cgMesh.GetVertices();
        meshFilter.mesh.normals = cgMesh.GetNormals();
        meshFilter.mesh.triangles = cgMesh.GetTriangles();
    }

    public void Rearranging(MeshFilter meshFilter)
    {
        CG_Mesh cgMesh = new CG_Mesh(meshFilter.mesh);
        cgMesh.Rearranging();

        meshFilter.mesh.vertices = cgMesh.GetVertices();
        meshFilter.mesh.normals = cgMesh.GetNormals();
        meshFilter.mesh.triangles = cgMesh.GetTriangles();
    }

    public void CalculatePosition(MeshFilter meshFilter)
    {
        CG_Mesh cgMesh = new CG_Mesh(meshFilter.mesh);
        cgMesh.CalculatePos(deltaT);

        meshFilter.mesh.vertices = cgMesh.GetVertices();
        meshFilter.mesh.normals = cgMesh.GetNormals();
        meshFilter.mesh.triangles = cgMesh.GetTriangles();

    }

}


