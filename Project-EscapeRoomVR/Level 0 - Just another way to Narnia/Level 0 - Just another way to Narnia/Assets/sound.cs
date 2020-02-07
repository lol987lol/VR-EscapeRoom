using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class sound : MonoBehaviour
{
    AudioClip clipa;
    AudioClip clipb;
    bool played;
    WWW www;
    float timer;

    public string url = "http://streams.ilovemusic.de/iloveradio1.mp3";
    public int interval = 300;

    void Start()
    {
        clipa = null;
        played = false;
        timer = 0;

    }

    void Update()
    {
        Debug.Log(timer);

        timer = timer + 1 * Time.deltaTime; //Mathf.FloorToInt(Time.timeSinceLevelLoad*10); 
                                            //Time.frameCount; 

        if (timer >= interval)
        {             //if(timer%interval == 0){
            if (www != null)
            {
                www.Dispose();
                www = null;
                played = false;
                timer = 0;
            }
        }
        else
        {
            if (www == null)
            {
                www = new WWW(url);
            }
        }
        if (clipa == null)
        {
            if (www != null)
            {
                clipa = www.GetAudioClip(false, true);
            }
        }

        if (clipa != null)
        {
            if (clipa.isReadyToPlay && played == false)
            {
                GetComponent<AudioSource>().PlayOneShot(clipa);
                played = true;
                clipa = null;
            }
        }
    }
}
