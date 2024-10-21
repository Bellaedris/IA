using System;
using System.Collections.Generic;
using UnityEngine;
using Utils;

[RequireComponent(typeof(Rigidbody), typeof(Animator), typeof(CharacterController))]
public class PlayerController : MonoBehaviour, IPositionSubject
{
    public float speed = 10f;
    public float rotationSmoothingTime = .1f;
    public float jumpForce = 5f;
    public Camera mainCamera;

    private float _currentRotation;
    
    private GameManager _gm;
    private Rigidbody _rb;
    private CharacterController _cc;
    private Animator _anim;
    
    private List<IPositionObserver> _positionObservers;
    
    void Awake()
    {
        _positionObservers = new List<IPositionObserver>();
        _cc = GetComponent<CharacterController>();
        _anim = GetComponent<Animator>();
        _rb = GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    void Update()
    {
        Vector3 direction = new Vector3(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical")).normalized;
        if (Input.GetKeyDown(KeyCode.Space))
        {
            _rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
            _anim.SetTrigger("Jump");
            Debug.Log("Jump");
        }
        if (direction.magnitude > 0.1f)
        {

            float targetAngle = Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg +
                                mainCamera.transform.eulerAngles.y;
            float angle = Mathf.SmoothDampAngle(transform.eulerAngles.y, targetAngle, ref _currentRotation,
                rotationSmoothingTime);

            Vector3 targetDirection = Quaternion.Euler(0, targetAngle, 0) * Vector3.forward;
            transform.rotation = Quaternion.Euler(0f, angle, 0f);
            _cc.Move(speed * Time.deltaTime * targetDirection);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Tile"))
            Notify(other.GetComponent<Tile>());
    }
    

    private void OnCollisionEnter(Collision other)
    {
        if (other.gameObject.CompareTag("Tile"))
        {
            Debug.Log("stopJump");
            _anim.SetTrigger("StopJump");
        }
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
