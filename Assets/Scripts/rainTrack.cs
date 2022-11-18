using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class rainTrack : MonoBehaviour
{
    public GameObject target;
    public float maxDistance = 10;
    
    void Update()
    {
        if ((target.transform.position-gameObject.transform.position).magnitude > maxDistance)
        {
            gameObject.transform.position = target.transform.position;
        }
    }
}
