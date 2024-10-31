using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(SphereCollider))]
public class Key : MonoBehaviour
{
    public float hoverHeight;
    public float hoverSpeed;
    
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        Vector3 movement = Time.deltaTime * Mathf.Sin(Time.realtimeSinceStartup * hoverSpeed) * hoverHeight * Vector3.up;
        transform.Translate(movement);
    }

    private void OnTriggerEnter(Collider other)
    {
        // if the key is hit by the player, switch the assigned blocks to laser 
        // and update game manager
        if (other.CompareTag("Player"))
        {
            GameManager.Instance.KeyObtained();
            Destroy(gameObject);
        }
    }
}
