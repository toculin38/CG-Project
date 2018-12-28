using System.Collections;
using System.Collections.Generic;
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

}


