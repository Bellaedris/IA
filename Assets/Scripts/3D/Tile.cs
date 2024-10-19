using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tile : MonoBehaviour
{

    public enum TileType
    {
        Empty,
        Laser
    }

    public TileType type;

    public int _x, _y;

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

    public float GetWeight()
    {
        switch (type)
        {
            default:
                return 1;
            case TileType.Laser:
                return Mathf.Infinity;
        }
    }
}
