﻿using ComputerGraphic;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
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

    void Start()
    {
        /*
        Mesh newMesh = objMeshFilter.mesh;

        Vector3[] newVertices = newMesh.vertices;

        for (int i = 0; i < newVertices.Length; i++)
        {
            newVertices[i] *= 0.04f;
        }

        newMesh.vertices = newVertices;

        AssetDatabase.CreateAsset(newMesh, "Assets/newApple.mesh" );
        AssetDatabase.SaveAssets();*/
        
        for (int i = 0; i < icoMeshFilter.mesh.vertices.Length; i++)
        {
            isVertexAnchored.Add(false);
        }
    }

    void Update()
    {
        DrawApple();
        DrawIco();
    }

    void DrawApple()
    {
        Vector3[] vertices = objMeshFilter.mesh.vertices;
        int[] triangles = objMeshFilter.mesh.triangles;

        for (int i = 0; i + 2 < triangles.Length; i += 3)
        {
            Vector3 v0 = objMeshFilter.transform.TransformPoint(vertices[triangles[i]]);
            Vector3 v1 = objMeshFilter.transform.TransformPoint(vertices[triangles[i + 1]]);
            Vector3 v2 = objMeshFilter.transform.TransformPoint(vertices[triangles[i + 2]]);
            Debug.DrawLine(v0, v1, Color.blue);
            Debug.DrawLine(v1, v2, Color.blue);
            Debug.DrawLine(v2, v0, Color.blue);
        }
    }

    void DrawIco()
    {
        Vector3[] vertices = icoMeshFilter.mesh.vertices;
        int[] triangles = icoMeshFilter.mesh.triangles;

        for (int i = 0; i + 2 < triangles.Length; i += 3)
        {
            Vector3 v0 = icoMeshFilter.transform.TransformPoint(vertices[triangles[i]]);
            Vector3 v1 = icoMeshFilter.transform.TransformPoint(vertices[triangles[i + 1]]);
            Vector3 v2 = icoMeshFilter.transform.TransformPoint(vertices[triangles[i + 2]]);
            Debug.DrawLine(v0, v1, Color.red);
            Debug.DrawLine(v1, v2, Color.red);
            Debug.DrawLine(v2, v0, Color.red);
        }
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
        //把兔子的局部座標點(local)轉成世界座標點(world)
        Vector3[] objWorldVertices = objMeshFilter.mesh.vertices.Select(v => objMeshFilter.transform.TransformPoint(v)).ToArray();
        //把這些世界座標點(world)轉成正二十面體的局部座標點(local)
        Vector3[] objVertices = objWorldVertices.Select(v => icoMeshFilter.transform.InverseTransformPoint(v)).ToArray();
        int[] objTriangles = objMeshFilter.mesh.triangles;
        CG_Mesh cgMesh = new CG_Mesh(icoMeshFilter.mesh, objVertices, objTriangles, isVertexAnchored);
        cgMesh.CalculatePos(deltaT);
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
        Vector3[] objWorldVertices = objMeshFilter.mesh.vertices.Select(v => objMeshFilter.transform.TransformPoint(v)).ToArray();
        Vector3[] objVertices = objWorldVertices.Select(v => icoMeshFilter.transform.InverseTransformPoint(v)).ToArray();
        int[] objTriangles = objMeshFilter.mesh.triangles;
        CG_Mesh cgMesh = new CG_Mesh(icoMeshFilter.mesh, objVertices, objTriangles, isVertexAnchored);
        cgMesh.Subdivision(areaThreshold);
        cgMesh.AssignToMesh(icoMeshFilter.mesh);
    }

    public void Rearranging()
    {
        Vector3[] objWorldVertices = objMeshFilter.mesh.vertices.Select(v => objMeshFilter.transform.TransformPoint(v)).ToArray();
        Vector3[] objVertices = objWorldVertices.Select(v => icoMeshFilter.transform.InverseTransformPoint(v)).ToArray();
        int[] objTriangles = objMeshFilter.mesh.triangles;
        CG_Mesh cgMesh = new CG_Mesh(icoMeshFilter.mesh, objVertices, objTriangles, isVertexAnchored);
        cgMesh.Rearranging();
        cgMesh.AssignToMesh(icoMeshFilter.mesh);
    }

    public void CalculatePosition()
    {
        Vector3[] objWorldVertices = objMeshFilter.mesh.vertices.Select(v => objMeshFilter.transform.TransformPoint(v)).ToArray();
        Vector3[] objVertices = objWorldVertices.Select(v => icoMeshFilter.transform.InverseTransformPoint(v)).ToArray();
        int[] objTriangles = objMeshFilter.mesh.triangles;
        CG_Mesh cgMesh = new CG_Mesh(icoMeshFilter.mesh, objVertices, objTriangles, isVertexAnchored);
        cgMesh.CalculatePos(deltaT);
        cgMesh.AssignToMesh(icoMeshFilter.mesh);
    }

}


