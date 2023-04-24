using System;
using UnityEngine;

namespace DroneScripts
{
    public class BoidBehaviour : MonoBehaviour
    {
        private Rigidbody rb;
        
        //Boid Steering/Flocking Variables
        public float separationStrength = 250.0f;
        public float cohesionStrength = 25.0f;

        public Vector3 flockDirection;
        public Vector3 flockCentre;
        public Vector3 separationDirection;

        private Vector3 _acceleration;
        private void Start()
        {
            rb = GetComponent<Rigidbody>();
        }

        private void Update()
        {
            ApplyBoidSteering();
        }
        
        private void ApplyBoidSteering()
        {
            _acceleration = Vector3.zero;
            
            var alignmentForce = flockDirection;
            var cohesionForce = (flockCentre - transform.position) * cohesionStrength;
            var separationForce = separationDirection * separationStrength;

            _acceleration += alignmentForce;
            _acceleration += cohesionForce;
            _acceleration += separationForce;
            rb.AddForce(_acceleration.normalized * 15f);
        }
    }
}