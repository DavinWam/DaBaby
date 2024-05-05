using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NoMoreFoodOnPlate : MonoBehaviour
{
    public List<string> potentialFruitPieces = new List<string> {"BananaSlice", "FruitPiece", "Grape"};

    // how to make sure plate is deleted afterwards
    // void OnCollisionExit(Collision collision)
    // {
    //     if (potentialFruitPieces.Contains(collision.gameObject.name))
    //     {
    //         Destroy(gameObject);
    //     }
    // }
}
