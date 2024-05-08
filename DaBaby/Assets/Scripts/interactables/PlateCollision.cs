using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class PlateCollision : MonoBehaviour
{
    public int maxFoodPieces = 5; // Maximum number of food pieces that can be on the plate
    public List<GameObject> foodPieces = new List<GameObject>(); // List to store food pieces
    public Transform spawnCenter; // Center around which food pieces will be spawned
    public float spawnRadius = 1.0f; // Radius within which food pieces will be spawned

    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("FoodPiece"))
        {
            // Check if the plate is not full
            if (foodPieces.Count < maxFoodPieces)
            {

                // Disable gravity for the food piece
                Rigidbody rb = collision.gameObject.GetComponent<Rigidbody>();
                if (rb != null)
                {
                    rb.velocity = Vector3.zero;
                    rb.isKinematic = true;
                }
                Collider col = collision.gameObject.GetComponent<Collider>();
                if (col != null)
                {
                    col.isTrigger = true;
                }
                // Disable interaction and make rigidbody kinematic
                XRGrabInteractable grabInteractable = collision.gameObject.GetComponent<XRGrabInteractable>();
                if (grabInteractable != null)
                {
                    grabInteractable.enabled = false;
                }
                // Attach the food piece as a child of the plate
                collision.gameObject.transform.parent = transform;
                // Randomly position the food piece around the plate
                Vector3 randomOffset = Random.insideUnitCircle * spawnRadius;
                Vector3 newPosition = spawnCenter.localPosition + new Vector3(randomOffset.x, 0.1f, randomOffset.y);
                collision.gameObject.transform.localPosition = newPosition;
                // Add the food piece to the list
                foodPieces.Add(collision.gameObject);
            }
            else
            {
                Debug.Log("Plate is full, cannot add more food.");
                // Optionally handle what happens when the plate is full
            }
        }
    }
}
