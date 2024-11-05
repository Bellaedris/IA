using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    
    public TileManager tileManager;
    public TMP_Text text;
    
    private static GameManager _instance;
    public static GameManager Instance => _instance;

    private int _numberOfkeysObtained = 0;
    private int _numberOfKeysTotal;
    
    private PlayerController _player;
    private Agent[] _agents;
    
    // Start is called before the first frame update
    void Start()
    {
        if (_instance != null)
        {
            Destroy(this);
            return;
        }

        _instance = this;

        _player = FindObjectOfType<PlayerController>();
        _agents = FindObjectsByType<Agent>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);
        foreach (var agent in _agents)
        {
            _player.RegisterObserver(agent);
        }

        _numberOfKeysTotal = FindObjectsByType<Key>(FindObjectsInactive.Exclude, FindObjectsSortMode.None).Length;
    }

    public List<Tile> GeneratePath(Tile start, Tile end, bool useAStar)
    {
        List<Tile> occupiedTiles = new List<Tile>();
        foreach (Agent a in _agents)
            if (a.GetCurrentTile() != end)
                occupiedTiles.Add(a.GetCurrentTile());

        return PathFinder.GeneratePath(start, end, tileManager.Neighbours, occupiedTiles, useAStar);
    }

    public void KeyObtained()
    {
        _numberOfkeysObtained++;
        if (_numberOfkeysObtained == _numberOfKeysTotal)
            ShowMessage("You won");
    }

    public void ShowMessage(string message)
    {
        text.text = message;
    }
}
