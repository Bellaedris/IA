using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TerrainManager : MonoBehaviour
{
    public GameObject terrainObject;

    [Header("Terrain")] 
    public float width = 5f;
    public float height = 5f;
    public float maxHeight = .1f;
    public int nx = 20;
    public int ny = 20;
    
    public int seed = 1337;
    public int octaves = 4;
    public float lacunarity = 2f;
    public float gain = .5f;
    public float scale = 1f;
    
    private Terrain terrainGenerator;

    void Awake()
    {
        GenerateTerrain();
        UpdateModel();
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit, Mathf.Infinity))
            {
                // Check if the hit object is your terrain
                if (hit.collider.gameObject.CompareTag("Terrain"))
                {
                    Vector3 hitPoint = hit.point;
                    terrainGenerator.setPlayerClick(hitPoint);
            
                    UpdateModel();
                }
            }
        }
    }

    public void GenerateTerrain()
    {
        terrainGenerator = new Terrain(width, height, nx, ny, seed, octaves, lacunarity, gain, scale, maxHeight);
    }

    public void UpdateModel()
    {
        terrainObject.GetComponent<MeshFilter>().sharedMesh = terrainGenerator.GenerateTerrain();
        terrainObject.GetComponent<MeshRenderer>().sharedMaterial.mainTexture = terrainGenerator.GenerateTexture();
    }
}
