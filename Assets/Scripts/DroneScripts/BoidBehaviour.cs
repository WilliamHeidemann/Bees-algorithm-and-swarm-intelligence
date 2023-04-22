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
        private Vector3 cohesionPos = new(0f, 0f, 0f);
        private int boidIndex = 0;

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

        private Vector3 flockDirection;
        private Vector3 flockCentre;
        private Vector3 separationDirection;
        private int boidsInCohesionRange;

        private Vector3 acceleration;
        
        private void CalculateBoidValues()
        {
            flockDirection = Vector3.zero;
            flockCentre = Vector3.zero;
            separationDirection = Vector3.zero;
            boidsInCohesionRange = 0;

            var boids = gameManager.enemyList;
            foreach (var boid in boids)
            {
                var offset = boid.transform.position - transform.position;
                var distance = Vector3.Magnitude(offset);
                if (distance == 0) continue;
                if (distance < cohesionDistance)
                {
                    boidsInCohesionRange += 1;
                    flockDirection += boid.transform.forward;
                    flockCentre += boid.transform.position;
                    if (distance < separationDistance)
                    {
                        separationDirection -= offset / distance;
                    }
                }
            }
            flockCentre /= boidsInCohesionRange;
        }
        
        private void ApplyBoidSteering()
        {
            acceleration = Vector3.zero;
            
            var alignmentForce = flockDirection;
            var cohesionForce = flockCentre - transform.position;
            var separationForce = separationDirection * 50;
            print(separationForce);
            acceleration += alignmentForce;
            acceleration += cohesionForce;
            acceleration += separationForce;

            rb.AddForce(acceleration);
            transform.forward = rb.velocity;
        }
    }
}