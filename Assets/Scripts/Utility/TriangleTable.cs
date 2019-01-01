using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace ComputerGraphic
{

    public class TriangleTable
    {
        private Dictionary<int, List<Triangle>> vertDict = new Dictionary<int, List<Triangle>>();
        private Dictionary<ValueTuple<int, int>, List<Triangle>> edgeDict = new Dictionary<(int, int), List<Triangle>>();

        public Triangle[] this[int vert]
        {
            get
            {
                return vertDict[vert].ToArray();
            }
        }

        public Triangle[] this[ValueTuple<int, int> edge]
        {
            get
            {
                return edgeDict[EdgeToKey(edge)].ToArray();
            }
        }

        public void Add(Triangle triangle)
        {
            AddVertices(triangle);
            AddEdges(triangle);
        }

        public void Remove(Triangle triangle)
        {
            RemoveVertices(triangle);
            RemoveEdges(triangle);
        }

        private void AddVertices(Triangle triangle)
        {
            int key1 = triangle.A;
            int key2 = triangle.B;
            int key3 = triangle.C;

            if (vertDict.ContainsKey(key1) == false)
            {
                vertDict[key1] = new List<Triangle>();
            }

            if (vertDict.ContainsKey(key2) == false)
            {
                vertDict[key2] = new List<Triangle>();
            }

            if (vertDict.ContainsKey(key3) == false)
            {
                vertDict[key3] = new List<Triangle>();
            }

            vertDict[key1].Add(triangle);
            vertDict[key2].Add(triangle);
            vertDict[key3].Add(triangle);
        }

        private void RemoveVertices(Triangle triangle)
        {
            int key1 = triangle.A;
            int key2 = triangle.B;
            int key3 = triangle.C;

            vertDict[key1].Remove(triangle);
            vertDict[key2].Remove(triangle);
            vertDict[key3].Remove(triangle);

            if (!vertDict[key1].Any()) {
                vertDict.Remove(key1);
            }

            if (!vertDict[key2].Any())
            {
                vertDict.Remove(key2);
            }

            if (!vertDict[key3].Any())
            {
                vertDict.Remove(key3);
            }
        }

        private void AddEdges(Triangle triangle)
        {
            ValueTuple<int, int> key1 = EdgeToKey((triangle.A, triangle.B));
            ValueTuple<int, int> key2 = EdgeToKey((triangle.B, triangle.C));
            ValueTuple<int, int> key3 = EdgeToKey((triangle.C, triangle.A));

            if (edgeDict.ContainsKey(key1) == false)
            {
                edgeDict[key1] = new List<Triangle>();
            }

            if (edgeDict.ContainsKey(key2) == false)
            {
                edgeDict[key2] = new List<Triangle>();
            }

            if (edgeDict.ContainsKey(key3) == false)
            {
                edgeDict[key3] = new List<Triangle>();
            }

            edgeDict[key1].Add(triangle);
            edgeDict[key2].Add(triangle);
            edgeDict[key3].Add(triangle);
        }

        private void RemoveEdges(Triangle triangle)
        {
            ValueTuple<int, int> key1 = EdgeToKey((triangle.A, triangle.B));
            ValueTuple<int, int> key2 = EdgeToKey((triangle.B, triangle.C));
            ValueTuple<int, int> key3 = EdgeToKey((triangle.C, triangle.A));
            edgeDict[key1].Remove(triangle);
            edgeDict[key2].Remove(triangle);
            edgeDict[key3].Remove(triangle);

            if (!edgeDict[key1].Any())
            {
                edgeDict.Remove(key1);
            }

            if (!edgeDict[key2].Any())
            {
                edgeDict.Remove(key2);
            }

            if (!edgeDict[key3].Any())
            {
                edgeDict.Remove(key3);
            }
        }

        public bool ContainsEdge(ValueTuple<int, int> edge)
        {
            return edgeDict.ContainsKey(EdgeToKey(edge));
        }

        public bool ContainsVert(int vert)
        {
            return vertDict.ContainsKey(vert);
        }

        private ValueTuple<int, int> EdgeToKey(ValueTuple<int, int> edge)
        {
            return edge.Item1 < edge.Item2 ? (edge.Item1, edge.Item2) : (edge.Item2, edge.Item1);
        }
    }

}