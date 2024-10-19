using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Utils;

public class PlayerController : MonoBehaviour, IPositionSubject
{
    private GameManager _gm;
    
    private List<IPositionObserver> _positionObservers;
    
    void Awake()
    {
        _positionObservers = new List<IPositionObserver>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Tile"))
            Notify(other.GetComponent<Tile>());
    }

    public void RegisterObserver(IPositionObserver observer)
    {
        _positionObservers.Add(observer);
    }

    public void UnregisterObserver(IPositionObserver observer)
    {
        _positionObservers.Remove(observer);
    }

    public void Notify(Tile tile)
    {
        foreach (var observer in _positionObservers)
        {
            observer.Update(tile);
        }
    }
}
