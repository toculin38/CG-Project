using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace ComputerGraphic
{
    public class CG_Mesh
    {
        private List<Vector3> vertices = new List<Vector3>();
        private List<Vector3> normals = new List<Vector3>();

        private List<Triangle> triangles = new List<Triangle>();// the triangles composed this mesh
        private Dictionary<uint, int> midVectices = new Dictionary<uint, int>(); //To prevent the repeat vertex

        public CG_Mesh(Mesh mesh)
        {
            vertices = mesh.vertices.ToList();
            normals = mesh.normals.ToList();

            int[] tArray = mesh.triangles;

            for (int i = 0; i + 2 < tArray.Length; i += 3)
            {
                triangles.Add(new Triangle((tArray[i], tArray[i + 1], tArray[i + 2]), vertices));
            }
        }

        public void SubdivideAlgorithm(float threashold)
        {
            List<Triangle> newTriangles = new List<Triangle>();
            List<ValueTuple<int, int>> nonConfirmEdges = new List<ValueTuple<int, int>>();

            #region Step 1
            for (int i = 0; i < triangles.Count; i++)
            {
                Triangle triangle = triangles[i];

                if (TriangleArea(triangle) > threashold)
                {
                    nonConfirmEdges.Add((triangle.A, triangle.B));
                    Triangle[] subTriangles = SubdivideTriangle(triangle);
                    newTriangles.AddRange(subTriangles);
                }
                else
                {
                    newTriangles.Add(triangle);
                }
            }

            triangles.Clear();
            triangles.AddRange(newTriangles);

            #endregion
            while (nonConfirmEdges.Count > 0)
            {
                ValueTuple<int, int>[] edges = nonConfirmEdges.ToArray();

                foreach (var edge in edges)
                {
                    Triangle triangle = triangles.FirstOrDefault(t => t.ContainsEdge(edge));

                    if (triangle != default(Triangle))
                    {
                        if (triangle.IsLongestEdge(edge))
                        {
                            Triangle[] subTriangles = SubdivideTriangle(triangle);
                            triangles.Remove(triangle);
                            triangles.AddRange(subTriangles);
                            nonConfirmEdges.Add((triangle.A, triangle.B));
                        }
                        else
                        {
                            nonConfirmEdges.Add((triangle.A, triangle.B));
                            Debug.Log("Not longest edge case");
                        }
                    }
                    else
                    {
                        nonConfirmEdges.Remove(edge);
                    }
                }
            }

        }

        private Triangle[] SubdivideTriangle(Triangle triangle)
        {
            int edgePoint1 = triangle.A;
            int edgePoint2 = triangle.B;
            int diagonalPoint = triangle.C;

            if (IsMidVertexInDict(triangle.A, triangle.B, out uint key, out int midVertIndex) == false)
            {
                midVectices.Add(key, midVertIndex);
                vertices.Add((vertices[edgePoint1] + vertices[edgePoint2]) * 0.5f);
                normals.Add((normals[edgePoint1] + normals[edgePoint2]).normalized);
            }

            return new Triangle[] {
                new Triangle((diagonalPoint, edgePoint1, midVertIndex),vertices),
                new Triangle((diagonalPoint, midVertIndex, edgePoint2),vertices)
            };
        }

        private bool IsMidVertexInDict(int index1, int index2, out uint key, out int value)
        {
            uint t1 = ((uint)index1 << 16) | (uint)index2;
            uint t2 = ((uint)index2 << 16) | (uint)index1;

            if (midVectices.ContainsKey(t2))
            {
                key = t2;
                value = midVectices[t2];
                return true;
            }
            else if (midVectices.ContainsKey(t1))
            {
                key = t1;
                value = midVectices[t1];
                return true;
            }
            else
            {
                key = t1;
                value = vertices.Count;
                return false;
            }
        }

        private float TriangleArea(Triangle triangle)
        {
            Vector3 v1 = vertices[triangle.A];
            Vector3 v2 = vertices[triangle.B];
            Vector3 v3 = vertices[triangle.C];
            Vector3 crossVector = Vector3.Cross(v1 - v2, v1 - v3);
            return crossVector.magnitude * 0.5f;
        }

        public Vector3[] GetVertices()
        {
            return vertices.ToArray();
        }

        public Vector3[] GetNormals()
        {
            return normals.ToArray();
        }

        public int[] GetTriangles()
        {
            return triangles.SelectMany(t => new int[] { t.A, t.B, t.C }).ToArray();
        }
    }

    /// <summary>
    /// A triangle with 3 point indices, A and B point will make a longest edge in this triangle.
    /// </summary>
    public class Triangle
    {
        //Important! A and B will make a longest edge in this triangle
        public int A { get; private set; }
        public int B { get; private set; }
        public int C { get; private set; }

        public Triangle(ValueTuple<int, int, int> points, List<Vector3> refVertices)
        {
            int[] vertsIndices = new int[] { points.Item1, points.Item2, points.Item3 };

            float longestLength = 0;

            for (int i = 0; i < 3; i++)
            {
                int p1 = vertsIndices[i];
                int p2 = vertsIndices[(i + 1) % vertsIndices.Length];
                int p3 = vertsIndices[(i + 2) % vertsIndices.Length];
                float length = Vector3.Distance(refVertices[p1], refVertices[p2]);

                if (length > longestLength)
                {
                    longestLength = length;
                    A = p1;
                    B = p2;
                    C = p3;
                }
            }
        }

        public bool IsLongestEdge(ValueTuple<int, int> edge)
        {
            int p1 = edge.Item1;
            int p2 = edge.Item2;
            return (A == p1 && B == p2) || (B == p1 && A == p2);
        }

        public bool ContainsEdge(ValueTuple<int, int> edge)
        {
            int p1 = edge.Item1;
            int p2 = edge.Item2;

            return
                (A == p1 && B == p2 || A == p2 && B == p1) ||
                (A == p1 && C == p2 || A == p2 && C == p1) ||
                (B == p1 && C == p2 || B == p2 && C == p1);
        }
    }
}