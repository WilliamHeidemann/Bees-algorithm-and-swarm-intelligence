using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class Asteroid : MonoBehaviour
{
    public int resource = 50;

    private void Start()
    {
        resource = Random.Range(10, 100);
    }
}
