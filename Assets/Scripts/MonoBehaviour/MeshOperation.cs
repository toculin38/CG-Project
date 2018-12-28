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
        print(meshFilter.mesh.vertices.Length);
        SubdivideMesh(meshFilter.mesh);
        print(meshFilter.mesh.vertices.Length);
    }

    private void AddElement(List<int> values) {
        values.Add(1);
        values.Add(2);
    }

    private void SubdivideMesh(Mesh mesh) {

        List<Vector3> vertices = mesh.vertices.ToList();
        List<Vector3> normals = mesh.normals.ToList();

        List<int> indices = new List<int>();//empty list, this is for new mesh.triangles

        int[] triangles = mesh.triangles; //original triangles

        Dictionary<uint, int> midVectices = new Dictionary<uint, int>(); //To prevent the repeat point

        #region Step 1
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

                int middleVertexIndex;

                uint t1 = ((uint)index1 << 16) | (uint)index2;
                uint t2 = ((uint)index2 << 16) | (uint)index1;

                if (midVectices.ContainsKey(t2))
                {
                    middleVertexIndex = midVectices[t2];
                }
                else if (midVectices.ContainsKey(t1))
                {
                    middleVertexIndex = midVectices[t1];
                }
                else
                {
                    //Get new Point
                    middleVertexIndex = vertices.Count;
                    //Add to dictionary
                    midVectices.Add(t1, middleVertexIndex);
                    //Add new Vertex
                    vertices.Add((vertices[index1] + vertices[index2]) * 0.5f);
                    //Add new Normal
                    normals.Add((normals[index1] + normals[index2]).normalized);
                }

                //Add new Triangles
                indices.AddRange(new int[] { index0, index1, middleVertexIndex });
                indices.AddRange(new int[] { index0, middleVertexIndex, index2 });
            }
            else
            {
                //Add original Triangle
                indices.AddRange(new int[] { index0, index1, index2 });
            }
        }
        #endregion

        mesh.vertices = vertices.ToArray();
        mesh.triangles = indices.ToArray();
        mesh.normals = normals.ToArray();
    }

}


