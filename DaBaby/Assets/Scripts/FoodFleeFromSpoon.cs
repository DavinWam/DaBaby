using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FoodFleeFromSpoon : MonoBehaviour
{
    public GameObject regularSpoon;
    
    // Start is called before the first frame update
    private void OnCollisionEnter(Collision collision) 
    {
        if (collision.gameObject.name.Contains("babyAnimated"))
        {
            Destroy(gameObject);
            Instantiate(regularSpoon, transform.position, Quaternion.identity);
        }
    }
}
