using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Valve.VR.InteractionSystem;

public class ResetLevel0 : MonoBehaviour
{

    Scene m_Scene;
    string sceneName;

    // Start is called before the first frame update
    void Start()
    {
        Debug.Log("Start Scene");
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    protected virtual void OnHandHoverBegin(Hand hand)
    {
        Debug.Log("Reset Scene");
        m_Scene = SceneManager.GetActiveScene();
        sceneName = m_Scene.name;
        SceneManager.LoadScene(sceneName);
    }
}
