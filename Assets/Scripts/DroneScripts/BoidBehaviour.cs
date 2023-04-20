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
            Simulate();
        }

        private void Simulate() 
        {
            boidIndex++;
            if (boidIndex >= gameManager.enemyList.Length) 
            {
                var cohesiveForce = (cohesionStrength / Vector3.Distance(cohesionPos,transform.position)) 
                                    * (cohesionPos - transform.position);
                rb.AddForce(cohesiveForce);
                boidIndex = 0;
                cohesionPos.Set(0f, 0f, 0f);
            }

            var pos = gameManager.enemyList[boidIndex].transform.position;
            var rot = gameManager.enemyList[boidIndex].transform.rotation;
            var dist = Vector3.Distance(transform.position, pos);
        
            if (dist > 0f) 
            {
                if (dist <= separationDistance) 
                {
                    var scale = separationStrength / dist;
                    rb.AddForce(scale * Vector3.Normalize(transform.position - pos));
                }
                else if (dist < cohesionDistance && dist > separationDistance) 
                {
                    cohesionPos += pos / gameManager.enemyList.Length;
                    transform.rotation = Quaternion.RotateTowards(transform.rotation, rot, 1f);
                }
            }
        }

    }
}