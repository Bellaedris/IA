using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Utils;

public class PathFinder
{
    public static List<Tile> GeneratePath(Tile start, Tile end, Dictionary<Tile, List<Tile>> neigh)
    {
        float startTime = Time.realtimeSinceStartup;

        Dictionary<Tile, Tile> predecessors = new Dictionary<Tile, Tile>();
        Dictionary<Tile, float> costToTile = new Dictionary<Tile, float>();
        var queue = new PriorityQueue<Tile, float>(); 
        queue.Enqueue(start, 0f);
        costToTile[start] = 0f;
        
        int visited = 0;
        while (queue.Count > 0)
        {
            visited++;
            Tile current = queue.Dequeue();
            if (current == end)
                break;
            
            foreach (Tile neighbor in neigh[current])
            {
                float cost = costToTile[current] + neighbor.GetWeight();
                if (!predecessors.ContainsKey(neighbor) || cost < costToTile[neighbor])
                {
                    costToTile[neighbor] = cost;
                    predecessors[neighbor] = current;
                    queue.Enqueue(neighbor, cost);
                }
            }
        }

        if (!predecessors.ContainsKey(end))
            return null;
        
        // the path is complete, we can trace it
        Tile cur = end;
        List<Tile> path = new List<Tile>();
        while (cur != start)
        {
            path.Add(cur);
            cur = predecessors[cur];
        }
        path.Add(start);

        float elapsed = Time.realtimeSinceStartup - startTime;
        Debug.Log("Path complete in " + elapsed + "s after visiting " + visited + " nodes.");
        
        return path;
    }
    
    public static List<Tile> GeneratePathAStar(Tile start, Tile end, Dictionary<Tile, List<Tile>> neigh)
    {
        float startTime = Time.realtimeSinceStartup;

        Dictionary<Tile, Tile> predecessors = new Dictionary<Tile, Tile>();
        Dictionary<Tile, float> costToTile = new Dictionary<Tile, float>();
        var queue = new PriorityQueue<Tile, float>(); 
        queue.Enqueue(start, 0f);
        costToTile[start] = 0f;
        
        int visited = 0;
        while (queue.Count > 0)
        {
            visited++;
            Tile current = queue.Dequeue();
            if (current == end)
                break;
            
            foreach (Tile neighbor in neigh[current])
            {
                float cost = costToTile[current] + neighbor.GetWeight();
                if (!predecessors.ContainsKey(neighbor) || cost < costToTile[neighbor])
                {
                    costToTile[neighbor] = cost;
                    predecessors[neighbor] = current;
                    queue.Enqueue(neighbor, cost);
                }
            }
        }

        if (!predecessors.ContainsKey(end))
            return null;
        
        // the path is complete, we can trace it
        Tile cur = end;
        List<Tile> path = new List<Tile>();
        while (cur != start)
        {
            path.Add(cur);
            cur = predecessors[cur];
        }
        path.Add(start);

        float elapsed = Time.realtimeSinceStartup - startTime;
        Debug.Log("Path complete in " + elapsed + "s after visiting " + visited + " nodes.");
        
        return path;
    }
}
