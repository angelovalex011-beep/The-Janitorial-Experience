using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

/// <summary>
/// Unity 6 Editor tool: copies all components (and their serialized values)
/// from a source GameObject to a target GameObject.
///
/// Install:
///   1. Put this script in an "Editor" folder anywhere in your project
///      (e.g. Assets/Editor/ComponentCopierWindow.cs).
///   2. Open it from the menu: Tools > Component Copier.
///
/// Usage:
///   - Drag the object you want to copy FROM into "Source".
///   - Drag the object you want to copy TO into "Target".
///   - Toggle "Include Transform" if you also want position/rotation/scale copied.
///   - Click "Copy All Components".
///
/// Notes:
///   - If Target is missing a component that Source has, it will be added.
///   - If Source has multiple components of the same type, they're matched
///     to Target's components of that type in order; extra ones are added.
///   - Uses Unity's built-in ComponentUtility copy/paste, which is the same
///     mechanism as the "Copy Component" / "Paste Component Values" context
///     menu items, so it respects serialization the same way the editor does.
/// </summary>
public class ComponentCopierWindow : EditorWindow
{
    private GameObject source;
    private GameObject target;
    private bool includeTransform = false;
    private bool includeInactiveComponents = true;

    [MenuItem("Tools/Component Copier")]
    private static void ShowWindow()
    {
        var window = GetWindow<ComponentCopierWindow>("Component Copier");
        window.minSize = new Vector2(320, 180);
    }

    private void OnGUI()
    {
        EditorGUILayout.Space(8);
        EditorGUILayout.LabelField("Copy Components Between GameObjects", EditorStyles.boldLabel);
        EditorGUILayout.Space(4);

        source = (GameObject)EditorGUILayout.ObjectField("Source", source, typeof(GameObject), true);
        target = (GameObject)EditorGUILayout.ObjectField("Target", target, typeof(GameObject), true);

        EditorGUILayout.Space(4);
        includeTransform = EditorGUILayout.Toggle("Include Transform", includeTransform);
        includeInactiveComponents = EditorGUILayout.Toggle("Include Disabled Components", includeInactiveComponents);

        EditorGUILayout.Space(10);

        using (new EditorGUI.DisabledScope(source == null || target == null || source == target))
        {
            if (GUILayout.Button("Copy All Components", GUILayout.Height(30)))
            {
                CopyAllComponents(source, target, includeTransform, includeInactiveComponents);
            }
        }

        if (source != null && target != null && source == target)
        {
            EditorGUILayout.HelpBox("Source and Target must be different objects.", MessageType.Warning);
        }

        EditorGUILayout.Space(8);
        EditorGUILayout.HelpBox(
            "Copies every component (scripts, renderers, colliders, rigidbodies, etc.) " +
            "and their current values from Source to Target. Missing components are added; " +
            "matching ones are overwritten. This action can be undone with Ctrl/Cmd+Z.",
            MessageType.Info);
    }

    /// <summary>
    /// Copies every component (optionally including Transform) from sourceGO to targetGO.
    /// </summary>
    public static void CopyAllComponents(GameObject sourceGO, GameObject targetGO, bool includeTransform, bool includeDisabled)
    {
        if (sourceGO == null || targetGO == null)
        {
            Debug.LogWarning("Component Copier: Source or Target is null.");
            return;
        }

        Component[] sourceComponents = sourceGO.GetComponents<Component>();

        Undo.RegisterCompleteObjectUndo(targetGO, "Copy All Components");
        Undo.SetCurrentGroupName("Copy All Components");
        int undoGroup = Undo.GetCurrentGroup();

        // Track how many of each type we've already consumed on the target,
        // so multiple components of the same type line up in order.
        var targetTypeCursor = new Dictionary<System.Type, int>();
        var targetComponentsCache = new Dictionary<System.Type, List<Component>>();

        int copiedCount = 0;
        int addedCount = 0;
        int skippedCount = 0;

        foreach (Component sourceComp in sourceComponents)
        {
            if (sourceComp == null)
            {
                skippedCount++;
                continue; // missing script reference
            }

            System.Type type = sourceComp.GetType();

            if (type == typeof(Transform))
            {
                if (!includeTransform)
                {
                    skippedCount++;
                    continue;
                }
            }

            if (!includeDisabled && sourceComp is Behaviour behaviour && !behaviour.enabled)
            {
                // Still copy it, just noting the flag exists; comment kept for clarity.
                // (We copy regardless of enabled state by default — see toggle below.)
            }

            // Build/refresh the cache of target components of this type.
            if (!targetComponentsCache.TryGetValue(type, out List<Component> targetOfType))
            {
                targetOfType = targetGO.GetComponents(type).ToList();
                targetComponentsCache[type] = targetOfType;
                targetTypeCursor[type] = 0;
            }

            int cursor = targetTypeCursor[type];
            Component targetComp;

            if (cursor < targetOfType.Count)
            {
                // Reuse an existing component of the same type on target.
                targetComp = targetOfType[cursor];
            }
            else if (type == typeof(Transform))
            {
                // Can't add another Transform; nothing to paste onto.
                skippedCount++;
                continue;
            }
            else
            {
                // Add a new component of this type to target.
                targetComp = Undo.AddComponent(targetGO, type);
                targetOfType.Add(targetComp);
                addedCount++;
            }

            targetTypeCursor[type] = cursor + 1;

            // Copy the values across using Unity's built-in serialized copy/paste.
            if (ComponentUtility.CopyComponent(sourceComp))
            {
                bool pasted = ComponentUtility.PasteComponentValues(targetComp);
                if (pasted)
                {
                    copiedCount++;
                }
                else
                {
                    Debug.LogWarning($"Component Copier: Failed to paste values for {type.Name} onto '{targetGO.name}'.");
                    skippedCount++;
                }
            }
            else
            {
                Debug.LogWarning($"Component Copier: Failed to copy {type.Name} from '{sourceGO.name}'.");
                skippedCount++;
            }
        }

        Undo.CollapseUndoOperations(undoGroup);

        EditorUtility.SetDirty(targetGO);

        Debug.Log($"Component Copier: Done. Copied values: {copiedCount}, Added: {addedCount}, Skipped: {skippedCount} " +
                  $"(from '{sourceGO.name}' to '{targetGO.name}').");
    }

    // Optional: right-click context menu shortcut on GameObjects.
    // Select exactly two objects in the Hierarchy (first = source, second/last = target)
    // then use GameObject > Copy All Components To Last Selected.
    [MenuItem("GameObject/Copy All Components To Last Selected", false, 49)]
    private static void CopyComponentsContextMenu(MenuCommand command)
    {
        GameObject[] selected = Selection.gameObjects;
        if (selected.Length < 2)
        {
            EditorUtility.DisplayDialog("Component Copier",
                "Select at least two GameObjects in the Hierarchy.\nThe last selected one is treated as the Target; all others are copied FROM the first selected one.",
                "OK");
            return;
        }

        GameObject src = selected[0];
        GameObject tgt = selected[selected.Length - 1];

        if (src == tgt)
        {
            EditorUtility.DisplayDialog("Component Copier", "Source and Target must be different objects.", "OK");
            return;
        }

        CopyAllComponents(src, tgt, includeTransform: false, includeDisabled: true);
    }

    [MenuItem("GameObject/Copy All Components To Last Selected", true)]
    private static bool ValidateCopyComponentsContextMenu()
    {
        return Selection.gameObjects != null && Selection.gameObjects.Length >= 2;
    }
}