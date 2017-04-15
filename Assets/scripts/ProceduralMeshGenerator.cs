//
// Procedural Mesh Generation - Utilities for creative coding and game  with Unity
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

using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class ProceduralMeshGenerator : BezierCurveBase
{

    public enum OrientEnum { XOriented, YOriented, ZOriented };

    [Header("Create Procedural Mesh")]
    public GameObject MeshObj;

    public bool CreateMesh = false;
    public bool IsTwoSided = true;
    public OrientEnum Orient;
    public bool FlipSide = false;
    public bool FlipUV = false;
    public float CustomCurveMultL = 1.0f;
    public float CustomCurveMultR = 1.0f;

    //let's say you edit from inspector, but you can built at runtime if you prefer
    public AnimationCurve CurveL = AnimationCurve.Linear(0f, 1f, 1f, 1f); //AnimationCurve.EaseInOut(0f, 0.1f, 1f, 0.9f);

    //let's say you edit from inspector, but you can built at runtime if you prefer
    public AnimationCurve CurveR = AnimationCurve.Linear(0f, 1f, 1f, 1f); // AnimationCurve.EaseInOut(0f, 0.1f, 1f, 0.9f);

    private MeshFilter Filter;
    private List<Vector3> Vertices = new List<Vector3>();
    private List<Vector2> UVs = new List<Vector2>();
    private List<int> Triangles = new List<int>();
    private Vector3[] CrossVectors = new Vector3[2];
    private Mesh ProceduralMesh;    

    protected override void Update()
    {
        base.Update();

        if (CreateMesh)
        {
            if (!MeshObj)
            {
                MeshObj = new GameObject();
                MeshObj.AddComponent<MeshFilter>();
                MeshObj.AddComponent<MeshRenderer>();
                MeshObj.transform.parent = transform;
                MeshObj.transform.position = new Vector3(0f, 0f, 0f);
                MeshObj.transform.localScale = new Vector3(1f, 1f, 1f);
                MeshObj.name = "ProceduralMesh";
                Filter = MeshObj.GetComponent<MeshFilter>();
                Filter.sharedMesh = new Mesh();
            }
            else if (MeshObj && !Filter)
            {
                Filter = MeshObj.GetComponent<MeshFilter>();
                Filter.sharedMesh = new Mesh();
            }

            CreateProceduralMesh();
        }
        else if (MeshObj && Filter)
        {
            RemoveProceduralMesh();
        }
    }

    private void CreateProceduralMesh()
    {
        Vertices.Clear();
        Triangles.Clear();
        UVs.Clear();

        Vector3[] pos = new Vector3[Resolution];

        GetCorePoints(ref pos);

        float t;

        float lastId = (float)(pos.Length - 1);
        for (int i = 0; i < pos.Length; i++)
        {
            t = i / (lastId); //  0.0 ~ 1.0
            CalculateSideVectors(ref pos, i, CustomCurveMultR * CurveR.Evaluate(t), CustomCurveMultL * CurveL.Evaluate(t));
            AddCurvePoint(CrossVectors[1], CrossVectors[0], i, pos.Length - 1);
        }

        Mesh mesh = Filter.sharedMesh;
        mesh.Clear();

        /*
        Vector3[] normales = new Vector3[vertices.Length];
        for (int n = 0; n < normales.Length; n++)
            normales[n] = Vector3.up;
        */

        mesh.vertices = Vertices.ToArray();
        //mesh.normals = normales;
        mesh.uv = UVs.ToArray();
        mesh.triangles = Triangles.ToArray();

        mesh.RecalculateBounds();
        //mesh.RecalculateNormals();

        if (MeshObj != null)
        {
            MeshObj.transform.position = Vector3.zero;
            MeshObj.transform.localScale = Vector3.one;
            MeshObj.transform.rotation = Quaternion.identity;
        }
    }

    private void RemoveProceduralMesh()
    {
        Vertices.Clear();
        Triangles.Clear();
        UVs.Clear();
        Mesh mesh = Filter.sharedMesh;
        mesh.Clear();
        DestroyImmediate(MeshObj);
    }

    private void CalculateSideVectors(ref Vector3[] pos, int i, float WidthR, float WidthL)
    {
        Vector3 tangent;
        if (i + 1 >= pos.Length)
        {
            tangent = (pos[i] - pos[i - 1]).normalized;
        }
        else
            tangent = (pos[i + 1] - pos[i]).normalized;

        float sign = 1f;
        //if (FlipSide)
        //    sign = -1f;

        Vector3 upvector = new Vector3(sign, pos[i].y, pos[i].z); // XOriented as default
        if (Orient == OrientEnum.YOriented)
            upvector = new Vector3(pos[i].x, sign, pos[i].z);
        else if (Orient == OrientEnum.ZOriented)
            upvector = new Vector3(pos[i].x, pos[i].y, sign);

        Vector3 toUpvector = (upvector - pos[i]);

        Vector3 CrossL = Vector3.Cross(tangent, toUpvector).normalized;
        Vector3 CrossR = CrossL * -1f;

        Vector3 r = pos[i] + CrossR * WidthR;
        Vector3 l = pos[i] + CrossL * WidthL;

        CrossVectors[0] = r;
        CrossVectors[1] = l;
    }

    private void GetCorePoints(ref Vector3[] pos)
    {
        Vector3 prevPos = P0.transform.position;
        pos[0] = prevPos;
        float lastId = (float)(Resolution - 1);
        for (int c = 1; c < Resolution; c++)
        {
            float t = c / lastId;
            pos[c] = CurveMath.CalculateBezierPoint(t, P0.transform.position, P0_Tangent.transform.position, P1_Tangent.transform.position, P1.transform.position);
            prevPos = pos[c];
        }
    }

    private void AddCurvePoint(Vector3 R, Vector3 L, int id, int count)
    {
        int start;

        Vertices.Add(R);
        Vertices.Add(L);

        if (FlipUV)
        {
            UVs.Add(new Vector2(0f, (float)id / count));
            UVs.Add(new Vector2(1f, (float)id / count));
        }
        else
        {
            UVs.Add(new Vector2((float)id / count, 0f));
            UVs.Add(new Vector2((float)id / count, 1f));
        }

        if (FlipSide)
        {
            if (Vertices.Count >= 4)
            {
                start = Vertices.Count - 4;
                Triangles.Add(start + 0);
                Triangles.Add(start + 2);
                Triangles.Add(start + 1);
                Triangles.Add(start + 1);
                Triangles.Add(start + 2);
                Triangles.Add(start + 3);
                // also create side for back so its two sided rendering mesh ,. or use custom shader
                if (IsTwoSided)
                {
                    start = Vertices.Count - 4;

                    Triangles.Add(start + 0);
                    Triangles.Add(start + 1);
                    Triangles.Add(start + 2);
                    Triangles.Add(start + 1);
                    Triangles.Add(start + 3);
                    Triangles.Add(start + 2);
                }
            }
        }
        else
        {
            if (Vertices.Count >= 4)
            {
                start = Vertices.Count - 4;
                Triangles.Add(start + 0);
                Triangles.Add(start + 1);
                Triangles.Add(start + 2);
                Triangles.Add(start + 1);
                Triangles.Add(start + 3);
                Triangles.Add(start + 2);
                // also create side for back so its two sided rendering mesh ,. or use custom shader
                if (IsTwoSided)
                {
                    start = Vertices.Count - 4;
                    Triangles.Add(start + 0);
                    Triangles.Add(start + 2);
                    Triangles.Add(start + 1);
                    Triangles.Add(start + 1);
                    Triangles.Add(start + 2);
                    Triangles.Add(start + 3);
                }
            }
        }
    }

    public override void OnDestroy()
    {
        base.OnDestroy();
        DestroyImmediate(MeshObj);
    }
}
