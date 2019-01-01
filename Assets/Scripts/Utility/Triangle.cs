using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ComputerGraphic
{
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
        /// <summary>
        /// determine two triangle is same. (not object reference equal! e.g. obj1.equals(obj2))
        /// </summary>
        /// <param name="triangle"></param>
        /// <returns></returns>
        public bool IsSame(Triangle triangle)
        {
            return A == triangle.A && B == triangle.B && C == triangle.C;
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