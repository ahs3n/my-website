using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class lightningExplosion : MonoBehaviour
{
    public player user;



    private ParticleSystem part;
    private List<ParticleCollisionEvent> collisionEvents;

    void Start()
    {
        part = gameObject.GetComponent<ParticleSystem>();
        collisionEvents = new List<ParticleCollisionEvent>();
    }



    void OnParticleCollision(GameObject other)
    {
        int numCollisionEvents = part.GetCollisionEvents(other, collisionEvents);

        Rigidbody rb = other.GetComponent<Rigidbody>();
        int i = 0;

        while (i < numCollisionEvents)
        {

            Vector3 pos = collisionEvents[i].intersection;

            //foreach (GameObject car in user.cars)
            //{
            //    rb = car.GetComponent<Rigidbody>();
            //    rb.AddExplosionForce(500000, pos, 40);

            //}

            i++;
        }
    }
}
