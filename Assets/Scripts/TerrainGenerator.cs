using System.Collections.Generic;
using UnityEngine;

public class Terrain
{
    private float[] hm;
    private List<int> playerClicks;

    private int nx, ny;
    private float width, height;
    private float maxHeight;
    
    private FastNoiseLite noise;
    
    public Terrain(float width, float height, int nx, int ny, int seed, int octaves, float lacunarity,
        float gain, float scale, float maxHeight)
    {
        hm = new float[nx * ny];
        this.nx = nx;
        this.ny = ny;
        this.width = width;
        this.height = height;
        this.maxHeight = maxHeight;
        
        noise = new FastNoiseLite();
        noise.SetNoiseType(FastNoiseLite.NoiseType.OpenSimplex2);
        noise.SetFractalType(FastNoiseLite.FractalType.FBm);
        noise.SetSeed(seed);
        noise.SetFractalOctaves(octaves);
        noise.SetFractalLacunarity(lacunarity);
        noise.SetFractalGain(gain);
        noise.SetFrequency(0.01f * scale);

        playerClicks = new List<int>();
    }

    float GetHeight(float x, float y)
    {
        return hm[GetIndex(x, y)];
    }

    int GetIndex(float x, float y)
    {
        return (int)y * nx + (int)x;
    }

    Vector3 PointInWorld(int index)
    {
        float x = index % nx;
        float z = index / ny;
        return new Vector3(x, hm[index], z);
    }

    int WorldToIndex(Vector3 pos)
    {
        return GetIndex(pos.x * (float)nx, pos.z * (float)ny);
    }

    // float GetColor(int x, int y)
    // {
    //     
    // }

    public void setPlayerClick(Vector3 hitPoint)
    {
        // store the hit point's index in the map
        Debug.Log(WorldToIndex(hitPoint));
        playerClicks.Add(WorldToIndex(hitPoint));
    }
    
    public Mesh GenerateTerrain()
    {
        Debug.Log("generating terrain");
        Mesh mesh = new Mesh();
        
        var vertices = new Vector3[nx * ny];
        var uvs = new Vector2[nx * ny];
        var triangles = new List<int>();
        
        float stepX = width / (float)(nx - 1);
        float stepY = height / (float)(ny - 1);
        
        for(int j = 0; j < ny; j++)
            for (int i = 0; i < nx; i++)
            {
                vertices[j * nx + i] = new Vector3((float)i * stepX, noise.GetNoise(i, j) * maxHeight, (float)j * stepY);
                uvs[j * nx + i] = new Vector2(i * stepX, j * stepY);
            }

        for(int j = 0; j < nx - 1; j++)
            for (int i = 0; i < ny - 1; i++)
            {
                int index = (j * nx) + i;
                triangles.Add(index);
                triangles.Add(index + nx);
                triangles.Add(index + nx + 1);
               
                triangles.Add(index);
                triangles.Add(index  + nx + 1);
                triangles.Add(index + 1);
            }
        
        mesh.SetVertices(vertices);
        mesh.uv = uvs;
        mesh.SetTriangles(triangles.ToArray(), 0);
        mesh.RecalculateNormals();

        return mesh;
    }

    public Texture2D GenerateTexture()
    {
        return TextureGenerator.GenerateTexture(hm, nx, ny, playerClicks);
    }

    public float GetWeight(int source, int dest)
    {
        return Vector3.Distance(PointInWorld(source), PointInWorld(dest));
    }

    public void GenerateRoad(int source, int dest)
    {
        // initialize graph
        GraphNode[] graph = new GraphNode[nx * ny];
        for(int j = 0; j < ny; j++)
            for (int i = 0; i < nx; i++)
            {
                List<int> neighbors = new List<int>();
                graph[j * nx + i] = new GraphNode(Mathf.Infinity, 0, neighbors);
            }
    }
}