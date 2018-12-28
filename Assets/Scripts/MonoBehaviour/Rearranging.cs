using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Rearranging : MonoBehaviour
{

    [SerializeField] MeshFilter meshFilter;
    // Use this for initialization
    void Start()
    {
        //畫你的初始mesh
        Mesh mesh = new Mesh();
        mesh.vertices = new Vector3[] {
            new Vector3(-3, -2, 0), new Vector3(-1, 2, 0), new Vector3(2, 2, 0),
            new Vector3(0, -2, 0), new Vector3(-5, -4, 0), new Vector3(-2, -4, 0),
            new Vector3(3, -4, 0), new Vector3(3, -2, 0), new Vector3(10, 0, 0)
        };
        mesh.triangles = new int[] { 0, 1, 2, 2, 3, 0, 0, 3, 4, 4, 3, 5, 5, 3, 6, 6, 3, 7, 7, 3, 8, 8, 3, 2 };
        mesh.RecalculateNormals();
        meshFilter.mesh = mesh;
        //如果你要用bunny試，把以上全部砍掉，然後把bunny拉到meshFilter裡
    }

    public void RearrangeMesh()
    {
        print("Rearranging Mesh");

        //拿原始Mesh
        List<Vector3> vertices = meshFilter.mesh.vertices.ToList();
        List<int> triangles = meshFilter.mesh.triangles.ToList();

        //TODO: 寫rearrange邏輯
        //vertices = new List<Vector3> { new Vector3(-3, -2, 0), new Vector3(-1, 2, 0), new Vector3(2, 2, 0), new Vector3(0, -2, 0) };
        //triangles = new List<int> { 1, 2, 3, 0, 1, 3 };

        List<int> longestEdge = new List<int>();//empty list, this is for longest edge
        List<int> indices = new List<int>();//empty list, this is for new mesh.triangles


        for (int i = 0; i + 2 < triangles.Count; i += 3)
        {
            //Find middle point in the longest edge and link Vertex index
            float maxDistance = 0;
            int indexPos = 0;
            int index0 = triangles[i];
            int index1 = triangles[i + 1];
            int index2 = triangles[i + 2];
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
                    indexPos = j;

                }
            }
            longestEdge.AddRange(new int[] {index1, index2, indexPos});
        }

    
        for (int i = 0; i + 2 < triangles.Count; i += 3)
        {
            int triangleIndex0 = triangles[i];
            int triangleIndex1 = triangles[i + 1];
            int triangleIndex2 = triangles[i + 2];
            for (int j = i + 3; j + 2 < triangles.Count; j += 3)
            {
                if((longestEdge[i] == longestEdge[j + 1]) && (longestEdge[i + 1] == longestEdge[j]))
                {   //rearranging
                    triangles[i + (longestEdge[i + 2] + 1) % 3] = triangles[j + longestEdge[j + 2]];
                    triangles[j + (longestEdge[j + 2] + 1) % 3] = triangles[i + longestEdge[i + 2]];
                }
    
            }

            indices.AddRange(new int[] { triangles[i], triangles[i + 1], triangles[i + 2] });
        }

      
        //重新渲染
        meshFilter.mesh.vertices = vertices.ToArray();
        meshFilter.mesh.triangles = indices.ToArray();
        meshFilter.mesh.RecalculateNormals();
    }

}
