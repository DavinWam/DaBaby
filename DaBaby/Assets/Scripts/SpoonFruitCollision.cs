using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpoonFruitCollision : MonoBehaviour
{
    //public List<string> potentialFruitPieces = new List<string> {"BananaSlice", "FruitPiece", "grape"};
    public GameObject bananaSpoon, fruitSpoon, grapeSpoon;

    private void OnCollisionEnter(Collision collision) 
    {
        Debug.Log(collision.gameObject.name);
        //Debug.Log(potentialFruitPieces.Contains(collision.gameObject.name));
        Debug.Log(collision.gameObject.name == "BananaSlice" || collision.gameObject.name == "FruitPiece" || collision.gameObject.name == "grape");

        //if (potentialFruitPieces.Contains(collision.gameObject.name))
        if (collision.gameObject.name.Contains("BananaSlice") || collision.gameObject.name.Contains("FruitPiece") || collision.gameObject.name.Contains("grape"))
        {
            Debug.Log(collision.gameObject.name);
            Debug.Log("msfdr it thus far");

            Destroy(collision.gameObject);
            Destroy(gameObject);
            GetFoodSpoon(collision.gameObject.name, transform.position);
        }
    }

    void GetFoodSpoon(string name, Vector3 pos) {
        if (name.Contains("BananaSlice")) 
        {
            Instantiate(bananaSpoon, pos, Quaternion.identity);
        }
        else if (name.Contains("FruitPiece"))
        {
            Instantiate(fruitSpoon, pos, Quaternion.identity);
        }
        else if (name.Contains("grape"))
        {
            Instantiate(grapeSpoon, pos, Quaternion.identity);
        }
    }
}
