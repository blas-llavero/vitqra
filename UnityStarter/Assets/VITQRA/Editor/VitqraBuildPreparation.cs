using System;
using UnityEditor;
using UnityEngine;

namespace Vitqra.Editor
{
    public static class VitqraBuildPreparation
    {
        // Invoked by Unity only after the project scripts have been imported and compiled.
        public static void VerifyBatch()
        {
            if (!Application.isBatchMode)
                throw new InvalidOperationException("Run this check with Unity's -batchmode argument.");

            Debug.Log($"VITQRA_IMPORT_OK unity={Application.unityVersion} target={EditorUserBuildSettings.activeBuildTarget}");
            EditorApplication.Exit(0);
        }
    }
}
