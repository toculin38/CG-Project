using ComputerGraphic;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class MeshOperation : MonoBehaviour
{
    [Header("Parameters")]

    [SerializeField]
    private float areaThreshold;
    [SerializeField]
    private float deltaT = 0.5f;

    private bool animating = false;

    void Start()
    {
        //StartInflate();
    }

    void Update()
    {

    }

    public void StartInflate(MeshFilter meshFilter)
    {
        if (animating)
        {
            Debug.Log("Animating!");
        }
        else
        {
            StartCoroutine(Inflating(meshFilter, deltaT));
        }
    }

    IEnumerator Inflating(MeshFilter meshFilter, float deltaT)
    {
        animating = true;

        Mesh newMesh = new Mesh();

        CG_Mesh cgMesh = new CG_Mesh(meshFilter.mesh);
        cgMesh.CalculatePos(deltaT);
        cgMesh.AssignToMesh(newMesh);

        Vector3[] startVertices = meshFilter.mesh.vertices;
        Vector3[] startNormals = meshFilter.mesh.normals;

        Vector3[] endVertices = newMesh.vertices;
        Vector3[] endNormals = newMesh.normals;

        Vector3[] frameVertices = new Vector3[startVertices.Length];
        Vector3[] frameNormals = new Vector3[startNormals.Length];

        for (float dTimer = 0; dTimer < deltaT; dTimer += Time.deltaTime)
        {
            float lerpRatio = dTimer / deltaT;

            for (int i = 0; i < frameVertices.Length; i++)
            {
                frameVertices[i] = Vector3.Lerp(startVertices[i], endVertices[i], lerpRatio);
                frameNormals[i] = Vector3.Lerp(startNormals[i], endNormals[i], lerpRatio);
            }

            meshFilter.mesh.vertices = frameVertices;
            meshFilter.mesh.normals = frameNormals;
            yield return null;
        }

        cgMesh.Subdivision(areaThreshold);
        cgMesh.Rearranging();
        cgMesh.AssignToMesh(meshFilter.mesh);
        animating = false;
    }

    public void Subdivision(MeshFilter meshFilter)
    {
        CG_Mesh cgMesh = new CG_Mesh(meshFilter.mesh);
        cgMesh.Subdivision(areaThreshold);
        cgMesh.AssignToMesh(meshFilter.mesh);
    }

    public void Rearranging(MeshFilter meshFilter)
    {
        CG_Mesh cgMesh = new CG_Mesh(meshFilter.mesh);
        cgMesh.Rearranging();
        cgMesh.AssignToMesh(meshFilter.mesh);
    }

    public void CalculatePosition(MeshFilter meshFilter)
    {
        CG_Mesh cgMesh = new CG_Mesh(meshFilter.mesh);
        cgMesh.CalculatePos(deltaT);
        cgMesh.AssignToMesh(meshFilter.mesh);
    }

}


