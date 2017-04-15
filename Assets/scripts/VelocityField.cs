//
// VelocityField - Utilities for creative coding and game with Unity
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

using UnityEngine;

[ExecuteInEditMode]
public class VelocityField : BezierCurveBase
{
    [Header("Velocity Field")]
    [Tooltip("Create Velocity Field")]
    public bool CreateVelocityField = true;

    [Tooltip("Update Velocity Field every frame")]
    public bool UpdateEveryFrame = false;

    [Tooltip("Visualize the radius that the velocity field will affect")]
    public bool ShowSearchRadius = true;

    [Tooltip("Raidus - the size of the radius that the velocity field will affect")]
    [Range(0.1f, 100)]
    public float SearchRadius = 5f;

    [Tooltip("Map the magnitude of curve velocity")]
    public AnimationCurve VelocityMagCurve = AnimationCurve.EaseInOut(0f, 0.8f, 1f, 1.0f);

    public bool Visualize = true;
    public float DisplayVelocityLength = 12f;

    protected override void Start()
    {
        base.Start();

        if (CreateVelocityField)
        {
            CalculateVelocityField();
        }
    }

    protected override void Update()
    {
        base.Update();

#if UNITY_EDITOR
        CalculateVelocityField(); // just run calculating the velocity field everytime we iterate on editor..
#endif
        if (UpdateEveryFrame)
        {
            CalculateVelocityField();
        }
    }

    public void CalculateVelocityField()
    {
        VelocityField.Clear();

        Vector3 prevPos = P0.transform.position;
        for (int c = 1; c <= Resolution; c++)
        {
            float t = (float)c / Resolution;
            Vector3 currPos = CurveMath.CalculateBezierPoint(t, P0.transform.position, P0_Tangent.transform.position, P1_Tangent.transform.position, P1.transform.position);
            Vector3 currTan = (currPos - prevPos).normalized;
            float mag = VelocityMagCurve.Evaluate(t);

            VelocityFieldNode ti = new VelocityFieldNode();
            ti.TargetPosition = prevPos;
            ti.TargetVelocity = currTan;
            ti.Mag = mag;
            VelocityField.Add(ti);
            prevPos = currPos;
        }
    }

    override public void OnDrawGizmos()
    {
        base.OnDrawGizmos();

        if (ShowSearchRadius)
        {
            Gizmos.color = GizmoColor;
            Gizmos.DrawWireSphere(P0.transform.position, SearchRadius);
            Gizmos.DrawWireSphere(P1.transform.position, SearchRadius);
        }

        if (Visualize)
        {
#if UNITY_EDITOR
            CalculateVelocityField();
#endif
            float MaxMag = float.MinValue;
            float MinMag = float.MaxValue;

            for (int i = 0; i < VelocityField.Count; i++)
            {
                if (VelocityField[i].Mag > MaxMag)
                {
                    MaxMag = VelocityField[i].Mag;
                }
                if (VelocityField[i].Mag < MinMag)
                {
                    MinMag = VelocityField[i].Mag;
                }
            }

            for (int i = 1; i < VelocityField.Count; i++)
            {
                float color = Remap(VelocityField[i - 1].Mag, MinMag, MaxMag, 0.05f, 1f);

                Color colorShift = new Color(GizmoColor.r * color, GizmoColor.g * color, GizmoColor.b * color);

                Gizmos.color = colorShift;
                Vector3 direction = transform.TransformDirection((VelocityField[i].TargetPosition - VelocityField[i - 1].TargetPosition).normalized) * DisplayVelocityLength;
                Gizmos.DrawRay(VelocityField[i - 1].TargetPosition, direction);
            }
        }
    }
}