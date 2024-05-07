using UnityEngine;

public class SpoonCollision : MonoBehaviour
{
    public GameObject defaultSpoon; // The empty spoon object, likely this gameObject itself.
    public Transform spoonParent; // The parent object to maintain the correct hierarchy and transformations.

    private GameObject currentFilledSpoon = null;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("FoodPiece") && currentFilledSpoon == null)
        {
            FoodPiece foodPiece = other.GetComponent<FoodPiece>();
            if (foodPiece != null)
            {
                // Instantiate the filled spoon prefab and position it at the current spoon's location
                currentFilledSpoon = Instantiate(foodPiece.filledSpoonPrefab, defaultSpoon.transform.position, defaultSpoon.transform.rotation, spoonParent);
                currentFilledSpoon.SetActive(true);

                // Optionally disable the default spoon to show only the filled one
                defaultSpoon.GetComponent<MeshRenderer>().enabled = false;

                // Handle interaction with the food piece
                foodPiece.Interact();
            }
        }

    }

    private void OnCollisionEnter(Collision collision){
        if (collision.gameObject.name.Contains("babyAnimated") && currentFilledSpoon)
        {
            ResetSpoon();
        }
    }
    public void ResetSpoon()
    {
        // Destroy the filled spoon and reactivate the default spoon
        if (currentFilledSpoon != null)
        {
            Destroy(currentFilledSpoon);
            currentFilledSpoon = null;
        }
        defaultSpoon.GetComponent<MeshRenderer>().enabled = true;
    }
}
