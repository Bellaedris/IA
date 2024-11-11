using System.Collections;
using System.Collections.Generic;
using Lasp;
using UnityEngine;

[RequireComponent(typeof(SpectrumAnalyzer))]
public class VirtualSceneManager : MonoBehaviour
{
    
    private SpectrumAnalyzer _spectrumAnalyzer;
    
    // Start is called before the first frame update
    void Start()
    {
        _spectrumAnalyzer = GetComponent<SpectrumAnalyzer>();
    }

    // Update is called once per frame
    void Update()
    {
        Debug.Log(_spectrumAnalyzer.logSpectrumArray);
    }
}
