using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Egg_Crack_Collision : MonoBehaviour
{
    public GameObject sunnySideUpEgg;

    private void OnCollisionEnter(Collision collision) 
    {
        if (collision.gameObject.name == "Pan"){
            Vector3 pos = collision.transform.position;
            Destroy(gameObject);
            Instantiate(sunnySideUpEgg, pos, Quaternion.identity);
    }
    }
}
