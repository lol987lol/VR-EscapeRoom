using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR.InteractionSystem;


public class sc_playSound : MonoBehaviour
{
    public AudioSource Sound;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    protected virtual void OnHandHoverBegin(Hand hand)
    {
        Sound.Play();
    }
}
