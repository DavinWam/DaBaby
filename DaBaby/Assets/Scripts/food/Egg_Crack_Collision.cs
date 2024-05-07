using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Egg_Crack_Collision : MonoBehaviour
{
    public GameObject sunnySideUpEgg;
    public float breakVelocity = .2f;
    private void OnCollisionEnter(Collision collision) 
    {
        if (collision.relativeVelocity.magnitude > breakVelocity && collision.gameObject.tag == "Pan"){
            Vector3 pos = transform.position; 
            Destroy(gameObject);
            Instantiate(sunnySideUpEgg, pos, Quaternion.identity);
    }
    }
}
