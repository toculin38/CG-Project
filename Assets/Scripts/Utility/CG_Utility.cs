using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace ComputerGraphic
{
    public static class CG_Utility
    {
        public static float CalculateTriangleArea(Vector3 A, Vector3 B, Vector3 C)
        {
            Vector3 crossVector = Vector3.Cross(A - B, A - C);
            return crossVector.magnitude * 0.5f;
        }
    }

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
                triangles.Add(new Triangle(tArray[i], tArray[i + 1], tArray[i + 2]));
            }
        }

        public void SubdivideAlgorithm(float threashold)
        {
            List<Triangle> newTriangles = new List<Triangle>();
            List<int> nonConfirmPoints = new List<int>();
            List<ValueTuple<int, int>> nonConfirmEdges = new List<ValueTuple<int, int>>();

            #region Step 1
            for (int i = 0; i < triangles.Count; i++)
            {
                Triangle triangle = triangles[i];

                if (TriangleArea(triangle) > threashold)
                {
                    ValueTuple<int, int, int> dividePoints = FindDividePointsByLongestEdge(triangle);
                    Triangle[] subTriangles = SubdivideTriangle(dividePoints);
                    newTriangles.AddRange(subTriangles);
                }
                else
                {
                    newTriangles.Add(triangle);
                }
            }
            triangles = newTriangles;
            #endregion
        }

        private ValueTuple<int, int, int> FindDividePointsByLongestEdge(Triangle triangle)
        {
            int[] vertsIndices = new int[] { triangle.A, triangle.B, triangle.C };
            float longestLength = 0;
            int index1 = 0, index2 = 0, diagnalIndex = 0;
            for (int i = 0; i < 3; i++)
            {
                int p1 = vertsIndices[i];
                int p2 = vertsIndices[(i + 1) % vertsIndices.Length];
                int p3 = vertsIndices[(i + 2) % vertsIndices.Length];
                float length = Vector3.Distance(vertices[p1], vertices[p2]);

                if (length > longestLength)
                {
                    longestLength = length;
                    index1 = p1;
                    index2 = p2;
                    diagnalIndex = p3;
                }
            }
            return (index1, index2, diagnalIndex);
        }

        private Triangle[] SubdivideTriangle(ValueTuple<int, int, int> dividePoints)
        {
            int edgePoint1 = dividePoints.Item1;
            int edgePoint2 = dividePoints.Item2;
            int diagonalPoint = dividePoints.Item3;

            if (IsMidVertexInDict(edgePoint1, edgePoint2, out uint key, out int midVertIndex) == false)
            {
                midVectices.Add(key, midVertIndex);
                vertices.Add((vertices[edgePoint1] + vertices[edgePoint2]) * 0.5f);
                normals.Add((normals[edgePoint1] + normals[edgePoint2]).normalized);
            }

            return new Triangle[] {
                new Triangle(diagonalPoint, edgePoint1, midVertIndex),
                new Triangle(diagonalPoint, midVertIndex, edgePoint2)
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

        private float TriangleArea(Triangle traiangle)
        {
            Vector3 v1 = vertices[traiangle.A];
            Vector3 v2 = vertices[traiangle.B];
            Vector3 v3 = vertices[traiangle.C];
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

    public class Triangle
    {
        public int A;
        public int B;
        public int C;

        public Triangle(int a, int b, int c)
        {
            A = a;
            B = b;
            C = c;
        }

        public bool ContainsEdge(int v1, int v2)
        {
            return
                (A == v1 && B == v2 || A == v2 && B == v1) ||
                (A == v1 && C == v2 || A == v2 && C == v1) ||
                (B == v1 && C == v2 || B == v2 && C == v1);
        }
    }
}