using System;
using UnityEngine;

public class FoodPiece : MonoBehaviour
{
    public int maxInteractions = 3;
    public GameObject filledSpoonPrefab; // Prefab of the spoon when filled with this type of food
    public FoodType foodType;
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
