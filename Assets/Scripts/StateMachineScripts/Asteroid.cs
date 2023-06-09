using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class Asteroid : MonoBehaviour
{
    public float resource = 50;

    private void Start()
    {
        var position = transform.position;
        var x = position.x / 1000;
        var z = position.z / 1000;
        resource = (Mathf.PerlinNoise(x, z) * 100);
    }
}
