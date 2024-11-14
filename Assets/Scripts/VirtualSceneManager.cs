using System.Collections;
using System.Collections.Generic;
using Lasp;
using UnityEngine;
using UnityEngine.Experimental.GlobalIllumination;

[RequireComponent(typeof(SpectrumAnalyzer))]
public class VirtualSceneManager : MonoBehaviour
{
    public GameObject[] bassSpots;
    public GameObject[] bandSpots;
    public GameObject[] highSpots;

    public Quaternion bassRotation;
    public Quaternion bandRotation;
    public Quaternion highRotation;

    private Quaternion baseRotation;

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
    
    
    // Start is called before the first frame update
    void Start()
    {
        baseRotation = Quaternion.Euler(0, Random.Range(30f, 60f), Random.Range(30f, 60f));
    }

    // Update is called once per frame
    void Update()
    {
        foreach (var bass in bassSpots)
            bass.transform.rotation = bassRotation * baseRotation;
        
        foreach (var band in bandSpots)
            band.transform.rotation = bandRotation * baseRotation;
        
        foreach (var high in highSpots)
            high.transform.rotation = highRotation * baseRotation;
    }
}
