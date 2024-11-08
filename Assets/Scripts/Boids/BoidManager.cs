using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoidManager : MonoBehaviour
{
    public GameObject boidPrefab;
    public int numberOfBoids = 10;
    public float spawnRadius = 10f;
    public BoxCollider bounds;
    
    // Start is called before the first frame update
    void Start()
    {
        for(int i = 0; i < numberOfBoids; i++)
            SpawnBoid();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void SpawnBoid()
    {
        GameObject boid = Instantiate(boidPrefab, Random.insideUnitSphere * spawnRadius, Quaternion.identity);
        boid.GetComponent<Boid>().boidManager = this;
    }
}
