using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KnifeFruitCollision : MonoBehaviour
{
   public GameObject choppedFruit;
   public AudioSource chopping;
    public float chopVelocity = .8f;
    private void OnCollisionEnter(Collision collision) 
    {
        if (collision.gameObject.tag == "Knife" && collision.relativeVelocity.magnitude > chopVelocity)
        {
            Vector3 pos = transform.position;
            float originalX = pos.x, originalZ = pos.z;

            chopping.Play();

            Destroy(gameObject);
            
            pos.y += .2f;




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
