using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public enum BoidGroup
{
    Bass,
    Mid,
    High
}

public class BoidsGPU : MonoBehaviour
{
    public float velocity;   
    public BoidGroup boidGroup;
    private MeshRenderer _meshRenderer;
    private MaterialPropertyBlock _propBlock;
    
    // Start is called before the first frame update
    void Start()
    {
        boidGroup = (BoidGroup) Enum.ToObject(typeof(BoidGroup) , Random.Range(0, 3));
        _meshRenderer = GetComponentInChildren<MeshRenderer>();
        _propBlock = new MaterialPropertyBlock();
    }

    // Update is called once per frame
    void Update()
    {
        _propBlock.SetFloat("_Velocity", velocity);
        _meshRenderer.SetPropertyBlock(_propBlock);
    }
}
