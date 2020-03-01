using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR.InteractionSystem;

public class sc_PlayJukebox : MonoBehaviour
{
    public AudioSource FunnySound;
    public AudioSource HappySound;
    public AudioSource CountrySound;
    public AudioSource PunkSound;
    public AudioSource RetroSound;
    public AudioSource RomanticSound;

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
        if (FunnySound.isPlaying)
        {
            FunnySound.Stop();
            HappySound.Play();
        }
        else if (HappySound.isPlaying)
        {
            HappySound.Stop();
            CountrySound.Play();
        }
        else if (CountrySound.isPlaying)
        {
            CountrySound.Stop();
            PunkSound.Play();
        }
        else if (PunkSound.isPlaying)
        {
            PunkSound.Stop();
            RetroSound.Play();
        }
        else if (RetroSound.isPlaying)
        {
            RetroSound.Stop();
            RomanticSound.Play();
        }
        else if (RomanticSound.isPlaying)
        {
            RomanticSound.Stop();
        }
        else
        {
            FunnySound.Play();
        }
    }
}
