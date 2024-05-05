using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class panCooking : MonoBehaviour
{
    public bool isOnStove = false;

    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.tag == "Stove")
        {
            isOnStove = true;
            Debug.Log("pan placed on the stove.");
        }
    }
    // void OnCollisionStay(Collision collision)
    // {
    //     if (collision.gameObject.tag == "Stove")
    //     {
    //         isOnStove = true;
    //         Debug.Log("pan placed on the stove");
    //     }
    // }

    void OnCollisionExit(Collision collision)
    {
        if (collision.gameObject.tag == "Stove")
        {
            isOnStove = false;
            Debug.Log("pan removed from the stove.");
            // No need to update the texture to raw as it's assumed to be the default
        }
    }
}
