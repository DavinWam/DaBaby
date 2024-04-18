using System.Collections.Generic;
using UnityEngine;

public class RecursiveHullObject : MonoBehaviour, IHullObject
{
    private HullMaterialConfig materialConfig;
    private Material materialCopy;
    public Color outlineColor = new Color(0,0,0,1);
    public float scale = 1.1f;
    public bool enableDynamicScaling = false;
    private List<GameObject> allHullInstances = new List<GameObject>();

    void Start()
    {
        materialConfig = Resources.Load<HullMaterialConfig>("Configurations/HullMaterial");
        // Instantiate a new material based on the materialConfig's material
        materialCopy = Instantiate(materialConfig.hullMaterial);
        if (materialConfig != null && materialConfig.hullMaterial != null)
        {
            List<Transform> transforms = new List<Transform>();
            transforms.Add(gameObject.transform);
            transforms.AddRange(CollectChildTransformsWithMesh());
            foreach (Transform transform in transforms)
            {
                CreateHull(transform);
            }

            ChangeMaterialColor(outlineColor);
            //some scaling problems
            foreach (GameObject hullInstance in allHullInstances)
            {
                MeshFilter meshFilter = hullInstance.GetComponent<MeshFilter>();
                if (meshFilter != null)
                {
                    hullInstance.transform.localScale = CalculateNonUniformScale(meshFilter, scale);
                }
            }
        }
        else
        {
            Debug.LogError("Material configuration is missing or incomplete.");
        }
    }

    void Update()
    {
        if (enableDynamicScaling)
        {
            foreach (GameObject hullInstance in allHullInstances)
            {
                MeshFilter meshFilter = hullInstance.GetComponent<MeshFilter>();
                if (meshFilter != null)
                {
                    hullInstance.transform.localScale = CalculateNonUniformScale(meshFilter, scale);
                }
            }
            ChangeMaterialColor(outlineColor);
        }
    }

    List<Transform> CollectChildTransformsWithMesh()
    {
        List<Transform> childTransforms = new List<Transform>();
        MeshFilter[] meshFilters = GetComponentsInChildren<MeshFilter>(true);

        foreach (MeshFilter meshFilter in meshFilters)
        {
            //skip hull objects
            if(meshFilter.gameObject.name == "InvertedHull") continue;
            
            // Skip the current object itself
            if (meshFilter.gameObject == gameObject) continue;

            // Check if it has a HullObject component
            if (meshFilter.GetComponent<HullObject>() != null) continue;

            // Check if it has a RecursiveHullObject component on itself or its parents, ignoring the current component's GameObject
            if (HasComponentInParent<RecursiveHullObject>(meshFilter.transform, gameObject))
            {
                continue;
            }

            // If none of the conditions are met, add the transform
            childTransforms.Add(meshFilter.transform);
        }

        return childTransforms;
    }

    // Check if any parent up to the root that is not the gameobject this script ison) has the specified component, ignoring a specific GameObject
    bool HasComponentInParent<T>(Transform transform, GameObject ignoreObject) where T : Component
    {
        Transform current = transform.parent;  // Start checking from the parent to respect the ignoreObject
        while (current != null && current != ignoreObject.transform)
        {
            if (current.GetComponent<T>() != null )
            {
                return true;
            }
            current = current.parent;
        }
        return false;
    }
    public void CreateHull(Transform targetTransform)
    {
        GameObject hullInstance = new GameObject($"InvertedHull ({targetTransform.gameObject.name})");
        hullInstance.transform.SetParent(targetTransform, false);

        MeshFilter targetMeshFilter = targetTransform.GetComponent<MeshFilter>();
        if (targetMeshFilter != null)
        {
            MeshFilter hullMeshFilter = hullInstance.AddComponent<MeshFilter>();
            hullMeshFilter.mesh = Instantiate(targetMeshFilter.mesh);
            MeshRenderer hullRenderer = hullInstance.AddComponent<MeshRenderer>();
            hullRenderer.material = materialCopy;
          //  hullInstance.transform.localScale = CalculateNonUniformScale(targetMeshFilter, scale);
            allHullInstances.Add(hullInstance);
        }
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

    public Vector3 CalculateNonUniformScale(MeshFilter meshFilter, float thickness)
    {
        if (meshFilter == null || meshFilter.sharedMesh == null)
        {
            Debug.LogError("MeshFilter or Mesh not found on the object.");
            return Vector3.one;
        }

        Bounds bounds = meshFilter.sharedMesh.bounds;
        Vector3 scaledBoundsSize = Vector3.Scale(bounds.size, meshFilter.gameObject.transform.localScale); // Apply scale to bounds size

        float maxDimension = Mathf.Max(scaledBoundsSize.x, Mathf.Max(scaledBoundsSize.y, scaledBoundsSize.z));

        // Adjust scale factors based on the scaled bounds of the object
        float scaleX = 1 + ((thickness - 1) / (scaledBoundsSize.x ));
        float scaleY = 1 + ((thickness - 1) / (scaledBoundsSize.y ));
        float scaleZ = 1 + ((thickness - 1) / (scaledBoundsSize.z ));

        return new Vector3(scaleX, scaleY, scaleZ);
    }

    void OnDestroy()
    {
        foreach (GameObject hull in allHullInstances)
        {
            if (hull != null)
            {
                Destroy(hull);
            }
        }
        allHullInstances.Clear();
    }
}
