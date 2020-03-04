using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR.InteractionSystem;

public class ContinueWhenAllInteracted : MonoBehaviour
{
    public bool allInteracted = false;
    public GameObject[] importantObjects;
    public AudioSource Sound;
    public AudioSource WayCleared;

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
        if (!allInteracted)
        {
            Sound.Play();
        }
    }

    public void CheckAllInteracted()
    {
        importantObjects = GameObject.FindGameObjectsWithTag("MustInteract");

        foreach (GameObject obj in importantObjects)
        {
            if (obj.GetComponent<OnInteractionTrue>().GetInteractionStatus() == false)
            {
                allInteracted = false;
                break;
            }
            else allInteracted = true;
        }

        if (allInteracted)
        {
            StartCoroutine(Wait());
            Destroy(GameObject.Find("DestructableBlanc"));
            WayCleared.Play();
            CircularDrive Lock1 = GameObject.Find("dr R.001").GetComponent<CircularDrive>();
            CircularDrive Lock2 = GameObject.Find("dr L.001").GetComponent<CircularDrive>();
            Lock1.enabled = true;
            Lock2.enabled = true;
        }
    }

    private IEnumerator Wait()
    {
        yield return new WaitForSeconds(5.0f);
        WayCleared.Play();
    }
}
