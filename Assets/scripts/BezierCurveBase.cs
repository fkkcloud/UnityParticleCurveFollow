//
// BezierCurveBase - Utilities for creative coding and game  with Unity
//
// Copyright (C) 2017 Jae Hyun Yoo
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.
//

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Description : Using a curve to create velocity field and update particle's velocity/position with it
public class VelocityFieldNode
{
    public Vector3 TargetPosition;
    public Vector3 TargetVelocity;
    public float Mag;
}

public class BezierCurveBase : MonoBehaviour
{
    [Header("Spline Property")]
    [Tooltip("How many actual vertices needed to create this curve")]
    [Range(5, 100)]
    public int Resolution = 24;
    [Tooltip("Hide the guide obj when executed")]
    bool HideOnExecute = true;

    [Header("Curve Guides")]
    public GameObject P0;
    public GameObject P1;
    public GameObject P0_Tangent;
    public GameObject P1_Tangent;

    public Color GizmoColor = new Color(1, 0, 0, 0.25f);
    public List<VelocityFieldNode> VelocityField = new List<VelocityFieldNode>();
    public bool ShowCurve = true;

    protected virtual void Start()
    {
        SetupGuideComp();

        if (HideOnExecute)
        {
            DisableTransforms();
        }
    }

    protected virtual void Update() { }

    void SetupGuideComp()
    {
        Debug.Log("Generating Helpers");

        if (!P0)
        {
            P0 = GameObject.CreatePrimitive(PrimitiveType.Cube);
            P0.transform.parent = transform;
			P0.transform.localPosition = new Vector3(10f, 0f, 0f);
            P0.transform.localScale = new Vector3(0.25f, 0.25f, 0.25f);
            P0.name = "P0";
        }
        if (!P1)
        {
            P1 = GameObject.CreatePrimitive(PrimitiveType.Cube);
            P1.transform.parent = transform;
			P1.transform.localPosition = new Vector3(-10f, 0f, 0f);
            P1.transform.localScale = new Vector3(0.25f, 0.25f, 0.25f);
            P1.name = "P1";
        }
        if (!P0_Tangent)
        {
            P0_Tangent = GameObject.CreatePrimitive(PrimitiveType.Cube);
            P0_Tangent.transform.parent = transform;
			P0_Tangent.transform.localPosition = new Vector3(10f, 0f, 10f);
            P0_Tangent.transform.localScale = new Vector3(0.25f, 0.25f, 0.25f);
            P0_Tangent.name = "P0_Tangent";
            P0_Tangent.transform.SetParent(P0.transform);

        }
        if (!P1_Tangent)
        {
            P1_Tangent = GameObject.CreatePrimitive(PrimitiveType.Cube);
            P1_Tangent.transform.parent = transform;
			P1_Tangent.transform.localPosition = new Vector3(-10f, 0f, -10f);
            P1_Tangent.transform.localScale = new Vector3(0.25f, 0.25f, 0.25f);
            P1_Tangent.name = "P1_Tangent";
            P1_Tangent.transform.SetParent(P1.transform);
        }
    }

    public void GetPositionAt(float val, ref Vector3 pos)
    {
        val = Mathf.Clamp(val, 0.0f, 1.0f);
        pos = CurveMath.CalculateBezierPoint(val, P0.transform.position, P0_Tangent.transform.position, P1_Tangent.transform.position, P1.transform.position);
    }

    public virtual void OnDestroy()
    {
        DestroyImmediate(P1_Tangent);
        DestroyImmediate(P0_Tangent);
        DestroyImmediate(P1);
        DestroyImmediate(P0);        
    }

    public virtual void OnDrawGizmos()
    {
        if (!ShowCurve || !P0 || !P1 || !P0_Tangent || !P1_Tangent)
            return;

        //draw tangents
        Gizmos.color = new Color(1, 0, 0, 1.0f);
        Gizmos.DrawLine(P0.transform.position, P0_Tangent.transform.position);
        Gizmos.color = new Color(1, 0, 0, 1.0f);
        Gizmos.DrawLine(P1.transform.position, P1_Tangent.transform.position);

        //draw curve
        Vector3 prevPos = P0.transform.position;
        for (int c = 1; c <= Resolution; c++)
        {
            float t = (float)c / Resolution;
            Vector3 currPos = CurveMath.CalculateBezierPoint(t, P0.transform.position, P0_Tangent.transform.position, P1_Tangent.transform.position, P1.transform.position);
            Vector3 currTan = (currPos - prevPos).normalized;
            float mag = (currPos - prevPos).magnitude;

            Gizmos.color = new Color(0, 0, mag, 1);
            Gizmos.DrawLine(prevPos, currPos);

            prevPos = currPos;
        }
    }

    public float Remap(float value, float from1, float to1, float from2, float to2)
    {
        return (value - from1) / (to1 - from1) * (to2 - from2) + from2;
    }

    void DisableTransforms()
    {
        if (Application.isPlaying)
        {
            P1_Tangent.SetActive(false);
            P0_Tangent.SetActive(false);
            P1.SetActive(false);
            P0.SetActive(false);
        }
    }
}
