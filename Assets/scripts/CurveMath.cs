using System.Collections;
using System.Collections.Generic;
using UnityEngine;

static public class CurveMath {

    // create bezier point on specific time t : 0 ~ 1
    static public Vector3 CalculateBezierPoint(float t, Vector3 start_pos, Vector3 start_tangent, Vector3 end_tangent, Vector3 end_pos)
    {
        float u = 1 - t;
        float tt = t * t;
        float uu = u * u;
        float uuu = uu * u;
        float ttt = tt * t;

        Vector3 p = uuu * start_pos; //first term
        p += 3 * uu * t * start_tangent; //second term
        p += 3 * u * tt * end_tangent; //third term
        p += ttt * end_pos; //fourth term

        return p;
    }
}
