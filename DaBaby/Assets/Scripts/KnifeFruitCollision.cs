using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KnifeFruitCollision : MonoBehaviour
{
   public GameObject choppedFruit;

    private void OnCollisionEnter(Collision collision) 
    {
        if (collision.gameObject.name == "KnifeTemp")
        {
            Vector3 pos = collision.transform.position;
            float originalX = pos.x, originalZ = pos.z;
            Destroy(gameObject);

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
