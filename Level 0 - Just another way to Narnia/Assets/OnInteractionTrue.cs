using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR.InteractionSystem;

public class OnInteractionTrue : MonoBehaviour
{
    public bool interacted = false;
    // Start is called before the first frame update
    void Start()
    {
        gameObject.tag = "MustInteract";
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    protected virtual void OnHandHoverBegin(Hand hand)
    {
        interacted = true;
        GameObject blockingBlanc = GameObject.FindGameObjectWithTag("Blocking");
        blockingBlanc.GetComponent<ContinueWhenAllInteracted>().CheckAllInteracted();
    }

    public bool GetInteractionStatus()
    {
        return interacted;
    }
}
