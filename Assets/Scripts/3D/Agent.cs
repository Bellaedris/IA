using System.Collections;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using Utils;

public class Agent : MonoBehaviour, IPositionObserver
{

    private Tile _playerTile;
    private Tile _currentTile;
    
    List<Node> _path;
    
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void Update(Tile tile)
    {
        _playerTile = tile;
        PathFinder.GeneratePath(_playerTile, _currentTile, GameManager.Instance.tileManager.Neighbours);
    }
    
    private void OnDestroy()
    {
        // Unregister this AI when destroyed
        var player = FindObjectOfType<PlayerController>();
        if (player != null)
            player.UnregisterObserver(this);
    }
}
