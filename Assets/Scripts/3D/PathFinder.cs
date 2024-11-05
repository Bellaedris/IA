using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Utils;

public class PathFinder
{
    public static List<Tile> GeneratePath(Tile start, Tile end, Dictionary<Tile, List<Tile>> neigh, List<Tile> occupied, bool useAStar)
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
                if (neighbor.type == Tile.TileType.Wall || occupied.Contains(neighbor))
                    continue;
                
                float cost = costToTile[current] + neighbor.GetWeight();
                if (!predecessors.ContainsKey(neighbor) || cost < costToTile[neighbor])
                {
                    costToTile[neighbor] = cost;
                    predecessors[neighbor] = current;
                    float queueCost = cost;
                    if (useAStar)
                        queueCost += Vector3.Distance(current.transform.position, end.transform.position);
                    queue.Enqueue(neighbor, queueCost);
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
        //Debug.Log("Path complete (Dijkstra) in " + elapsed + "s after visiting " + visited + " nodes.");
        
        return path;
    }
    
    // public static List<Tile> GeneratePathAStar(Tile start, Tile end, Dictionary<Tile, List<Tile>> neigh, List<Tile> occupied)
    // {
    //     float startTime = Time.realtimeSinceStartup;
    //
    //     var predecessors = new Dictionary<Tile, Tile>();
    //     var costToTile = new Dictionary<Tile, float>();
    //     var queue = new PriorityQueue<Tile, float>(); 
    //     queue.Enqueue(start, 0f);
    //     costToTile[start] = 0f;
    //     
    //     int visited = 0;
    //     while (queue.Count > 0)
    //     {
    //         visited++;
    //         Tile current = queue.Dequeue();
    //         if (current == end)
    //             break;
    //         
    //         foreach (Tile neighbor in neigh[current])
    //         {
    //             if (neighbor.type == Tile.TileType.Wall || occupied.Contains(neighbor))
    //                 continue;
    //             
    //             float cost = costToTile[current] + neighbor.GetWeight();
    //             if (!predecessors.ContainsKey(neighbor) || cost < costToTile[neighbor])
    //             {
    //                 costToTile[neighbor] = cost;
    //                 predecessors[neighbor] = current;
    //                 // we add a heuristic to our priority queue, here the distance to the goal. It probably should be weighted to suit our needs
    //                 // we also only add tiles that are reacheable: not occupied and not a wall
    //                 queue.Enqueue(neighbor, cost );
    //             }
    //         }
    //     }
    //
    //     if (!predecessors.ContainsKey(end))
    //         return null;
    //     
    //     // the path is complete, we can trace it
    //     Tile cur = end;
    //     List<Tile> path = new List<Tile>();
    //     while (cur != start)
    //     {
    //         path.Add(cur);
    //         cur = predecessors[cur];
    //     }
    //     path.Add(start);
    //
    //     float elapsed = Time.realtimeSinceStartup - startTime;
    //     //Debug.Log("Path complete (A*) in " + elapsed + "s after visiting " + visited + " nodes.");
    //     
    //     return path;
    // }
}
