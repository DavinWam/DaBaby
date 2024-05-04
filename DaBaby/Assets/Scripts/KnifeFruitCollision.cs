using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KnifeFruitCollision : MonoBehaviour
{
   public GameObject choppedFruit, plate;

    private void OnCollisionEnter(Collision collision) 
    {
        if (collision.gameObject.name == "THEKnife")
        {
            Vector3 pos = collision.transform.position;
            float originalX = pos.x, originalZ = pos.z;
            Destroy(gameObject);

            Instantiate(plate, pos, Quaternion.identity);
            
            pos.y += .5f;


            for (int i = 0; i < 5; i++)
            {
                float subDistance = (i * 0.02f);

                if (i % 2 == 0)
                {
                 pos.x = originalX - subDistance;
                } else 
                {
                 pos.z = originalZ - subDistance;
                }

                Instantiate(choppedFruit, pos, Quaternion.identity);
            }
        }
    }
}
