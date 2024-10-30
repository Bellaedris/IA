using System;
using System.Collections.Generic;
using UnityEngine;
using Utils;

[RequireComponent(typeof(Animator), typeof(CharacterController))]
public class PlayerController : MonoBehaviour, IPositionSubject
{
    public float speed = 10f;
    public float rotationSmoothingTime = .1f;
    public float jumpForce = 5f;
    public Camera mainCamera;
    public Transform feet;

    private float _currentRotation;
    private bool _grounded;
    
    private GameManager _gm;
    private CharacterController _cc;
    private Animator _anim;
    
    private List<IPositionObserver> _positionObservers;
    
    void Awake()
    {
        _positionObservers = new List<IPositionObserver>();
        _cc = GetComponent<CharacterController>();
        _anim = GetComponent<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
        Vector3 direction = new Vector3(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical")).normalized;
        Vector3 movement = Vector3.zero;
        
        transform.rotation = Quaternion.Euler(0, mainCamera.transform.eulerAngles.y, 0);
        if (direction.magnitude > 0.1f)
        {
            float targetAngle = Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg +
                                mainCamera.transform.eulerAngles.y;
            float angle = Mathf.SmoothDampAngle(transform.eulerAngles.y, targetAngle, ref _currentRotation,
                rotationSmoothingTime);

            movement = Quaternion.Euler(0, targetAngle, 0) * Vector3.forward;
            //transform.rotation = Quaternion.Euler(0f, angle, 0f);
            
            if (Input.GetKeyDown(KeyCode.Space) && _grounded)
            {
                movement.y = jumpForce;
                _anim.SetTrigger("Jump");
            }
        }
        
        _anim.SetFloat("Speed", direction.magnitude * speed);
        
        movement.y += -9.8f * Time.deltaTime; // add gravity
        _cc.Move(speed * Time.deltaTime * movement);
        
        // update grounded state
        RaycastHit hit;
        Physics.Raycast(feet.position, Vector3.down, out hit, 0.1f);
            _grounded = hit.collider is not null;
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
