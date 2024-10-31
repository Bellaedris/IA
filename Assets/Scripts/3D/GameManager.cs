using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    
    public TileManager tileManager;

    private static GameManager _instance;
    public static GameManager Instance => _instance;

    private int _numberOfkeysObtained = 0;
    private int _numberOfKeysTotal;
    
    private PlayerController _player;

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
        foreach (var agent in FindObjectsByType<Agent>(FindObjectsInactive.Exclude, FindObjectsSortMode.None))
        {
            _player.RegisterObserver(agent);
        }

        _numberOfKeysTotal = FindObjectsByType<Key>(FindObjectsInactive.Exclude, FindObjectsSortMode.None).Length;
    }

    public void KeyObtained()
    {
        _numberOfkeysObtained++;
        if (_numberOfkeysObtained == _numberOfKeysTotal)
            Debug.Log("You won");
    }
}
