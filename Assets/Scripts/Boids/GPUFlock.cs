using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;

public struct BoidGPU
{
    public Vector3 position, direction, velocity;
    public Vector3 cohesion, center, align, separation;
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
    
    [Header("Compute settings")]
    public ComputeShader computeShader;
    public int threadGroupSize = 64;
    
    [FormerlySerializedAs("bassAttractorStrength")] [HideInInspector]
    public float bassStrength;
    [FormerlySerializedAs("bandAttractorStrength")] [HideInInspector]
    public float bandStrength;
    [FormerlySerializedAs("highAttractorStrength")] [HideInInspector]
    public float highStrength;

    public float BassStrength
    {
        get => bassStrength;
        set => bassStrength = value;
    }

    public float BandStrength
    {
        get => bandStrength;
        set => bandStrength = value;
    }

    public float HighStrength
    {
        get => highStrength;
        set => highStrength = value;
    }
    
    private BoidGPU[] _boids;
    private BoidsGPU[] _boidsObjects;

    public static Vector3[] avoidDirections;

    private int _kernelIndex;
    
    // Start is called before the first frame update
    void Start()
    {
        _kernelIndex = computeShader.FindKernel("CSMain");
        
        _boidsObjects = new BoidsGPU[numberOfBoids];
        _boids = new BoidGPU[numberOfBoids];
        
        for (int i = 0; i < numberOfBoids; i++)
        {
            var boid = new BoidGPU();
            boid.position = Random.insideUnitSphere * spawnRadius;
            boid.direction = Random.insideUnitSphere.normalized;
            _boids[i] = boid;
            
            _boidsObjects[i] = Instantiate(boidPrefab, boid.position, Quaternion.Euler(boid.direction)).GetComponent<BoidsGPU>();
        }
        
        GenerateRandomDirections(); 
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            separationModifyer = Random.Range(0f, 10f);
            cohesionModifyer = Random.Range(0f, 10f);
            alignModifyer = Random.Range(0f, 10f);
            centerModifyer = Random.Range(0f, 1f);
        }
        
        ComputeBuffer boidsBuffer = new ComputeBuffer(numberOfBoids, 84);
        boidsBuffer.SetData(_boids);

        float separation = separationModifyer;
        float align = alignModifyer;
        if (useReactiveAudio)
        {
            align *= bandStrength;
            separation *= bassStrength;
        }

        computeShader.SetBuffer(_kernelIndex, "boids", boidsBuffer);
        computeShader.SetInt("boidCount", boidsBuffer.count);
        computeShader.SetFloat("separationModifyer", separation);
        computeShader.SetFloat("cohesionModifyer", this.cohesionModifyer);
        computeShader.SetFloat("alignModifyer", align);
        computeShader.SetFloat("centerModifyer", this.centerModifyer);
        computeShader.SetFloat("deltaTime", Time.deltaTime);
        computeShader.SetFloat("speed", this.maxSpeed);
        computeShader.SetFloat("repulsionRadius", this.repulsionRadius);
        computeShader.SetFloat("flockRadius", this.flockRadius);
        computeShader.SetVector("attractor", this.attractor.transform.position);
        
        computeShader.Dispatch(_kernelIndex, Mathf.CeilToInt(_boids.Length / (float)threadGroupSize), 1, 1);
        boidsBuffer.GetData(_boids);
        boidsBuffer.Release();

        Vector3 avoid = Vector3.zero;
        for (int i = 0; i < _boids.Length; i++)
        {
            var b = _boids[i];
            
            //check obstacle
            RaycastHit hitInfo;
            if (Physics.SphereCast(_boids[i].position, obstacleRadius, _boids[i].direction, out hitInfo, obstacleDist, obstacleMask))
            {
                // sample a direction until we find a direction that does not collide
                foreach (var dir in avoidDirections)
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
            switch (_boidsObjects[i].boidGroup)
            {
                case BoidGroup.Bass:
                    _boidsObjects[i].velocity = bassStrength;
                    break;
                case BoidGroup.Mid:
                    _boidsObjects[i].velocity = bandStrength;
                    break;
                case BoidGroup.High:
                    _boidsObjects[i].velocity = highStrength;
                    break;
            }
            
        }
    }

    void GenerateRandomDirections()
    {
        int numberOfDirections = 300;
        avoidDirections = new Vector3[numberOfDirections];
        for (int i = 0; i < numberOfDirections; i++)
        {
            float index = i + .5f;
            float phi = Mathf.Acos(1f - 2f * index / numberOfDirections);
            float theta = Mathf.PI * 1f + Mathf.Pow(5f, .5f) * i;
            avoidDirections[i] = new Vector3(Mathf.Cos(theta) * Mathf.Sin(phi), Mathf.Sin(theta) * Mathf.Sin(phi),
                Mathf.Cos(phi)).normalized;
        }
    }
    
    // Vector3 SteerTowards (Vector3 vector) {
    //     Vector3 v = vector.normalized * maxSpeed - velocity;
    //     return Vector3.ClampMagnitude (v, settings.maxSteerForce);
    // }

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
