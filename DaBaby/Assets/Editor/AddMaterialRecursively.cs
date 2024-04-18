using UnityEngine;
using UnityEditor;

public class AddMaterialRecursively : EditorWindow
{
    private Material materialToAdd;

    [MenuItem("Tools/Add Material Recursively")]
    public static void ShowWindow()
    {
        EditorWindow window = GetWindow(typeof(AddMaterialRecursively));
        window.titleContent = new GUIContent("Add Material");
        window.Show();
    }

    void OnGUI()
    {
        GUILayout.Label("Add Material Recursively", EditorStyles.boldLabel);
        materialToAdd = EditorGUILayout.ObjectField("Material:", materialToAdd, typeof(Material), true) as Material;

        if (GUILayout.Button("Apply Material"))
        {
            if (materialToAdd == null)
            {
                Debug.LogError("Please assign a material first.");
                return;
            }

            GameObject[] selectedObjects = Selection.gameObjects;

            foreach (GameObject obj in selectedObjects)
            {
                Renderer[] renderers = obj.GetComponentsInChildren<Renderer>(true);

                foreach (Renderer renderer in renderers)
                {
                    renderer.material = materialToAdd;
                }
            }

            Debug.Log("Material applied to all renderers recursively.");
        }
    }
}
