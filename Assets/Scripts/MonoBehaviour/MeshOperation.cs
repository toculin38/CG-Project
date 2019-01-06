﻿using ComputerGraphic;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class MeshOperation : MonoBehaviour
{
    [Header("Object reference")]
    [SerializeField]
    MeshFilter icoMeshFilter;
    [SerializeField]
    MeshFilter objMeshFilter;

    [Header("Parameters")]
    [SerializeField]
    private float areaThreshold;
    [SerializeField]
    private float deltaT;

    private bool animating = false;
    private List<bool> isVertexAnchored = new List<bool>();

    private CG_Mesh cgMesh;

    void Start()
    {
        //初始化
        //把兔子的局部座標點(local)轉成世界座標點(world)
        Vector3[] objWorldVertices = objMeshFilter.mesh.vertices.Select(v => objMeshFilter.transform.TransformPoint(v)).ToArray();
        //把這些世界座標點(world)轉成正二十面體的局部座標點(local)
        Vector3[] objVertices = objWorldVertices.Select(v => icoMeshFilter.transform.InverseTransformPoint(v)).ToArray();
        int[] objTriangles = objMeshFilter.mesh.triangles;

        cgMesh = new CG_Mesh(icoMeshFilter.mesh, objVertices, objTriangles, isVertexAnchored);

        for (int i = 0; i < icoMeshFilter.mesh.vertices.Length; i++)
        {
            isVertexAnchored.Add(false);
        }
    }

    void Update()
    {

    }

    public void StartInflate()
    {
        if (animating)
        {
            Debug.Log("Animating!");
        }
        else
        {
            StartCoroutine(Inflating(deltaT));
        }
    }

    IEnumerator Inflating(float deltaT)
    {
        animating = true;

        Mesh newMesh = new Mesh();
        isVertexAnchored = cgMesh.CalculatePos(deltaT);
        cgMesh.AssignToMesh(newMesh);

        Vector3[] startVertices = icoMeshFilter.mesh.vertices;
        Vector3[] startNormals = icoMeshFilter.mesh.normals;

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

            icoMeshFilter.mesh.vertices = frameVertices;
            icoMeshFilter.mesh.normals = frameNormals;
            yield return null;
        }

        cgMesh.Subdivision(areaThreshold);
        cgMesh.Rearranging();
        cgMesh.AssignToMesh(icoMeshFilter.mesh);
        animating = false;
    }

    public void Subdivision()
    {
        cgMesh.Subdivision(areaThreshold);
        cgMesh.AssignToMesh(icoMeshFilter.mesh);
    }

    public void Rearranging()
    {
        cgMesh.Rearranging();
        cgMesh.AssignToMesh(icoMeshFilter.mesh);
    }

    public void CalculatePosition()
    {
        isVertexAnchored = cgMesh.CalculatePos(deltaT);
        cgMesh.AssignToMesh(icoMeshFilter.mesh);
    }

}


