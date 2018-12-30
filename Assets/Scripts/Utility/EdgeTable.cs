using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ComputerGraphic
{
    public class EdgeTable
    {
        Dictionary<ValueTuple<int, int>, List<Triangle>> dict = new Dictionary<(int, int), List<Triangle>>();

        public IEnumerable<Triangle> this[ValueTuple<int, int> edge]    // Indexer declaration  
        {
            get
            {
                var key = EdgeToKey(edge);
                if(dict.ContainsKey(key) == false)
                {
                    dict[key] = new List<Triangle>();
                }
                return dict[EdgeToKey(edge)];
            }
        }

        public void Add(Triangle triangle)
        {
            ValueTuple<int, int> key1 = EdgeToKey((triangle.A, triangle.B));
            ValueTuple<int, int> key2 = EdgeToKey((triangle.B, triangle.C));
            ValueTuple<int, int> key3 = EdgeToKey((triangle.C, triangle.A));

            if (dict.ContainsKey(key1) == false)
            {
                dict[key1] = new List<Triangle>();
            }

            if (dict.ContainsKey(key2) == false)
            {
                dict[key2] = new List<Triangle>();
            }

            if (dict.ContainsKey(key3) == false)
            {
                dict[key3] = new List<Triangle>();
            }

            dict[key1].Add(triangle);
            dict[key2].Add(triangle);
            dict[key3].Add(triangle);
        }

        public void Remove(Triangle triangle)
        {
            ValueTuple<int, int> key1 = EdgeToKey((triangle.A, triangle.B));
            ValueTuple<int, int> key2 = EdgeToKey((triangle.B, triangle.C));
            ValueTuple<int, int> key3 = EdgeToKey((triangle.C, triangle.A));
            dict[key1].Remove(triangle);
            dict[key2].Remove(triangle);
            dict[key3].Remove(triangle);
        }

        private ValueTuple<int, int> EdgeToKey(ValueTuple<int, int> edge)
        {
            return edge.Item1 < edge.Item2 ? (edge.Item1, edge.Item2) : (edge.Item2, edge.Item1);
        }

    }

}