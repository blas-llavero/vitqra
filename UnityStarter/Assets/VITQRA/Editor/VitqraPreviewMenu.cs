using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace Vitqra.Editor
{
    public static class VitqraPreviewMenu
    {
        [MenuItem("VITQRA/Create Desktop Preview Scene")]
        public static void Create()
        {
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
            var scene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);
            var root = new GameObject("VITQRA Desktop Preview");
            root.AddComponent<VitqraPreview>();
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
        }
    }
}
