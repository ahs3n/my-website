using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class smoothRigidbodyFollow : MonoBehaviour
{
    public GameObject target;
    public float speed = 500f;
    private Rigidbody rb;
    public bool followingTarget = true;
    public float followStartDelay;

    private float originalDrag;

    // Start is called before the first frame update
    void Start()
    {
        rb = gameObject.GetComponent<Rigidbody>();
        originalDrag = rb.drag;
        if (speed < 0f)
        {
            if (speed != 0f)
            {
                speed = Mathf.Abs(speed);
            }
            else
            {
                speed = 10f;
            }
        }

        StartCoroutine(Wait());
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (followingTarget) { rb.AddForce((target.transform.position - gameObject.transform.position) * speed); }
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.R))
        {
            rb.drag = 15;
            StartCoroutine(Wait());
        }
    }

    IEnumerator Wait()
    {
        yield return new WaitForSeconds(followStartDelay);
        followingTarget = true;
        rb.drag = originalDrag;
    }
}
