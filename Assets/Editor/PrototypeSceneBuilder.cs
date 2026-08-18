#if UNITY_EDITOR
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace LastPassenger.Editor
{
    public static class PrototypeSceneBuilder
    {
        private const string ScenePath = "Assets/Scenes/Prototype.unity";

        [InitializeOnLoadMethod]
        private static void ScheduleInitialSceneCreation()
        {
            EditorApplication.delayCall += EnsureSceneExists;
        }

        [MenuItem("Tools/The Last Passenger/Rebuild Prototype Scene")]
        public static void RebuildScene()
        {
            CreateAndSaveScene(openAfterCreation: true);
            Debug.Log("The Last Passenger prototype scene rebuilt. Press Play to run the generated vertical slice.");
        }

        [MenuItem("Tools/The Last Passenger/Open Prototype Scene")]
        public static void OpenScene()
        {
            EnsureSceneExists();
            EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
        }

        private static void EnsureSceneExists()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode) return;

            SceneAsset sceneAsset = AssetDatabase.LoadAssetAtPath<SceneAsset>(ScenePath);
            if (sceneAsset == null)
            {
                CreateAndSaveScene(openAfterCreation: true);
            }
            else
            {
                EnsureBuildSettings();
            }
        }

        private static void CreateAndSaveScene(bool openAfterCreation)
        {
            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            GameObject note = new GameObject("Scene generated at runtime — see README");
            note.transform.position = Vector3.zero;
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene, ScenePath);
            EnsureBuildSettings();
            AssetDatabase.SaveAssets();

            if (openAfterCreation)
            {
                EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
            }
        }

        private static void EnsureBuildSettings()
        {
            EditorBuildSettingsScene[] existing = EditorBuildSettings.scenes;
            if (existing.Any(scene => scene.path == ScenePath && scene.enabled)) return;

            EditorBuildSettings.scenes = existing
                .Where(scene => scene.path != ScenePath)
                .Concat(new[] { new EditorBuildSettingsScene(ScenePath, true) })
                .ToArray();
        }
    }
}
#endif
