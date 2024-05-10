using UnityEngine;

public class SpoonCollision : MonoBehaviour
{
    public GameObject defaultSpoon; // The empty spoon object, likely this gameObject itself.
    public Transform spoonParent; // The parent object to maintain the correct hierarchy and transformations.
    private FoodType  spoonFoodType = FoodType.Empty;
    private GameObject currentFilledSpoon = null;
    public MaterialSwapper ms;
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

                spoonFoodType = other.GetComponent<FoodPiece>().foodType;
                // Optionally disable the default spoon to show only the filled one
                defaultSpoon.GetComponent<MeshRenderer>().enabled = false;

                // Handle interaction with the food piece
                foodPiece.Interact();
            }
        }

    }
    public void ChangeFilledSpoonMat(){
        if (currentFilledSpoon != null){
            Renderer renderer = currentFilledSpoon.GetComponent<Renderer>();
            ms.SwapMaterials(renderer);
        }
    }
    public void ResetFilledSpoonMat(){
        if (currentFilledSpoon != null){
            Renderer renderer = currentFilledSpoon.GetComponent<Renderer>();
            ms.RevertMaterials(renderer);
        }
    }
    private void OnCollisionEnter(Collision collision){
        if (collision.gameObject.name.Contains("babyAnimated") && currentFilledSpoon)
        {
            if(collision.gameObject.GetComponent<BabyAI>().Eat(spoonFoodType)){
                ResetSpoon();
            }
            
        }
    }
    public void ResetSpoon()
    {
        // Destroy the filled spoon and reactivate the default spoon
        if (currentFilledSpoon != null)
        {
            Destroy(currentFilledSpoon);
            currentFilledSpoon = null;
            spoonFoodType = FoodType.Empty;
        }
        defaultSpoon.GetComponent<MeshRenderer>().enabled = true;
    }
}
