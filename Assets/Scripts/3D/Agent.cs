using System.Collections.Generic;
using UnityEngine;
using Utils;

public class Agent : MonoBehaviour, IPositionObserver
{
    public float speed = 1f;
    public bool useAStar = false;
    
    private int _currentNodeIndex;
    private float _timeSinceNodeEnter;
    private float _currentSpeed;
    
    private Tile _playerTile;
    private Tile _currentTile;
    
    List<Tile> _path;

    // Update is called once per frame
    void Update()
    {
        if (_path == null)
            return;

        if (transform.position != _path[_currentNodeIndex].transform.position)
        {
            _timeSinceNodeEnter += Time.deltaTime;
            transform.position = Vector3.Lerp(transform.position, _path[_currentNodeIndex].transform.position, _timeSinceNodeEnter * _currentSpeed);
            transform.LookAt(_playerTile.transform.position);
        }
        else
        {
            _timeSinceNodeEnter = 0;
            if (_currentNodeIndex < _path.Count - 1)
                _currentNodeIndex += 1;
        }
    }

    public void Update(Tile tile)
    {
        _playerTile = tile;
        
        if (!_currentTile)
            return;
        
        // deactivate all indicators on old path
        if (_path != null)
            foreach (Tile t in _path)
                t.ToggleIndicator();
        
        List<Tile> path = GameManager.Instance.GeneratePath(_playerTile, _currentTile, useAStar);

        if (path == null)
            return;
          
        _path = path;
        _currentNodeIndex = 0;
        
        // activate all in new path
        foreach (Tile t in _path)
            t.ToggleIndicator();
    }
    
    private void OnDestroy()
    {
        // Unregister this AI when destroyed
        var player = FindObjectOfType<PlayerController>();
        if (player != null)
            player.UnregisterObserver(this);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Tile"))
        {
            _currentTile = other.GetComponent<Tile>();
            if (_playerTile != null)
                Update(_playerTile);

            switch (_currentTile.type)
            {
                case Tile.TileType.Laser:
                    _currentSpeed = speed * .1f;
                    break;
                case Tile.TileType.Hangar:
                    _currentSpeed = speed * .3f;
                    break;
                default:
                    _currentSpeed = speed;
                    break;
            }
        }
    }

    public Tile GetCurrentTile()
    {
        return _currentTile;
    }
}
