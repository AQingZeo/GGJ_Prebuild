#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;

/// <summary>
/// Creates a sample InteractableDefinition asset. Menu: Tools > Create Sample Interactable Definition.
/// Edit the asset in Inspector to add rules (e.g. Add Inventory Item Action + Consume Interactable Action).
/// </summary>
public static class CreateSampleInteractableDef
{
    private const string MENU = "Tools/Create Sample Interactable Definition";
    private const string DEFAULT_PATH = "Assets/_Project/Resources/InteractableDefinitions";

    [MenuItem(MENU)]
    public static void Run()
    {
        var def = ScriptableObject.CreateInstance<InteractableDefinition>();
        def.id = "sample_pickup";
        def.interactionType = InteractionType.Both;
        def.oneShot = true;

        string dir = DEFAULT_PATH;
        if (!AssetDatabase.IsValidFolder("Assets/_Project/Resources"))
        {
            dir = "Assets/_Project";
        }
        else if (!AssetDatabase.IsValidFolder("Assets/_Project/Resources/InteractableDefinitions"))
        {
            AssetDatabase.CreateFolder("Assets/_Project/Resources", "InteractableDefinitions");
        }

        string path = AssetDatabase.GenerateUniqueAssetPath($"{dir}/SamplePickup_Def.asset");
        AssetDatabase.CreateAsset(def, path);
        AssetDatabase.SaveAssets();
        EditorGUIUtility.PingObject(def);
        Selection.activeObject = def;
        Debug.Log($"Created sample InteractableDefinition at {path}. Add rules in Inspector (e.g. Add Inventory Item Action + Consume Interactable Action).");
    }
}
#endif
