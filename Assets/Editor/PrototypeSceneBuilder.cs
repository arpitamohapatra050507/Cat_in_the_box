#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;

namespace LastPassenger.Editor
{
    public static class PrototypeSceneBuilder
    {
        private const string MenuScenePath = "Assets/Scenes/MainMenu.unity";
        private const string GameplayScenePath = "Assets/Scenes/Prototype.unity";

        [InitializeOnLoadMethod]
        private static void ScheduleBuildSettingsCheck()
        {
            EditorApplication.delayCall += EnsureBuildSettings;
        }

        [MenuItem("Tools/The Last Passenger/Open Main Menu Scene")]
        public static void OpenMainMenuScene()
        {
            EditorSceneManager.OpenScene(MenuScenePath, OpenSceneMode.Single);
        }

        [MenuItem("Tools/The Last Passenger/Open Gameplay Scene")]
        public static void OpenGameplayScene()
        {
            EditorSceneManager.OpenScene(GameplayScenePath, OpenSceneMode.Single);
        }

        [MenuItem("Tools/The Last Passenger/Repair Build Scene Order")]
        public static void EnsureBuildSettings()
        {
            EditorBuildSettings.scenes = new[]
            {
                new EditorBuildSettingsScene(MenuScenePath, true),
                new EditorBuildSettingsScene(GameplayScenePath, true)
            };
        }
    }
}
#endif
