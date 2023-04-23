using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DroneScripts;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;
using UnityEngine.Jobs;

public class BoidJobScheduler : MonoBehaviour
{
    public float separationDistance = 25.0f;
    public float cohesionDistance = 50.0f;
    private BoidBehaviour[] _boidBehaviours;
    private Transform[] _boidTransforms;
    
    private void Update()
    {
        if (_boidBehaviours == null)
        {
            _boidBehaviours = FindObjectsOfType<BoidBehaviour>();
            _boidTransforms = _boidBehaviours.Select(boid => boid.transform).ToArray();
        }
        
        var positions = _boidTransforms.Select(trans => trans.position).ToArray();
        var nativeArrayPositions = new NativeArray<Vector3>(positions, Allocator.TempJob);
        
        var forwards = _boidTransforms.Select(trans => trans.forward).ToArray();
        var nativeArrayForwards = new NativeArray<Vector3>(forwards, Allocator.TempJob);
        
        var boidJob = new CalculateBoidValueJob()
        {
            boidPositions = nativeArrayPositions,
            boidForwards = nativeArrayForwards,
            boids = new NativeArray<BoidValues>(_boidTransforms.Length, Allocator.TempJob)
        };
        
        var jobHandle = boidJob.Schedule(_boidTransforms.Length, 1);
        jobHandle.Complete();
        
        for (int i = 0; i < _boidBehaviours.Length; i++)
        {
            _boidBehaviours[i].flockDirection = boidJob.boids[i].flockDirection;
            _boidBehaviours[i].flockCentre = boidJob.boids[i].flockCentre;
            _boidBehaviours[i].separationDirection = boidJob.boids[i].separationDirection;
        }

        boidJob.boidPositions.Dispose();
        boidJob.boidForwards.Dispose();
        boidJob.boids.Dispose();
    }
}
public struct BoidValues
{
    public Vector3 flockDirection;
    public Vector3 flockCentre;
    public Vector3 separationDirection;
}

public struct CalculateBoidValueJob : IJobParallelFor
{
    [ReadOnly] public NativeArray<Vector3> boidPositions;
    [ReadOnly] public NativeArray<Vector3> boidForwards;
    private const float separationDistance = 25.0f;
    private const float cohesionDistance = 50.0f;
    public NativeArray<BoidValues> boids;
    
    public void Execute(int index)
    {
        var boidValues = boids[index];
        var boidsInCohesionRange = 0;
        for (int i = 0; i < boids.Length; i++)
        {
            var offset = boidPositions[i] - boidPositions[index];
            var distance = Vector3.Magnitude(offset);
            if (distance == 0) continue;
            if (distance < cohesionDistance)
            {
                boidsInCohesionRange += 1;
                boidValues.flockDirection += boidForwards[i];
                boidValues.flockCentre += boidPositions[i];
                if (distance < separationDistance)
                {
                    boidValues.separationDirection -= offset / distance;
                }
            }
        }
        if (boidsInCohesionRange > 0) boidValues.flockCentre /= boidsInCohesionRange;
        boids[index] = boidValues;
    }
}