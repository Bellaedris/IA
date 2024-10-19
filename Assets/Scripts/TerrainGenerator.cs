using System;
using System.Collections.Generic;
using UnityEngine;

public class Terrain
{
    // terrain datas
    private float[] hm;
    private float[] slope;
    private float maxSlope;
    
    private List<int> playerClicks;
    private List<List<int>> paths;

    private int nx, ny;
    private float width, height;
    private float maxHeight;
    
    private FastNoiseLite noise;
    
    public Terrain(float width, float height, int nx, int ny, int seed, int octaves, float lacunarity,
        float gain, float scale, float maxHeight)
    {
        hm = new float[nx * ny];
        slope = new float[nx * ny];
        maxSlope = 0;
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
        paths = new List<List<int>>();

        for(int j = 0; j < ny; j++)
        for (int i = 0; i < nx; i++)
        {
            hm[j * nx + i] = noise.GetNoise(i, j);
        }

        ComputeSlope();
    }

    float GetHeight(int x, int y)
    {
        return hm[GetIndex(x, y)];
    }

    Vector2 GetGradient(int x, int y)
    {
        float gradX = (hm[GetIndex(x + 1, y)] - hm[GetIndex(x - 1, y)]) / (2 * width / (float)nx);
        float gradY = (hm[GetIndex(x, y + 1)] - hm[GetIndex(x, y - 1)]) / (2 * height / (float)ny);
        
        return new Vector2(gradX, gradY);
    }

    float GetSlope(int x, int y)
    {
        return GetGradient(x, y).magnitude;
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

    public void setPlayerClick(Vector3 hitPoint)
    {
        // store the hit point's index in the map
        playerClicks.Add(WorldToIndex(hitPoint));
        if(playerClicks.Count == 2)
        {
            GenerateRoad(playerClicks[0], playerClicks[1]);
            GenerateRoadAStar(playerClicks[0], playerClicks[1]);
            playerClicks.Clear();
        }
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
                vertices[j * nx + i] = new Vector3((float)i * stepX, 0, (float)j * stepY);
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
        return TextureGenerator.GenerateTexture(ref hm, ref slope, maxSlope, nx, ny, playerClicks, paths);
    }

    public float GetWeightGlobal(int source, int neigh, int dest)
    {
        // weight depends on the distance and the height (water has a higher weight)
        float distToDest = Vector3.Distance(PointInWorld(source), PointInWorld(dest));
        
        float weight = Vector3.Distance(PointInWorld(source), PointInWorld(neigh)) + slope[neigh];
        return hm[dest] < -.5f ? Mathf.Infinity : weight + distToDest;
    }
    
    public float GetWeight(int source, int dest)
    {
        // weight depends on the distance, the height difference and the slope (water has a higher weight)
        float weight = Vector3.Distance(PointInWorld(source), PointInWorld(dest)) + slope[dest];
        return hm[dest] < -.5f ? Mathf.Infinity : weight;
    }

    public void GenerateRoad(int source, int dest)
    {
        // initialize graph
        GraphNode[] graph = new GraphNode[nx * ny];
        for(int j = 1; j < ny - 1; j++)
            for (int i = 1; i < nx - 1; i++)
            {
                int index = (j * nx + i);
                List<int> neighbors = new List<int>();
                for(int x = -1; x <= 1; x++)
                    for(int y = -1; y <= 1; y++)
                        neighbors.Add(index + x + (y * nx));
                graph[index] = new GraphNode(Mathf.Infinity, 0, neighbors, index);
            }
        graph[source].cost = 0;

        float start = Time.realtimeSinceStartup;
        
        // dijkstra: we setup a list of graph to process
        var queue = new Heap<GraphNode>((a, b) => a.cost.CompareTo(b.cost));
        queue.Push(graph[source]);
        int visited = 0;
        while (queue.Count > 0)
        {
            GraphNode current = queue.Pop();
            visited++;
            if (current.visited)
                continue;

            if (current.index == dest)
                break;
            
            foreach (int neighbor in current.adjacencies)
            {
                GraphNode node = graph[neighbor];
                // we're on the borders, ignore
                if (node == null)
                    continue;
                
                float cost = current.cost + GetWeight(current.index, neighbor);
                if (cost < node.cost)
                {
                    node.cost = cost;
                    node.previous = current.index;
                    queue.Push(graph[neighbor]);
                }
            }
            current.visited = true;
        }
        
        // the path is complete, we can trace it
        GraphNode cur = graph[dest];
        List<int> path = new List<int>();
        while (cur.index != source)
        {
            path.Add(cur.index);
            cur = graph[cur.previous];
        }
        paths.Add(path);

        float elapsed = Time.realtimeSinceStartup - start;
        Debug.Log("Path complete in " + elapsed + "s after visiting " + visited + " nodes.");
    }
    
    public void GenerateRoadAStar(int source, int dest)
    {
        // initialize graph
        GraphNode[] graph = new GraphNode[nx * ny];
        for(int j = 1; j < ny - 1; j++)
            for (int i = 1; i < nx - 1; i++)
            {
                int index = (j * nx + i);
                List<int> neighbors = new List<int>();
                for(int x = -1; x <= 1; x++)
                    for(int y = -1; y <= 1; y++)
                        neighbors.Add(index + x + (y * nx));
                graph[index] = new GraphNode(Mathf.Infinity, 0, neighbors, index);
            }
        graph[source].cost = 0;

        float start = Time.realtimeSinceStartup;
        
        // dijkstra: we setup a list of graph to process
        var queue = new Heap<GraphNode>((a, b) => a.cost.CompareTo(b.cost));
        queue.Push(graph[source]);
        int visited = 0;
        while (queue.Count > 0)
        {
            GraphNode current = queue.Pop();
            visited++;
            if (current.visited)
                continue;
            
            if (current.index == dest)
                break;
            
            foreach (int neighbor in current.adjacencies)
            {
                GraphNode node = graph[neighbor];
                // we're on the borders, ignore
                if (node == null)
                    continue;
                
                float cost = current.cost + GetWeightGlobal(current.index, neighbor, dest);
                if (cost < node.cost)
                {
                    node.cost = cost;
                    node.previous = current.index;
                    queue.Push(graph[neighbor]);
                }
            }
            current.visited = true;
        }
        
        // the path is complete, we can trace it
        GraphNode cur = graph[dest];
        List<int> path = new List<int>();
        while (cur.index != source)
        {
            path.Add(cur.index);
            cur = graph[cur.previous];
        }
        paths.Add(path);

        float elapsed = Time.realtimeSinceStartup - start;
        Debug.Log("Path complete in " + elapsed + "s after visiting " + visited + " nodes.");
    }

    private void ComputeSlope()
    {
        float curSlope;
        for(int j = 1; j < ny - 1; j++)
        for (int i = 1; i < nx - 1; i++)
        {
            curSlope = GetSlope(i, j);
            slope[GetIndex(i, j)] = GetSlope(i, j);
            if (curSlope > maxSlope)
                maxSlope = curSlope;
        }
    }
}