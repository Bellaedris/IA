using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using Random = UnityEngine.Random;

public struct BoidGPU
{
    public Vector3 position, direction;
    public Vector3 cohesion, center, align, separation;
}

public class GPUFlock : MonoBehaviour
{
    public int directionNumber = 200;
    public GameObject attractor;
    public GameObject boidPrefab;
    public int numberOfBoids = 10;
    public float spawnRadius = 10f;
    public BoxCollider bounds;
    public LayerMask obstacleMask;
    
    public float speed = 5f;
    public float rotationSpeed = 1f;
    public float repulsionRadius = 1f;
    public float flockRadius = 5f;
    public float obstacleRadius = 2f;
    public float obstacleDist = 1f;
    
    public float separationModifyer = 2f;
    public float cohesionModifyer = 4f;
    public float alignModifyer = 1f;
    public float centerModifyer = .1f;
    public float obstacleModifyer = 10f;
    
    public int threadGroupSize = 64;
    
    public ComputeShader computeShader;

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
        ComputeBuffer boidsBuffer = new ComputeBuffer(numberOfBoids, 72);
        boidsBuffer.SetData(_boids);
        
        computeShader.SetBuffer(_kernelIndex, "boids", boidsBuffer);
        computeShader.SetInt("boidCount", boidsBuffer.count);
        computeShader.SetFloat("separationModifyer", this.separationModifyer);
        computeShader.SetFloat("cohesionModifyer", this.cohesionModifyer);
        computeShader.SetFloat("alignModifyer", this.alignModifyer);
        computeShader.SetFloat("centerModifyer", this.centerModifyer);
        computeShader.SetFloat("deltaTime", Time.deltaTime);
        computeShader.SetFloat("speed", this.speed);
        computeShader.SetFloat("rotationSpeed", this.rotationSpeed);
        computeShader.SetFloat("repulsionRadius", this.repulsionRadius);
        computeShader.SetFloat("flockRadius", this.flockRadius);
        computeShader.SetVector("attractor", attractor.transform.position);
        
        computeShader.Dispatch(_kernelIndex, Mathf.CeilToInt(_boids.Length / threadGroupSize), 1, 1);
        boidsBuffer.GetData(_boids);
        boidsBuffer.Release();

        for (int i = 0; i < _boids.Length; i++)
        {
            var b = _boids[i];
            
            Vector3 obstacleAvoidance = Vector3.zero;
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
                        obstacleAvoidance = dir * obstacleModifyer;
                        break;
                    }
                }
            }
            
            Vector3 direction = (b.cohesion + b.align + b.separation + b.center + obstacleAvoidance).normalized;
            
            float ip = Mathf.Exp(-1f * Time.deltaTime);
            b.direction = Vector3.Lerp(direction, b.direction, ip);
            b.position += Time.deltaTime * speed * b.direction;
            _boids[i] = b;
            
            _boidsObjects[i].transform.position = _boids[i].position;
            _boidsObjects[i].transform.rotation = Quaternion.LookRotation(_boids[i].direction);
        }
    }

    void GenerateRandomDirections()
    {
        int numberOfDirections = 500;
        _avoidDirections = new Vector3[numberOfDirections];
        for (int i = 0; i < numberOfDirections; i++)
        {
            float index = i + .5f;
            float phi = Mathf.Acos(1f - 2f * index / numberOfDirections);
            float theta = Mathf.PI * 1f + Mathf.Pow(5f, .5f) * i;
            _avoidDirections[i] = new Vector3(Mathf.Cos(theta) * Mathf.Sin(phi), Mathf.Sin(theta) * Mathf.Sin(phi),
                Mathf.Cos(phi));
        }
    }

    private void OnDrawGizmos()
    {
        // for (int i = 0; i < directionNumber; i++)
        // {
        //     Gizmos.DrawSphere(_avoidDirections[i]
        //         , .01f);
        // }
    }
}
