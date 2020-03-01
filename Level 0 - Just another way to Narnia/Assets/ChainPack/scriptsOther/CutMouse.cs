using UnityEngine;

//This script is simply throwing a ray from the previous position to the current mouse position.
//And if a ray collides with any of the objects, the object is passed to the static function Chain.CutMe,
//which will check whether the object passed in the chain link, and if it is, then the circuit is cut off.

public class CutMouse : MonoBehaviour
{
    private bool cutOn;
    private Vector3 oldMouse;
    private Vector3 currentMouse;

    private LineRenderer line;

    void Start()
    {
        line = GetComponent<LineRenderer>();
    }
    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            cutOn = true;
            oldMouse = (Vector2)Camera.main.ScreenToWorldPoint(Input.mousePosition);
        }

        if (Input.GetMouseButtonUp(0))
            cutOn = false;

        if (cutOn)
        {
            currentMouse = (Vector2)Camera.main.ScreenToWorldPoint(Input.mousePosition);
            Ray2D ray = new Ray2D(oldMouse, currentMouse - oldMouse);
            RaycastHit2D hit = Physics2D.Raycast(ray.origin, ray.direction, (currentMouse - oldMouse).magnitude);
            if (hit.collider != null)
            {
                Chain.CutMe(hit.collider.gameObject);
            }


        }

        line.SetPosition(0, currentMouse);
        line.SetPosition(1, oldMouse);
        oldMouse = currentMouse;
    }


}
