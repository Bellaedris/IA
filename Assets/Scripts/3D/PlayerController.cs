using System.Collections.Generic;
using UnityEngine;
using Utils;

public class PlayerController : MonoBehaviour, IPositionSubject
{
    public float speed = 10f;
    public float rotationSpeed = 90f;
    
    private GameManager _gm;
    private Rigidbody _rb;
    
    private List<IPositionObserver> _positionObservers;
    
    void Awake()
    {
        _positionObservers = new List<IPositionObserver>();
    }

    // Update is called once per frame
    void Update()
    {
        Vector3 direction = new Vector3(0, 0, Input.GetAxis("Vertical"));
        
        transform.Translate(speed * Time.deltaTime * direction);
        transform.Rotate(Vector3.up, Input.GetAxis("Horizontal") * rotationSpeed * Time.deltaTime);
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
