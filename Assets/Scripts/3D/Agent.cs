using System.Collections.Generic;
using UnityEngine;
using Utils;

public class Agent : MonoBehaviour, IPositionObserver
{
    public float speed = 1f;
    
    private int _currentNodeIndex;
    private float _timeSinceNodeEnter;
    
    private Tile _playerTile;
    private Tile _currentTile;
    
    List<Tile> _path;
    
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        _timeSinceNodeEnter += Time.deltaTime;
        if (_path == null)
            return;

        if (transform.position != _path[_currentNodeIndex].transform.position)
        {
            transform.position = Vector3.Lerp(transform.position, _path[_currentNodeIndex].transform.position, _timeSinceNodeEnter);
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
        
        _path = PathFinder.GeneratePath(_playerTile, _currentTile, GameManager.Instance.tileManager.Neighbours);
        _currentNodeIndex = 0;
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
            if (_path == null && _playerTile != null)
                Update(_playerTile);
        }
    }
}
