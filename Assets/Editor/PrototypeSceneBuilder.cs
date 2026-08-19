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
        private const string ConfigurationObjectName =
            "Prototype asset configuration — assign prefabs here";

        private struct AssetConfigurationSnapshot
        {
            public bool hasValue;
            public GameObject roadChunkPrefab;
            public GameObject pineTreePrefab;
            public GameObject leaflessTreePrefab;
            public GameObject trafficCarPrefab;
            public GameObject barricadePrefab;
            public float roadChunkLength;
        }

        [InitializeOnLoadMethod]
        private static void ScheduleInitialSceneCreation()
        {
            EditorApplication.delayCall += EnsureSceneExists;
        }

        [MenuItem("Tools/The Last Passenger/Rebuild Prototype Scene")]
        public static void RebuildScene()
        {
            CreateAndSaveScene(openAfterCreation: true);
            Debug.Log("The Last Passenger prototype scene rebuilt. Existing prefab overrides were preserved.");
        }

        [MenuItem("Tools/The Last Passenger/Open Prototype Scene")]
        public static void OpenScene()
        {
            EnsureSceneExists();
            EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
        }

        [MenuItem("Tools/The Last Passenger/Select Prefab Overrides")]
        public static void SelectPrefabOverrides()
        {
            EnsureSceneExists();
            Scene scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
            PrototypeAssetConfiguration configuration = FindConfiguration(scene);
            if (configuration == null) return;

            Selection.activeGameObject = configuration.gameObject;
            EditorGUIUtility.PingObject(configuration.gameObject);
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
                EnsureConfigurationExists();
                EnsureBuildSettings();
            }
        }

        private static void CreateAndSaveScene(bool openAfterCreation)
        {
            AssetConfigurationSnapshot previousConfiguration = CaptureConfiguration();
            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            GameObject note = new GameObject("Scene generated at runtime — see README");
            note.transform.position = Vector3.zero;

            PrototypeAssetConfiguration configuration = CreateConfiguration(scene);
            if (previousConfiguration.hasValue)
            {
                configuration.ApplyEditorOverrides(
                    previousConfiguration.roadChunkPrefab,
                    previousConfiguration.pineTreePrefab,
                    previousConfiguration.leaflessTreePrefab,
                    previousConfiguration.trafficCarPrefab,
                    previousConfiguration.barricadePrefab,
                    previousConfiguration.roadChunkLength);
            }

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene, ScenePath);
            EnsureBuildSettings();
            AssetDatabase.SaveAssets();

            if (openAfterCreation)
            {
                EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
            }
        }

        private static void EnsureConfigurationExists()
        {
            Scene scene = SceneManager.GetSceneByPath(ScenePath);
            bool closeWhenFinished = !scene.IsValid() || !scene.isLoaded;
            if (closeWhenFinished)
            {
                scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Additive);
            }

            if (FindConfiguration(scene) == null)
            {
                CreateConfiguration(scene);
                EditorSceneManager.MarkSceneDirty(scene);
                EditorSceneManager.SaveScene(scene);
            }

            if (closeWhenFinished)
            {
                EditorSceneManager.CloseScene(scene, removeScene: true);
            }
        }

        private static PrototypeAssetConfiguration CreateConfiguration(Scene scene)
        {
            GameObject configurationObject = new GameObject(ConfigurationObjectName);
            if (configurationObject.scene != scene)
            {
                SceneManager.MoveGameObjectToScene(configurationObject, scene);
            }
            return configurationObject.AddComponent<PrototypeAssetConfiguration>();
        }

        private static PrototypeAssetConfiguration FindConfiguration(Scene scene)
        {
            if (!scene.IsValid() || !scene.isLoaded) return null;

            GameObject[] roots = scene.GetRootGameObjects();
            for (int i = 0; i < roots.Length; i++)
            {
                PrototypeAssetConfiguration configuration =
                    roots[i].GetComponentInChildren<PrototypeAssetConfiguration>(true);
                if (configuration != null) return configuration;
            }

            return null;
        }

        private static AssetConfigurationSnapshot CaptureConfiguration()
        {
            if (AssetDatabase.LoadAssetAtPath<SceneAsset>(ScenePath) == null)
            {
                return default;
            }

            Scene scene = SceneManager.GetSceneByPath(ScenePath);
            bool closeWhenFinished = !scene.IsValid() || !scene.isLoaded;
            if (closeWhenFinished)
            {
                scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Additive);
            }

            PrototypeAssetConfiguration configuration = FindConfiguration(scene);
            AssetConfigurationSnapshot snapshot = default;
            if (configuration != null)
            {
                snapshot.hasValue = true;
                snapshot.roadChunkPrefab = configuration.RoadChunkPrefab;
                snapshot.pineTreePrefab = configuration.PineTreePrefab;
                snapshot.leaflessTreePrefab = configuration.LeaflessTreePrefab;
                snapshot.trafficCarPrefab = configuration.TrafficCarPrefab;
                snapshot.barricadePrefab = configuration.BarricadePrefab;
                snapshot.roadChunkLength = configuration.RoadChunkLength;
            }

            if (closeWhenFinished)
            {
                EditorSceneManager.CloseScene(scene, removeScene: true);
            }

            return snapshot;
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
