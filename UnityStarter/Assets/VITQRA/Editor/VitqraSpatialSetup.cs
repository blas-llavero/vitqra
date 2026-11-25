using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace Vitqra.Editor
{
    public static class VitqraSpatialSetup
    {
        [MenuItem("VITQRA/Add Spatial Workspace to Selected XR Camera")]
        public static void Add()
        {
            var selected = Selection.activeGameObject;
            var camera = selected != null ? selected.GetComponent<Camera>() : null;
            if (camera == null || camera.transform.parent == null || EditorUtility.IsPersistent(selected))
            {
                EditorUtility.DisplayDialog("VITQRA", "Select the tracked Main Camera inside an XR Origin in the open scene.", "OK");
                return;
            }
            if (Object.FindFirstObjectByType<VitqraSpatialWorkspace>(FindObjectsInactive.Include) != null)
            {
                EditorUtility.DisplayDialog("VITQRA", "This scene already contains a spatial workspace.", "OK");
                return;
            }
            var root = new GameObject("VITQRA Spatial Workspace");
            Undo.RegisterCreatedObjectUndo(root, "Add VITQRA workspace");
            var workspace = Undo.AddComponent<VitqraSpatialWorkspace>(root);
            workspace.Configure(camera, camera.transform.parent);
            EditorUtility.SetDirty(workspace);
            EditorSceneManager.MarkSceneDirty(root.scene);
            Selection.activeGameObject = root;
        }
    }
}
