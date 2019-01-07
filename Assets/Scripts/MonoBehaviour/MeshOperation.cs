using ComputerGraphic;
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
    [SerializeField]
    Mesh[] fMeshes;
    [SerializeField]
    Mesh[] sMeshes;

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
        //DrawApple();
        //DrawIco();
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

    public void StartBakeMeshes()
    {
        Vector3[] objWorldVertices = objMeshFilter.mesh.vertices.Select(v => objMeshFilter.transform.TransformPoint(v)).ToArray();
        Vector3[] objVertices = objWorldVertices.Select(v => icoMeshFilter.transform.InverseTransformPoint(v)).ToArray();
        int[] objTriangles = objMeshFilter.mesh.triangles;


        int preVertNum;
        int iteration = 0;

        do
        {
            preVertNum = icoMeshFilter.mesh.vertexCount;
            CG_Mesh cgMesh = new CG_Mesh(icoMeshFilter.mesh, objVertices, objTriangles, isVertexAnchored);
            cgMesh.CalculatePos(deltaT);
            cgMesh.AssignToMesh(icoMeshFilter.mesh);
            BakeMesh("Assets/MeshAnim/Apple/f" + iteration + ".mesh");
            cgMesh.Subdivision(areaThreshold);
            cgMesh.Rearranging();
            cgMesh.AssignToMesh(icoMeshFilter.mesh);
            BakeMesh("Assets/MeshAnim/Apple/s" + iteration + ".mesh");
            iteration++;
        } while (preVertNum != icoMeshFilter.mesh.vertexCount);

    }

    void BakeMesh(string path)
    {

        icoMeshFilter.mesh = Instantiate(icoMeshFilter.mesh);
        AssetDatabase.CreateAsset(icoMeshFilter.mesh, path);
        AssetDatabase.SaveAssets();
    }

    IEnumerator Inflating(float deltaT)
    {
        animating = true;

        for (int i = 0; i < fMeshes.Length && i < sMeshes.Length; i++)
        {
            Vector3[] startVertices = icoMeshFilter.mesh.vertices;
            Vector3[] startNormals = icoMeshFilter.mesh.normals;

            Vector3[] endVertices = fMeshes[i].vertices;
            Vector3[] endNormals = fMeshes[i].normals;

            Vector3[] frameVertices = new Vector3[startVertices.Length];
            Vector3[] frameNormals = new Vector3[startNormals.Length];

            for (float dTimer = 0; dTimer < deltaT; dTimer += Time.deltaTime)
            {
                float lerpRatio = dTimer / deltaT;

                for (int j = 0; j < frameVertices.Length; j++)
                {
                    frameVertices[j] = Vector3.Lerp(startVertices[j], endVertices[j], lerpRatio);
                    frameNormals[j] = Vector3.Lerp(startNormals[j], endNormals[j], lerpRatio);
                }

                icoMeshFilter.mesh.vertices = frameVertices;
                icoMeshFilter.mesh.normals = frameNormals;
                yield return null;
            }

            icoMeshFilter.mesh = sMeshes[i];
        }


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


