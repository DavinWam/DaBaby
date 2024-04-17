using System.Collections.Generic;
using UnityEngine;

public class RecursiveHullObject : MonoBehaviour, IHullObject
{
    private HullMaterialConfig materialConfig;
    public float scale = 1.1f;
    public bool enableDynamicScaling = false;
    private List<GameObject> allHullInstances = new List<GameObject>();

    void Start()
    {
        materialConfig = Resources.Load<HullMaterialConfig>("Configurations/HullMaterial");
        if (materialConfig != null && materialConfig.hullMaterial != null)
        {
            List<Transform> transforms = new List<Transform>();
            transforms.Add(gameObject.transform);
            transforms.AddRange(CollectChildTransformsWithMesh());
            foreach (Transform transform in transforms)
            {
                CreateHull(transform);
            }

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
        }
    }

    List<Transform> CollectChildTransformsWithMesh()
    {
        List<Transform> childTransforms = new List<Transform>();
        MeshFilter[] meshFilters = GetComponentsInChildren<MeshFilter>(true);
        foreach (MeshFilter meshFilter in meshFilters)
        {
            if (meshFilter.gameObject != gameObject)
            {
                childTransforms.Add(meshFilter.transform);
            }
        }
        return childTransforms;
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
            hullRenderer.material = materialConfig.hullMaterial;
          //  hullInstance.transform.localScale = CalculateNonUniformScale(targetMeshFilter, scale);
            allHullInstances.Add(hullInstance);
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
