using UnityEngine;
using System.Collections;
using System;

// Класс звена. Когда звено удаляется, то оно посылает событие к цепи, чтобы та удалила это звено из массива links
public class Link : MonoBehaviour
{
    public Action<GameObject> removeMeFromList;
    
    public void OnDestroy()
    {
        if (removeMeFromList != null) removeMeFromList(gameObject);
    }
}
