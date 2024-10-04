using System.Collections.Generic;
using UnityEngine;

public class Terrain
{
    private float[] hm;

    private int nx, ny;
    private float width, height;
    private float maxHeight;
    
    private FastNoiseLite noise;
    private Mesh mesh;
    
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
        
        GenerateTerrain();
    }

    float GetHeight(float x, float y)
    {
        return hm[GetIndex(x, y)];
    }

    int GetIndex(float x, float y)
    {
        return (int)y * nx + (int)x;
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
        // compute the hit point in the map
        mesh.colors[WorldToIndex(hitPoint)] = Color.red;
    }
    
    public void GenerateTerrain()
    {
        mesh = new Mesh();
        
        var vertices = new List<Vector3>();
        var triangles = new List<int>();
        var colors = new List<Color>();
        
        float stepX = width / (float)(nx - 1);
        float stepY = height / (float)(ny - 1);
        
        for(int j = 0; j < ny; j++)
            for (int i = 0; i < nx; i++)
            {
                vertices.Add(new Vector3((float)i * stepX, noise.GetNoise(i, j) * maxHeight, (float)j * stepY));
                colors.Add(Color.white);
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
        
        mesh.SetVertices(vertices.ToArray());
        mesh.SetColors(colors.ToArray());
        mesh.SetTriangles(triangles.ToArray(), 0);
        mesh.RecalculateNormals();
    }

    public ref Mesh getMesh()
    {
        return ref mesh;
    }
}