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
        mesh.vertices = new Vector3[] { new Vector3(-3, -2, 0), new Vector3(-1, 2, 0), new Vector3(2, 2, 0), new Vector3(0, -2, 0) };
        mesh.triangles = new int[] { 0, 1, 2, 2, 3, 0 };
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
        vertices = new List<Vector3> { new Vector3(-3, -2, 0), new Vector3(-1, 2, 0), new Vector3(2, 2, 0), new Vector3(0, -2, 0) };
        triangles = new List<int> { 1, 2, 3, 0, 1, 3 };

        //重新渲染
        meshFilter.mesh.vertices = vertices.ToArray();
        meshFilter.mesh.triangles = triangles.ToArray();
        meshFilter.mesh.RecalculateNormals();
    }

}
