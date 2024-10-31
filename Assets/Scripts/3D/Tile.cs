using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tile : MonoBehaviour
{
    public enum TileType
    {
        Empty,
        Laser,
        Hangar,
        Wall
    }
    
    public Material emptyMaterial;
    public Material laserMaterial;
    public GameObject wall;
    public GameObject hangar;

    public TileType type;

    public int _x, _y;
    
    
    public bool _occupied;
    
    public int y
    {
        get => _x;
        set => _x = value;
    }

    public int x
    {
        get => _y;
        set => _y = value;
    }

    private void Start()
    {
        _occupied = false;
    }

    public float GetWeight()
    {
        switch (type)
        {
            default:
                return 1f;
            case TileType.Laser:
                return 10f;
            case TileType.Wall:
                return Mathf.Infinity;
            case TileType.Hangar:
                return 3f;
        }
    }

    public void UpdateType()
    {
        if (type == TileType.Wall)
        {
            wall.SetActive(true);
            return;
        }

        if (type == TileType.Hangar)
        {
            hangar.SetActive(true);
            return;
        }

        wall.SetActive(false);
        hangar.SetActive(false);
        var renderer = GetComponent<MeshRenderer>();
        switch (type)
        {
            default:
                renderer.sharedMaterial = emptyMaterial;
                break;
            case TileType.Laser:
                renderer.sharedMaterial = laserMaterial;
                break;
        }
    }
}
