using UnityEngine;

public class MaterialSwapper : MonoBehaviour
{
    private Material[] originalMaterials;
    public Material swapMaterial;
    public BabyAI babyAI;
    // Function to swap materials
    public void SwapMaterials()
    {
        if (babyAI != null && babyAI.isBeingHeld == true){
            return;
        }
        Renderer renderer = GetComponent<Renderer>();
        if (renderer == null){
            renderer = GetComponentInChildren<Renderer>();
        }
        if (renderer != null)
        {
            // Store original materials if not already stored
            if (originalMaterials == null)
                originalMaterials = renderer.materials;

            // Create an array of swap materials with the length of original materials
            Material[] swapMaterials = new Material[originalMaterials.Length];
            for (int i = 0; i < swapMaterials.Length; i++)
            {
                swapMaterials[i] = swapMaterial;
            }

            // Assign the swap materials to the renderer
            renderer.materials = swapMaterials;
        }
        else
        {
            Debug.LogWarning("Renderer component not found on object.");
        }
    }
    public void SwapMaterials(Renderer renderer)
    {
        if (renderer != null)
        {
            // Store original materials if not already stored
            if (originalMaterials == null)
                originalMaterials = renderer.materials;

            // Create an array of swap materials with the length of original materials
            Material[] swapMaterials = new Material[originalMaterials.Length];
            for (int i = 0; i < swapMaterials.Length; i++)
            {
                swapMaterials[i] = swapMaterial;
            }

            // Assign the swap materials to the renderer
            renderer.materials = swapMaterials;
        }
        else
        {
            Debug.LogWarning("Renderer component not found on object.");
        }
    }
    // Function to revert to original materials
    public void RevertMaterials()
    {
        Renderer renderer = GetComponent<Renderer>();
        if (renderer != null && originalMaterials != null)
        {
            // Assign the original materials back to the renderer
            renderer.materials = originalMaterials;
        }
        else
        {
            Debug.LogWarning("Renderer component not found or original materials not stored.");
        }
    }
        public void RevertMaterials(Renderer renderer)
    {
        if (renderer != null && originalMaterials != null)
        {
            // Assign the original materials back to the renderer
            renderer.materials = originalMaterials;
        }
        else
        {
            Debug.LogWarning("Renderer component not found or original materials not stored.");
        }
    }
}
