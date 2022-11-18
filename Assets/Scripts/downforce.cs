using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(carDriver))]
public class downforce : MonoBehaviour
{
    public int constForce = 5000;
    public int multiplier = 200;

    private carDriver car;
    private Rigidbody rb;

    private void Start()
    {
        rb = gameObject.GetComponent<Rigidbody>();
        car = gameObject.GetComponent<carDriver>();
    }


    // Update is called once per frame
    void Update()
    {
        //RaycastHit rayHit;
        //if (Physics.Raycast(gameObject.transform.position, gameObject.transform.position - Vector3.up, out rayHit, 100))
        //{ }
        rb.AddForce(Vector3.down * constForce + Vector3.down * car.speed * multiplier);//*/ * rayHit.distance);
    }
}
