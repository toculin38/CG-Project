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

    static Dictionary<uint, int> newVectices;

    public void SubdevideMesh(MeshFilter meshFilter)
    {
        Mesh mesh = meshFilter.mesh;

        List<Vector3> vertices = meshFilter.mesh.vertices.ToList();
        List<Vector3> normals = meshFilter.mesh.normals.ToList();

        List<int> indices = new List<int>();//empty list, this is for new mesh.triangles

        int[] triangles = meshFilter.mesh.triangles; //original triangle

        for (int i = 0; i + 2 < triangles.Length; i += 3)
        {
            int index0 = triangles[i];
            int index1 = triangles[i + 1];
            int index2 = triangles[i + 2];

            float area = CG_Utility.CalculateTriangleArea(vertices[index0], vertices[index1], vertices[index2]);

            if (area > areaThreshold)
            {
                //Find middle point in the longest edge and link Vertex index
                float maxDistance = 0;

                for (int j = 0; j < 3; j++)
                {
                    int newLinkIndex = triangles[i + j];
                    int newIndex1 = triangles[i + ((j + 1) % 3)];
                    int newIndex2 = triangles[i + ((j + 2) % 3)];
                    float distance = Vector3.Distance(vertices[newIndex1], vertices[newIndex2]);

                    if (distance > maxDistance)
                    {
                        index0 = newLinkIndex;
                        index1 = newIndex1;
                        index2 = newIndex2;
                        maxDistance = distance;
                    }
                }

                //Get new Point
                int middlePointIndex = vertices.Count;
                Vector3 middlePoint = (vertices[index1] + vertices[index2]) * 0.5f;
                //Add new Vertex
                vertices.Add(middlePoint);
                //Add new Normal
                normals.Add((normals[index1] + normals[index2]).normalized);
                //Add new Triangles
                indices.AddRange(new int[] { index0, index1, middlePointIndex });
                indices.AddRange(new int[] { index0, middlePointIndex, index2 });
            }
            else
            {
                //Add original Triangle
                indices.AddRange(new int[] { index0, index1, index2 });
            }
        }

        mesh.vertices = vertices.ToArray();
        mesh.triangles = indices.ToArray();
        mesh.normals = normals.ToArray();

        meshFilter.mesh = mesh;
    }

}


