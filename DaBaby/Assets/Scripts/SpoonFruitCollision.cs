using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpoonFruitCollision : MonoBehaviour
{
    public List<string> potentialFruitPieces = new List<string> {"BananaSlice", "FruitPiece", "Grape"};
    public Texture foodOnSpoon;

    private void OnCollisionEnter(Collision collision) 
    {
        Renderer renderer = GetComponent<Renderer>();
        Debug.Log(collision.gameObject.name);
        if (potentialFruitPieces.Contains(collision.gameObject.name))
        {
            Destroy(collision.gameObject);
            TextureChanger.ChangeTexture(GetComponent<Renderer>(), foodOnSpoon);
        }
    }
}
