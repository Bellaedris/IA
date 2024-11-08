using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Boid : MonoBehaviour
{
    public float speed = 5f;
    public float rotationSpeed = 1f;
    public float closeRadius = 1f;

    public float separationModifyer = 5f;
    public float cohesionModifyer = 1f;
    public float alignModifyer = 1f;
    public float centerModifyer = .1f;
    
    public int threadGroupSize = 64;
    
    public BoidManager boidManager;

    private Vector3 _direction;
    private float _maxVelocity;

    Vector3 GetAwayVector(Transform other)
    {
        var res = Vector3.zero;
        // if the distance to another boid is inferior to a threshold, move in the direction opposite to the mean direction
        float dist = Vector3.Distance(transform.position, other.position);
        if (dist < closeRadius)
            res += transform.position - other.position;

        return res;
    }

    // Update is called once per frame
    void Update()
    {
        var currentRotation = transform.rotation;

        Vector3 cohesion = Vector3.zero; // move towards the center of the boid
        Vector3 align = Vector3.zero; // match the velocity of the boids
        Vector3 separation = Vector3.zero; // stay away from other boids
        Vector3 center = (boidManager.bounds.bounds.center - transform.position) * centerModifyer; // stay wear the middle of the bounds
        
        var nearby = Physics.OverlapSphere(transform.position, 5f, 1 << gameObject.layer);

        foreach (var boid in nearby)
        {
            if (boid.gameObject == gameObject)
                continue;
            
            cohesion += boid.transform.position;
            align += boid.transform.forward;
            separation += GetAwayVector(boid.transform);
        }

        float mean = 1f / nearby.Length;
        cohesion *= mean;
        align *= mean * alignModifyer;
        separation *= separationModifyer;
        
        cohesion = (cohesion - transform.position).normalized * cohesionModifyer;

        _direction = (cohesion + align + separation + center).normalized;
        
        // if (!boidManager.bounds.bounds.Contains(transform.position))
        //     _direction = -_direction;
        
        // turn the boid to the direction 
        var rotation = Quaternion.FromToRotation(Vector3.forward, _direction.normalized);

        // Applys the rotation with interpolation.
        if (rotation != currentRotation)
        {
            var ip = Mathf.Exp(-rotationSpeed * Time.deltaTime);
            transform.rotation = Quaternion.Lerp(rotation, currentRotation, ip);
        }
        
        transform.position += (Time.deltaTime * speed * transform.forward);
    }

    private void OnTriggerExit(Collider other)
    {
        // if (other.CompareTag("bounds"))
        //     _direction *= -1;
    }
}
