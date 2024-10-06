using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GraphNode
{
    float cost;
    int previous;
    List<int> adjacencies;
    public GraphNode(float cost, int previous, List<int> adjacencies)
    {
        this.cost = cost;
        this.previous = previous;
        this.adjacencies = adjacencies;
    }
}

public class Graph
{
    GraphNode[] nodes;
}

public class RoadsGenerator
{
    
}
