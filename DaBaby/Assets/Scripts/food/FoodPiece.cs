using UnityEngine;

public class FoodPiece : MonoBehaviour
{
    public int maxInteractions = 3;
    public GameObject filledSpoonPrefab; // Prefab of the spoon when filled with this type of food

    private int interactionCount = 0;

    public void Interact()
    {
        
        interactionCount++;
        if (interactionCount >= maxInteractions)
        {
            Destroy(gameObject);
        }
    }
}
