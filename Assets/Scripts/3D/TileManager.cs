using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TileManager : MonoBehaviour
{
    public GameObject tilePrefab;
    public Transform parent;

    public int mapSize = 10;
    
    private Tile[,] _tiles;
    private Dictionary<Tile, List<Tile>> _neighbours;

    public Dictionary<Tile, List<Tile>> Neighbours => _neighbours;

    private void Start()
    {
        // read all tiles and add fill the datas
        _tiles = new Tile[mapSize, mapSize];
        _neighbours = new Dictionary<Tile, List<Tile>>();

        var tiles = FindObjectsByType<Tile>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);
        foreach (var tile in tiles)
        {
            int x = tile.x;
            int y = tile.y;
            
            _tiles[x, y] = tile;
        }

        // add all 8-neighbors
        foreach (var tile in tiles)
        {
            int x = tile.x;
            int y = tile.y;
            List<Tile> neighbours = new List<Tile>();

            for (int dx = -1; dx <= 1; dx++)
            {
                for (int dy = -1; dy <= 1; dy++)
                {
                    if (dx == 0 && dy == 0)
                        continue;

                    int nx = x + dx;
                    int ny = y + dy;

                    if (nx >= 0 && nx < mapSize && ny >= 0 && ny < mapSize)
                    {
                        neighbours.Add(_tiles[nx, ny]);
                    }
                }
            }

            _neighbours[_tiles[x, y]] = neighbours;
        }
    }

    public void GenerateMap()
    {
        _tiles = new Tile[mapSize, mapSize];
        _neighbours = new Dictionary<Tile, List<Tile>>();
        
        for(int x = 0; x < mapSize; x++)
        for (int y = 0; y < mapSize; y++)
        {
            _tiles[x, y] = Instantiate(tilePrefab, new Vector3(x, 0, y), Quaternion.identity).GetComponent<Tile>();
            _tiles[x, y].x = x;
            _tiles[x, y].y = y;
            _tiles[x, y].transform.SetParent(parent);
        }
        
        for(int x = 0; x < mapSize; x++)
        for (int y = 0; y < mapSize; y++)
        {
            List<Tile> neighbours = new List<Tile>();
            if (x > 0)
                neighbours.Add(_tiles[x - 1, y]);
            if (x < mapSize - 1)
                neighbours.Add(_tiles[x + 1, y]);
            if (y > 0)
                neighbours.Add(_tiles[x, y - 1]);
            if (y < mapSize - 1)
                neighbours.Add(_tiles[x, y + 1]);
            _neighbours[_tiles[x, y]] = neighbours;
        }
    }
}
