using UnityEngine;

public class TextureChanger : MonoBehaviour
{
    // Static method to change texture on all materials
    public static void ChangeTexture(Renderer renderer, Texture newTexture)
    {
        // Ensure the renderer and newTexture are not null
        if (renderer != null && newTexture != null)
        {
            // Loop through all materials and set the new texture
            foreach (Material mat in renderer.materials)
            {
                mat.SetTexture("_DiffuseText", newTexture); // Update the texture key as needed
            }
        }
    }
}
