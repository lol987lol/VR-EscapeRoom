using UnityEngine;
using Valve.VR.InteractionSystem;

public class Light_switch : MonoBehaviour
{

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
        var lightObjects = GameObject.FindGameObjectsWithTag("Light");

        foreach (GameObject obj in lightObjects)
        {
            obj.GetComponent<Light>().enabled = !obj.GetComponent<Light>().enabled;
        }
    }
}
