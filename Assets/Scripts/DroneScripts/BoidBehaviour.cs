using System;
using UnityEngine;

namespace DroneScripts
{
    public class BoidBehaviour : MonoBehaviour
    {
        private Rigidbody rb;
        private GameManager gameManager;
        
        //Boid Steering/Flocking Variables
        public float separationDistance = 25.0f;
        public float cohesionDistance = 50.0f;
        public float separationStrength = 250.0f;
        public float cohesionStrength = 25.0f;

        private Vector3 _flockDirection;
        private Vector3 _flockCentre;
        private Vector3 _separationDirection;

        private Vector3 _acceleration;
        private void Start()
        {
            gameManager = FindObjectOfType<GameManager>();
            rb = GetComponent<Rigidbody>();
        }

        private void Update()
        {
            CalculateBoidValues();
            ApplyBoidSteering();
        }
        
        private void CalculateBoidValues()
        {
            _flockDirection = Vector3.zero;
            _flockCentre = Vector3.zero;
            _separationDirection = Vector3.zero;
            var boidsInCohesionRange = 0;

            var boids = gameManager.enemyList;
            foreach (var boid in boids)
            {
                var offset = boid.transform.position - transform.position;
                var distance = Vector3.Magnitude(offset);
                if (distance == 0) continue;
                if (distance < cohesionDistance)
                {
                    boidsInCohesionRange += 1;
                    _flockDirection += boid.transform.forward;
                    _flockCentre += boid.transform.position;
                    if (distance < separationDistance)
                    {
                        _separationDirection -= offset / distance;
                    }
                }
            }
            if (boidsInCohesionRange > 0) _flockCentre /= boidsInCohesionRange;
        }
        
        private void ApplyBoidSteering()
        {
            _acceleration = Vector3.zero;
            
            var alignmentForce = _flockDirection;
            var cohesionForce = (_flockCentre - transform.position) * cohesionStrength;
            var separationForce = _separationDirection * separationStrength;

            _acceleration += alignmentForce;
            _acceleration += cohesionForce;
            _acceleration += separationForce;
            rb.AddForce(_acceleration.normalized * 15f);
        }
    }
}