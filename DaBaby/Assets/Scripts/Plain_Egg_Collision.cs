using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Plain_Egg_Collision : MonoBehaviour
{
    public GameObject crackedEgg;

    private void OnCollisionEnter(Collision collision) 
    {

        
        if (collision.gameObject.name != "box" && collision.gameObject.name != "holder" && collision.gameObject.name != "counter" && collision.gameObject.name != "egg"){
            Debug.Log(collision.gameObject.name);
            Vector3 pos = transform.position;
            Destroy(gameObject);
            Instantiate(crackedEgg, pos, Quaternion.identity);
        }
    }
}
