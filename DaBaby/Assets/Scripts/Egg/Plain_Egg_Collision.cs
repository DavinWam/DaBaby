using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Plain_Egg_Collision : MonoBehaviour
{
    public GameObject crackedEgg;
    private bool hasMadeEgg = false;
    public float breakVelocity = .2f;

    private void OnCollisionEnter(Collision collision) 
    {

         Debug.Log(collision.relativeVelocity.magnitude);
        if ( collision.relativeVelocity.magnitude > breakVelocity && !hasMadeEgg){
            hasMadeEgg = true;
            Debug.Log(collision.gameObject.name);
            Vector3 pos = transform.position + new Vector3(0,.2f,0);
            Destroy(gameObject);
            GameObject newEgg = Instantiate(crackedEgg, pos, Quaternion.identity);
            newEgg.SetActive(true);
        }
    }
}
