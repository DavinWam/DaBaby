using UnityEngine;

public class TextureChanger : MonoBehaviour
{

    public void ChangeTexture(Renderer renderer, Texture newTexture)
    {
        //gameObject.GetComponent<Renderer>();

        // Make sure the renderer and newTexture are not null
        if (renderer != null && newTexture != null)
        {
            // Change the texture tagged as "_DiffuseText"
            renderer.material.SetTexture("_DiffuseText", newTexture);
        }
    }
}
