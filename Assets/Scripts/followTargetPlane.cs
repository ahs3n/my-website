using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class followTargetPlane : MonoBehaviour
{
    public Transform target;
    public bool x = true;
    public bool y = false;
    public bool z = true;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        transform.position = new Vector3(x ? target.transform.position.x : transform.position.x, y ? target.transform.position.y : transform.position.y, z ? target.transform.position.z : transform.position.z);
    }
}
