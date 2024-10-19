using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    
    public TileManager tileManager;
    public Agent agent;
    public PlayerController player;

    private static GameManager _instance;
    public static GameManager Instance => _instance;

    // Start is called before the first frame update
    void Start()
    {
        if (_instance != null)
        {
            Destroy(this);
            return;
        }
        else
            _instance = this;

        player = FindObjectOfType<PlayerController>();
        foreach (var agent in FindObjectsByType<Agent>(FindObjectsInactive.Exclude, FindObjectsSortMode.None))
        {
            player.RegisterObserver(agent);
        }
    }
}
