using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using Random = UnityEngine.Random;

public struct BoidGPU
{
    public Vector3 position, direction, velocity;
    public Vector3 cohesion, center, align, separation;
}

public struct Attractor
{
    public Vector4 positionIntensity; // packed position + intensity as float
}

public class GPUFlock : MonoBehaviour
{
    [Header(("Simulation"))]
    public GameObject attractor;
    public GameObject boidPrefab;
    public int numberOfBoids = 10;
    public float spawnRadius = 10f;
    public LayerMask obstacleMask;
    
    [Header("Boid Setting")]
    public float minSpeed = 1f;
    public float maxSpeed = 5f;
    public float accelerationModifyer = 10f;
    public float repulsionRadius = 1f;
    public float flockRadius = 5f;
    public float obstacleRadius = 2f;
    public float obstacleDist = 1f;
    
    [Header("Weights")]
    public float separationModifyer = 2f;
    public float cohesionModifyer = 4f;
    public float alignModifyer = 1f;
    public float centerModifyer = .1f;
    public float obstacleModifyer = 10f;

    [Header("Audio")] 
    public bool useReactiveAudio;
    public float bassSeparatorModifier;
    public GameObject bassAttractor;
    public GameObject bandAttractor;
    public GameObject highAttractor;
    public float BassSeparatorModifier
    {
        get => bassSeparatorModifier;
        set => bassSeparatorModifier = value;
    }
    
    [Header("Compute settings")]
    public ComputeShader computeShader;
    public int threadGroupSize = 64;
    
    private Attractor[] _attractors;
    [HideInInspector]
    public float bassAttractorStrength;

    public float BassAttractorStrength
    {
        get => bassAttractorStrength;
        set => bassAttractorStrength = value;
    }

    public float BandAttractorStrength
    {
        get => bandAttractorStrength;
        set => bandAttractorStrength = value;
    }

    public float HighAttractorStrength
    {
        get => highAttractorStrength;
        set => highAttractorStrength = value;
    }

    [HideInInspector]
    public float bandAttractorStrength;
    [HideInInspector]
    public float highAttractorStrength;
    
    private BoidGPU[] _boids;
    private GameObject[] _boidsObjects;

    private Vector3[] _avoidDirections;

    private int _kernelIndex;
    
    // Start is called before the first frame update
    void Start()
    {
        _kernelIndex = computeShader.FindKernel("CSMain");
        
        _boidsObjects = new GameObject[numberOfBoids];
        _boids = new BoidGPU[numberOfBoids];

        _attractors = new Attractor[3];
        _attractors[0].positionIntensity = new Vector4(bassAttractor.transform.position.x, bassAttractor.transform.position.y, bassAttractor.transform.position.z, 1);
        _attractors[1].positionIntensity = new Vector4(bandAttractor.transform.position.x, bandAttractor.transform.position.y, bandAttractor.transform.position.z, 1);
        _attractors[2].positionIntensity = new Vector4(highAttractor.transform.position.x, highAttractor.transform.position.y, highAttractor.transform.position.z, 1);

        for (int i = 0; i < numberOfBoids; i++)
        {
            var boid = new BoidGPU();
            boid.position = Random.insideUnitSphere * spawnRadius;
            boid.direction = Random.insideUnitSphere.normalized;
            _boids[i] = boid;
            
            _boidsObjects[i] = Instantiate(boidPrefab, boid.position, Quaternion.Euler(boid.direction));
        }
        
        GenerateRandomDirections(); 
    }

