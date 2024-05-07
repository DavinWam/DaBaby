using UnityEngine;

public class HullObject : MonoBehaviour, IHullObject
{
    private HullMaterialConfig materialConfig; // No longer public, loaded from Resources
    private Material materialCopy;
    public Color outlineColor = new Color(0,0,0,1);
    public float scaleMultiplier = 1.1f; // Multiplier for scaling the hull relative to the parent object
    public bool enableDynamicScaling = false; // Flag to enable/disable dynamic scaling
    private GameObject hullInstance;
    private Vector3 scaleVector; // To store the initial scale of the hull

    void Start()
    {
        // Load the ScriptableObject from the Resources/Configurations folder
        materialConfig = Resources.Load<HullMaterialConfig>("Configurations/HullMaterial");
        // Instantiate a new material based on the materialConfig's material
        materialCopy = Instantiate(materialConfig.hullMaterial);
        if (materialConfig != null && materialConfig.hullMaterial != null)
        {
            CreateHull(gameObject.transform);
            ChangeMaterialColor(outlineColor);
        }
        else
        {
            Debug.LogError("Material configuration is missing or incomplete.");
        }
    }

    void Update()
    {
        // Check if dynamic scaling is enabled and adjust scale in Update
        if (enableDynamicScaling && hullInstance != null)
        {
            hullInstance.transform.localScale = CalculateNonUniformScale(scaleMultiplier);
            ChangeMaterialColor(outlineColor);
        }
    }

    public void CreateHull(Transform transform)
    {
        // Create a new GameObject for the hull
        hullInstance = new GameObject("InvertedHull");
        hullInstance.transform.SetParent(transform, false);

        // Copy the mesh from the current component
        MeshFilter parentMeshFilter = GetComponent<MeshFilter>();
        if (parentMeshFilter != null)
        {
            MeshFilter hullMeshFilter = hullInstance.AddComponent<MeshFilter>();
            hullMeshFilter.mesh = Instantiate(parentMeshFilter.mesh);
        }
        else
        {
            Debug.LogError("No MeshFilter found on the parent object!");
            return;
        }

        // Add a renderer and set the material
        MeshRenderer hullRenderer = hullInstance.AddComponent<MeshRenderer>();
        hullRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        hullRenderer.material = materialCopy;

        // Set the initial scale based on the multiplier
        scaleVector = CalculateNonUniformScale(scaleMultiplier);
        hullInstance.transform.localScale = scaleVector;
    }

    public Vector3 CalculateNonUniformScale(float thickness)
    {
        MeshFilter meshFilter = GetComponent<MeshFilter>();
        if (meshFilter == null || meshFilter.sharedMesh == null)
        {
            Debug.LogError("MeshFilter or Mesh not found on the object.");
            return Vector3.one; // Return default scale if no MeshFilter or Mesh is found
        }

        Bounds bounds = meshFilter.sharedMesh.bounds;  // Local bounds of the mesh
        Vector3 scaledBoundsSize = Vector3.Scale(bounds.size, transform.localScale); // Apply scale to bounds size
        float maxDimension = Mathf.Max(scaledBoundsSize.x, Mathf.Max(scaledBoundsSize.y, scaledBoundsSize.z));

        // Adjust scale factors based on the scaled bounds of the object
        float scaleX = 1 + ((thickness - 1) / (scaledBoundsSize.x ));
        float scaleY = 1 + ((thickness - 1) / (scaledBoundsSize.y ));
        float scaleZ = 1 + ((thickness - 1) / (scaledBoundsSize.z ));

        return new Vector3(scaleX, scaleY, scaleZ);
    }

    public void ChangeMaterialColor(Color newColor)
    {
        if (materialCopy != null)
        {
            // Update the color of the material
            materialCopy.SetColor("_Color", newColor);
        }
        else
        {
            Debug.LogError("Material copy is not initialized!");
        }
    }

    void OnDestroy()
    {
        // Ensure the hull instance is destroyed when the parent object is destroyed
        if (hullInstance != null)
        {
            Destroy(hullInstance);
        }
    }
}
