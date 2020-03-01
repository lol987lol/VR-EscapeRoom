using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR.InteractionSystem;


public class sc_playVideo : MonoBehaviour
{
    public UnityEngine.Video.VideoPlayer videoPlayer;

    // Start is called before the first frame update
    private void Start()
    {
        Debug.Log("Video abspielen!");

    }

    // Update is called once per frame
    private void Update()
    {
        
    }

    protected virtual void OnHandHoverBegin(Hand hand)
    {
        Debug.Log("Video abspielen!");
        videoPlayer.Play();

    }
}
