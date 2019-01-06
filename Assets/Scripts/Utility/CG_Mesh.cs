using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace ComputerGraphic
{
    public class CG_Mesh
    {
        private Mesh objMesh;

        private List<Vector3> vertices = new List<Vector3>();
        private List<Vector3> normals = new List<Vector3>();
        private List<Triangle> triangles = new List<Triangle>();// the triangles composed this mesh
        private Dictionary<uint, int> midVectices = new Dictionary<uint, int>(); //To prevent the repeat vertex

        private TriangleTable triangleTable = new TriangleTable();

        public CG_Mesh(Mesh mesh, Mesh objMesh)
        {
            this.objMesh = objMesh;

            vertices = mesh.vertices.ToList();
            normals = mesh.normals.ToList();

            int[] indices = mesh.triangles;

            for (int i = 0; i + 2 < indices.Length; i += 3)
            {
                AddTriangle(new Triangle((indices[i], indices[i + 1], indices[i + 2]), vertices));
            }
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
                Triangle another = triangleTable[(triangle.A, triangle.B)].FirstOrDefault(a =>
                    triangle.IsSame(a) == false && // not the same triangle
                    triangle.IsLongestEdge((a.A, a.B)) && //share same longest edge
                    PointsDistance(a.A, a.B) > PointsDistance(a.C, triangle.C)//need to rearrange
                    );

                if (another != default(Triangle))
                {
                    RearrangeTriangles(triangle, another);
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

        private void RearrangeTriangles(Triangle triangleA, Triangle triangleB)
        {
            RemoveTriangle(triangleA);
            RemoveTriangle(triangleB);
            AddTriangle(new Triangle((triangleA.C, triangleB.C, triangleB.A), vertices));
            AddTriangle(new Triangle((triangleB.C, triangleA.C, triangleA.A), vertices));
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

                Vector3 nextPos = vertices[i] + deltaT * (fi - gi);


                if (IsVertexInObjMesh(vertices[i], normals[i], out Vector3 intersection))
                {

                }

                if (Vector3.Distance(vertices[i], intersection) < Vector3.Distance(vertices[i], nextPos))
                {
                    vertices[i] = intersection;
                }
                else
                {
                    vertices[i] = nextPos;
                }

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

        bool IsVertexInObjMesh(Vector3 vertex, Vector3 normal, out Vector3 firstIntersect)
        {
            Vector3[] objVertices = objMesh.vertices;
            int[] indices = objMesh.triangles;

            Ray ray = new Ray(vertex, normal);
            int hitCount = 0;

            firstIntersect = vertex;

            for (int i = 0; i + 2 < indices.Length; i += 3)
            {
                Vector3 v0 = objVertices[indices[i]];
                Vector3 v1 = objVertices[indices[i + 1]];
                Vector3 v2 = objVertices[indices[i + 2]];

                if (Intersect3D_RayTriangle(ray, v0, v1, v2, out Vector3 vi))
                {
                    if (hitCount == 0)
                    {
                        firstIntersect = vi;
                    }

                    hitCount++;
                }
            }

            return hitCount % 2 == 1;
        }

        struct RayTriangleInfo
        {
            public Vector3 n;
            public float uu;
            public float uv;
            public float vv;
            public float D;
        }

        Dictionary<ValueTuple<Vector3, Vector3, Vector3>, RayTriangleInfo> objRayTriangleTable = new Dictionary<(Vector3, Vector3, Vector3), RayTriangleInfo>();

        bool Intersect3D_RayTriangle(Ray ray, Vector3 v0, Vector3 v1, Vector3 v2, out Vector3 intersectPoint)
        {
            intersectPoint = default(Vector3);

            Vector3 u, v, n;              // triangle vectors
            Vector3 dir, w0, w;           // ray vectors
            float r, a, b;              // params to calc ray-plane intersect

            float uu, uv, vv, wu, wv, D;

            // get triangle edge vectors and plane normal
            u = v1 - v0;
            v = v2 - v0;

            //Table是否存在以計算過的資訊
            if (objRayTriangleTable.ContainsKey((v0, v1, v2)))
            {
                RayTriangleInfo info = objRayTriangleTable[(v0, v1, v2)];
                n = info.n;
                uu = info.uu;
                uv = info.uv;
                vv = info.vv;
                D = info.D;
            }
            else
            {
                n = Vector3.Cross(u, v);              // cross product
                uu = Vector3.Dot(u, u);
                uv = Vector3.Dot(u, v);
                vv = Vector3.Dot(v, v);
                D = uv * uv - uu * vv;

                RayTriangleInfo info = new RayTriangleInfo()
                {
                    n = n,
                    uu = uu,
                    uv = uv,
                    vv = vv,
                    D = D
                };

                objRayTriangleTable.Add((v0, v1, v2), info);
            }

            if (n == Vector3.zero)             // triangle is degenerate
                return false;                  // do not deal with this case

            dir = ray.direction;              // ray direction vector
            w0 = ray.origin - v0;
            a = -Vector3.Dot(n, w0);
            b = Vector3.Dot(n, dir);

            // get intersect point of ray with triangle plane
            r = a / b;

            if (r < 0.0)
            {                // ray goes away from triangle
                return false;                   // => no intersect
            }              // for a segment, also test if (r > 1.0) => no intersect

            intersectPoint = ray.origin + r * dir;            // intersect point of ray and plane

            // is intersect point inside Triangle?
            w = intersectPoint - v0;
            wu = Vector3.Dot(w, u);
            wv = Vector3.Dot(w, v);

            // get and test parametric coords
            float s, t;
            s = (uv * wv - vv * wu) / D;
            if (s < 0.0 || s > 1.0)
            {
                return false;// intersect point is outside Triangle
            }

            t = (uv * wu - uu * wv) / D;
            if (t < 0.0 || (s + t) > 1.0)
            {
                return false;// intersect point is outside Triangle
            }

            return true;// intersect point is in Triangle
        }

    }
}