using System.Collections;
using System.Collections.Generic;
using Lasp;
using UnityEngine;
using UnityEngine.Experimental.GlobalIllumination;

[RequireComponent(typeof(SpectrumAnalyzer))]
public class VirtualSceneManager : MonoBehaviour
{
    public float bassSinSpeed = 5f;
    public float bandRotationSpeed = 5f;
    
    public GameObject[] bassSpots;
    public GameObject[] bandSpots;
    public GameObject[] highSpots;

    [HideInInspector]
    public Quaternion bassRotation;
    [HideInInspector]
    public Quaternion bandRotation;
    [HideInInspector]
    public Quaternion highRotation;

    private List<Quaternion> bandFuzzRotations;

    public Quaternion BassRotation
    {
        get => bassRotation;
        set => bassRotation = value;
    }

    public Quaternion BandRotation
    {
        get => bandRotation;
        set => bandRotation = value;
    }

    public Quaternion HighRotation
    {
        get => highRotation;
        set => highRotation = value;
    }

    void Start()
    {
        bandFuzzRotations = GenerateFuzzRotations(bandSpots.Length);
    }

    void Update()
    {
        float bassSinRotation = Mathf.Abs(Mathf.Sin(Time.time * bassSinSpeed * Mathf.PI * Mathf.Deg2Rad)) * 90f;
        foreach (var spot in bassSpots)
            spot.transform.localRotation = bassRotation * Quaternion.Euler(0f, bassSinRotation, 0f);
        
        Quaternion bandCircleRotation = Quaternion.Euler(0f, Mathf.Sin(Time.time * bandRotationSpeed * Mathf.PI * Mathf.Deg2Rad) * 23f, Mathf.Cos(Time.time * bandRotationSpeed * Mathf.PI * Mathf.Deg2Rad) * 30f);
        for (int i = 0; i < bandSpots.Length; i++)
            bandSpots[i].transform.localRotation = bandRotation * bandCircleRotation * bandFuzzRotations[i];
        
        for (int i = 0; i < highSpots.Length; i++)
            highSpots[i].transform.rotation = highRotation;
    }

    private List<Quaternion> GenerateFuzzRotations(int count)
    {
        var fuzzRotations = new List<Quaternion>();
        for (int i = 0; i < count; i++)
        {
            float fuzzX = Random.Range(-20f, 20f);
            float fuzzY = Random.Range(-20f, 20f);
            float fuzzZ = Random.Range(-20f, 20f);
            fuzzRotations.Add(Quaternion.Euler(fuzzX, fuzzY, fuzzZ));
        }
        return fuzzRotations;
    }
}