    // Update is called once per frame
    void Update()
    {
        // update attractor data
        _attractors[0].positionIntensity.w = bassAttractorStrength;
        _attractors[1].positionIntensity.w = bandAttractorStrength;
        _attractors[2].positionIntensity.w = highAttractorStrength;
        
        ComputeBuffer boidsBuffer = new ComputeBuffer(numberOfBoids, 84);
        boidsBuffer.SetData(_boids);
        ComputeBuffer attractorBuffer = new ComputeBuffer(3, 16);
        attractorBuffer.SetData(_attractors);

        float separation = separationModifyer;
        if (useReactiveAudio)
            separation *= bassSeparatorModifier;
        
        computeShader.SetBuffer(_kernelIndex, "boids", boidsBuffer);
        computeShader.SetBuffer(_kernelIndex, "attractors", attractorBuffer);
        computeShader.SetInt("boidCount", boidsBuffer.count);
        computeShader.SetInt("attractorCount", attractorBuffer.count);
        computeShader.SetFloat("separationModifyer", separation);
        computeShader.SetFloat("cohesionModifyer", this.cohesionModifyer);
        computeShader.SetFloat("alignModifyer", this.alignModifyer);
        computeShader.SetFloat("centerModifyer", this.centerModifyer);
        computeShader.SetFloat("deltaTime", Time.deltaTime);
        computeShader.SetFloat("speed", this.maxSpeed);
        computeShader.SetFloat("repulsionRadius", this.repulsionRadius);
        computeShader.SetFloat("flockRadius", this.flockRadius);
        
        computeShader.Dispatch(_kernelIndex, Mathf.CeilToInt(_boids.Length / (float)threadGroupSize), 1, 1);
        boidsBuffer.GetData(_boids);
        boidsBuffer.Release();
        attractorBuffer.Release();

        Vector3 avoid = Vector3.zero;
        for (int i = 0; i < _boids.Length; i++)
        {
            var b = _boids[i];
            
            //check obstacle
            RaycastHit hitInfo;
            if (Physics.SphereCast(_boids[i].position, obstacleRadius, _boids[i].direction, out hitInfo, obstacleDist, obstacleMask))
            {
                // sample a direction until we find a direction that does not collide
                foreach (var dir in _avoidDirections)
                {
                    Ray ray = new Ray(_boids[i].position, _boidsObjects[i].transform.TransformDirection(dir));
                    if (!Physics.SphereCast(ray, obstacleRadius, obstacleDist, obstacleMask))
                    {
                        Debug.DrawRay(ray.origin, ray.direction * obstacleDist, Color.yellow);
                        avoid = dir * obstacleModifyer;
                        break;
                    }
                }
            }
            
            Vector3 direction = (b.cohesion + b.align + b.separation + b.center + avoid).normalized;
            Vector3 acceleration = (direction - b.velocity.normalized) * accelerationModifyer;
            b.velocity += acceleration * Time.deltaTime;
            
            float speed = Mathf.Clamp(b.velocity.magnitude, minSpeed, maxSpeed);
            b.velocity = b.velocity.normalized * speed;
            
            //float ip = Mathf.Exp(-1f * Time.deltaTime);
            b.position += b.velocity * Time.deltaTime;
            b.direction = b.velocity.normalized;
            _boids[i] = b;
            
            _boidsObjects[i].transform.position = _boids[i].position;
            _boidsObjects[i].transform.rotation = Quaternion.LookRotation(_boids[i].direction);
        }
    }

    void GenerateRandomDirections()
    {
        int numberOfDirections = 300;
        _avoidDirections = new Vector3[numberOfDirections];
        for (int i = 0; i < numberOfDirections; i++)
        {
            float index = i + .5f;
            float phi = Mathf.Acos(1f - 2f * index / numberOfDirections);
            float theta = Mathf.PI * 1f + Mathf.Pow(5f, .5f) * i;
            _avoidDirections[i] = new Vector3(Mathf.Cos(theta) * Mathf.Sin(phi), Mathf.Sin(theta) * Mathf.Sin(phi),
                Mathf.Cos(phi)).normalized;
        }
    }

    private void OnDrawGizmos()
    {
        // for(int i = 0; i < _boids.Length; i++)
        // {
        //     //RaycastHit hitInfo;
        //     //if (Physics.SphereCast(_boids[i].position, obstacleRadius, _boids[i].direction, out hitInfo, obstacleDist,
        //      //       obstacleMask))
        //         Gizmos.DrawRay(_boids[i].position, _boids[i].position + _boids[i].direction * obstacleDist);
        //         Gizmos.DrawSphere(_boids[i].position + _boids[i].direction * obstacleDist, obstacleRadius);
        // }
    }
}
