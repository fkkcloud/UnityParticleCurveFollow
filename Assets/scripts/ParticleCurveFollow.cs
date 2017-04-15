//
// Particle Curve Follow - Utilities for creative coding and game  with Unity
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

[ExecuteInEditMode]
public class ParticleCurveFollow : MonoBehaviour {


    [Header("VelocitySource")]
    [Tooltip("Use Bezier Curve to drive particle's position and velocity")]
    public VelocityField Curve;

    [Header("Particle Control")]
    [Tooltip("How fast the particle will move along the curve")]
    public float SpeedOnCurve = 1f;
    [Tooltip("How fast the particle will move to the curve")]
    public float ForceToNearestCurve = 0f;

    private ParticleSystem ParticleSys;
	private float SearchRadius = 5f;

    // Use this for initialization
    void Start ()
    {
        ParticleSys = GetComponent<ParticleSystem>();

        if (!ParticleSys)
        {
            Debug.LogError("There is no ParticleSystem component in this gameObject.");
            return;
        }

        // set the particle systems that particle surf is using set to simulation space to be world
        ParticleSystem.MainModule main = ParticleSys.main;
        if (main.simulationSpace != ParticleSystemSimulationSpace.World)
            main.simulationSpace = ParticleSystemSimulationSpace.World;

        SetupCurveData();
    }

    void Update() {

        if (!ParticleSys)
        {
            return;
        }

#if UNITY_EDITOR
        if (Curve)
        {
            Curve.CalculateVelocityField();
        }
#endif
        UpdateParticles();
    }

    public void SetupCurveData()
    {
        if (!Curve)
            return;

        SearchRadius = Curve.SearchRadius;
    }

    void UpdateParticles()
    {
        if (!ParticleSys)
            return;

        ParticleSystem.Particle[] ParticleList = new ParticleSystem.Particle[ParticleSys.particleCount];
        int NumParticleAlive = ParticleSys.GetParticles(ParticleList);
		for (int i = 0; i < NumParticleAlive; ++i)
        {
            ParticleSystem.Particle Particle = ParticleList[i];

            VelocityFieldNode velocityField = new VelocityFieldNode();

            bool IsImported = false;
            if (Curve)
            {
                IsImported = GetTargetNode(Particle, ref Curve.VelocityField, ref velocityField);
            }

            if (!IsImported)
                continue;

			Vector3 targetVelocity = velocityField.TargetVelocity * SpeedOnCurve * velocityField.Mag;

            // get vector from particle position to curve's iteration pos
            Vector3 toCurveVelocity = (velocityField.TargetPosition - Particle.position).normalized;
            targetVelocity += toCurveVelocity * ForceToNearestCurve;

            // apply hierarchy scale for velocity as well
            targetVelocity.x *= transform.lossyScale.x;
            targetVelocity.y *= transform.lossyScale.y;
            targetVelocity.z *= transform.lossyScale.z;

            ParticleList[i].position = Particle.position + (targetVelocity * Time.deltaTime);
            ParticleList[i].velocity = Particle.velocity;
        }
        ParticleSys.SetParticles(ParticleList, ParticleSys.particleCount);
    }

    private bool GetTargetNode(ParticleSystem.Particle Particle, ref List<VelocityFieldNode> velocityField, ref VelocityFieldNode targetInfo)
    {
        float minDist = float.MaxValue;
        VelocityFieldNode node = new VelocityFieldNode();
		for (int i = 0; i < velocityField.Count; i++)
        {
			float dist = Vector3.Distance(velocityField[i].TargetPosition, Particle.position);

			if (dist > SearchRadius)
				continue;
			
            if (dist < minDist)
            {
                minDist = dist;
                node = velocityField[i];
            }
        }

        targetInfo = node;
        return true;
    }
}
