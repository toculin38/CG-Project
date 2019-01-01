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

        private TriangleTable triangleTable = new TriangleTable();

        public CG_Mesh(Mesh mesh)
        {
            vertices = mesh.vertices.ToList();
            normals = mesh.normals.ToList();

            int[] indices = mesh.triangles;

            for (int i = 0; i + 2 < indices.Length; i += 3)
            {
                AddTriangle(new Triangle((indices[i], indices[i + 1], indices[i + 2]), vertices));
            }
        }

        private void SetTriangle(int index, Triangle triangle)
        {
            Triangle oldTriangle = triangles[index];
            triangles[index] = triangle;

            triangleTable.Remove(oldTriangle);
            triangleTable.Add(triangle);
        }

        private void AddTriangle(Triangle triangle)
        {
            triangles.Add(triangle);
            triangleTable.Add(triangle);
        }

        private void RemoveTriangle(Triangle triangle)
        {
            triangles.Remove(triangle);
            triangleTable.Remove(triangle);
        }

        public void Subdivision(float threashold)
        {
            #region Find All nonconfirm edge
            List<ValueTuple<int, int>> nonConfirmEdges = triangles.Where(t => TriangleArea(t) > threashold).Select(t => (t.A, t.B)).ToList();
            #endregion

            #region Begin Subdivide
            while (nonConfirmEdges.Count > 0)
            {
                ValueTuple<int, int>[] edges = nonConfirmEdges.ToArray();

                foreach (var edge in edges)
                {
                    if (triangleTable.ContainsEdge(edge))
                    {
                        Triangle[] neighbors = triangleTable[edge];

                        foreach (var triangle in neighbors)
                        {
                            if (triangle.IsLongestEdge(edge))
                            {
                                SubdivideTriangle(triangle);
                            }
                            else
                            {
                                nonConfirmEdges.Add((triangle.A, triangle.B));
                            }
                        }
                    }
                    else
                    {
                        nonConfirmEdges.Remove(edge);
                    }
                }
            }
            #endregion
        }

        public void Rearranging()
        {
            for (int i = 0; i < triangles.Count; i++)
            {
                Triangle triangle = triangles[i];

                for (int j = i; j < triangles.Count; j++)
                {
                    Triangle another = triangles[j];

                    bool rearrangeCondition =
                        triangle.IsLongestEdge((another.A, another.B)) && //share same longest edge
                        triangle.IsSame(another) == false &&
                        PointsDistance(another.A, another.B) > PointsDistance(another.C, triangle.C); //prevent repeated

                    if (rearrangeCondition)
                    {
                        SetTriangle(i, new Triangle((triangle.C, another.C, another.A), vertices));
                        SetTriangle(j, new Triangle((another.C, triangle.C, triangle.A), vertices));
                        break; //We assumed that an edge only composed two triangle at best, so no need to do the rest iteration.
                    }

                }
            }
        }

        public void AssignToMesh(Mesh mesh)
        {
            mesh.vertices = vertices.ToArray();
            mesh.normals = normals.ToArray();
            mesh.triangles = triangles.SelectMany(t => new int[] { t.A, t.B, t.C }).ToArray();
        }

        private void SubdivideTriangle(Triangle triangle)
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

            RemoveTriangle(triangle);
            AddTriangle(new Triangle((diagonalPoint, edgePoint1, midVertIndex), vertices));
            AddTriangle(new Triangle((diagonalPoint, midVertIndex, edgePoint2), vertices));
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

        public void CalculatePos(float deltaT)
        {
            for (int i = 0; i < vertices.Count; i++)
            {
                List<int> triangleIndex = new List<int>();
                for (int j = 0; j < triangles.Count; j++)
                {
                    if (triangles[j].A == i || triangles[j].B == i || triangles[j].C == i)
                    {
                        triangleIndex.Add(j);
                    }
                }

                Vector3 fi = CalculateFi(triangleIndex, i);
                Vector3 gi = CalculateGi(i);

                vertices[i] = vertices[i] + deltaT * (fi - gi);
            }
        }

        private Vector3 CalculateFi(List<int> triangleIndex, int j)
        {
            Vector3 surroundingNormal = Vector3.zero;
            Vector3 inflationForce = Vector3.zero;
            for (int i = 0; i < triangleIndex.Count; i++)
            {
                surroundingNormal = normals[j];

                if (triangles[i].A == j)
                {
                    surroundingNormal = FindNeighborNormal(triangles[i].B, triangles[i].C);
                }
                else if (triangles[i].B == j)
                {
                    surroundingNormal = FindNeighborNormal(triangles[i].A, triangles[i].C);
                }
                else if (triangles[i].C == j)
                {
                    surroundingNormal = FindNeighborNormal(triangles[i].A, triangles[i].B);
                }


                inflationForce = inflationForce + surroundingNormal.normalized;
            }

            inflationForce = inflationForce.normalized;

            return inflationForce;
        }

        private Vector3 FindNeighborNormal(int index0, int index1)
        {
            Vector3 surroundingNormal = Vector3.zero;

            Triangle[] neighbors = triangleTable[(index0, index1)];

            if (neighbors.Length > 0)
            {
                foreach (var triangle in neighbors)
                {
                    Vector3 norm = Vector3.Cross(vertices[triangle.B] - vertices[triangle.A], vertices[triangle.C] - vertices[triangle.A]);
                    surroundingNormal = surroundingNormal + norm.normalized;
                }
            }
            return surroundingNormal.normalized;
        }

        private Vector3 CalculateGi(int i)
        {
            Vector3 Sij = Vector3.zero;
            Vector3 Rij = Vector3.zero;

            for (int j = 0; j < vertices.Count && i != j; j++)
            {
                bool isAnEdge = triangleTable.ContainsEdge((i, j));

                if (isAnEdge)
                {
                    Rij = vertices[j] - vertices[i];
                    Sij = Sij + Rij;
                }
            }

            return Sij.normalized * 0.1f;
        }

        private float PointsDistance(int v1, int v2)
        {
            return Vector3.Distance(vertices[v1], vertices[v2]);
        }
    }
}