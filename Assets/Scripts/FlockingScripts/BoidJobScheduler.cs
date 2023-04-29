using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DroneScripts;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;
using UnityEngine.Jobs;

public class BoidJobScheduler : MonoBehaviour
{
    private List<BoidBehaviour> _boidBehaviours;
    private List<Transform> _boidTransforms;
    private void Update()
    {
        _boidBehaviours = FindObjectsOfType<BoidBehaviour>().ToList();
        _boidTransforms = _boidBehaviours.Select(boid => boid.transform).ToList();

        var positions = _boidTransforms.Select(trans => trans.position).ToArray();
        var nativeArrayPositions = new NativeArray<Vector3>(positions, Allocator.TempJob);
        
        var forwards = _boidTransforms.Select(trans => trans.forward).ToArray();
        var nativeArrayForwards = new NativeArray<Vector3>(forwards, Allocator.TempJob);
        
        var boidJob = new CalculateBoidValueJob()
        {
            boidPositions = nativeArrayPositions,
            boidForwards = nativeArrayForwards,
            boids = new NativeArray<BoidValues>(_boidTransforms.Count, Allocator.TempJob)
        };
        
        var jobHandle = boidJob.Schedule(_boidTransforms.Count, 1);
        jobHandle.Complete();
        
        for (int i = 0; i < _boidBehaviours.Count; i++)
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

[BurstCompile]
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