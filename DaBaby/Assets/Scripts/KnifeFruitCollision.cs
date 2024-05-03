using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KnifeFruitCollision : MonoBehaviour
{
   public GameObject choppedFruit;
   //List<string> potentialFruits = new List<string> {"Banana", "Apple", "Pear", "Grapes"};

    private void OnCollisionEnter(Collision collision) 
    {
        if (collision.gameObject.name == "Apple")
        {
            Vector3 pos = collision.transform.position;
            float originalX = pos.x, originalZ = pos.z;
            Destroy(gameObject);

            for (int i = 0; i < 5; i++)
            {
                if (i % 2 == 0){
                 pos.x = originalX - (i * 0.02f);
                } else{
                 pos.z = originalZ - (i * 0.02f);
                }
                Instantiate(choppedFruit, pos, Quaternion.identity);
            }
        }
    }
}
