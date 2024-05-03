using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Egg_Crack_Collision : MonoBehaviour
{
    public GameObject sunnySideUpEgg;

    private void OnCollisionEnter(Collision collision) 
    {
        if (collision.gameObject.name == "Pan"){
            Vector3 pos = collision.transform.position - new Vector3(0.12f,0,0);
            Destroy(gameObject);
            Instantiate(sunnySideUpEgg, pos, Quaternion.identity);
    }
    }
}
