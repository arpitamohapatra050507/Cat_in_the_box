using UnityEngine;
using UnityEngine.SceneManagement;

namespace LastPassenger
{
    public sealed class MainMenuController : MonoBehaviour
    {
        [Header("Scene transition")]
        [SerializeField] private string gameplaySceneName = "Prototype";

        [Header("Menu audio")]
        [SerializeField] private AudioClip menuTheme;
        [SerializeField, Range(0f, 1f)] private float menuVolume = 0.24f;

        private bool loading;
        private float inputReadyAt;

        private void Awake()
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            inputReadyAt = Time.unscaledTime + 0.2f;
            EnsureMenuCamera();

            AudioClip theme = menuTheme != null
                ? menuTheme
                : Resources.Load<AudioClip>("Audio/MenuTheme");
            if (theme == null) return;

            AudioSource source = gameObject.AddComponent<AudioSource>();
            source.clip = theme;
            source.loop = true;
            source.spatialBlend = 0f;
            source.volume = menuVolume;
            source.Play();
        }

        private void EnsureMenuCamera()
        {
            Camera menuCamera = GetComponent<Camera>();
            if (menuCamera == null) menuCamera = Object.FindFirstObjectByType<Camera>();
            if (menuCamera == null) menuCamera = gameObject.AddComponent<Camera>();

            menuCamera.enabled = true;
            menuCamera.targetTexture = null;
            menuCamera.targetDisplay = 0;
            menuCamera.clearFlags = CameraClearFlags.SolidColor;
            menuCamera.backgroundColor = new Color(0.004f, 0.008f, 0.007f);
            if (!menuCamera.CompareTag("MainCamera")) menuCamera.tag = "MainCamera";

            AudioListener listener = menuCamera.GetComponent<AudioListener>();
            if (listener == null) listener = menuCamera.gameObject.AddComponent<AudioListener>();
            listener.enabled = true;
        }

        private void Update()
        {
            if (Time.unscaledTime < inputReadyAt) return;
            if (PrototypeInput.ConfirmPressed) PlayGame();
            if (PrototypeInput.CancelPressed) QuitGame();
        }

        public void PlayGame()
        {
            if (loading) return;
            loading = true;
            SceneManager.LoadScene(gameplaySceneName, LoadSceneMode.Single);
        }

        public void QuitGame()
        {
            Application.Quit();
        }

        private void OnGUI()
        {
            Color previousColor = GUI.color;
            GUI.color = new Color(0.004f, 0.008f, 0.007f, 0.96f);
            GUI.DrawTexture(new Rect(0f, 0f, Screen.width, Screen.height), Texture2D.whiteTexture);
            GUI.color = Color.white;

            GUIStyle title = new GUIStyle(GUI.skin.label)
            {
                alignment = TextAnchor.MiddleCenter,
                fontSize = Mathf.Clamp(Screen.height / 12, 42, 82),
                fontStyle = FontStyle.Bold,
                normal = { textColor = new Color(0.72f, 0.76f, 0.69f) }
            };
            GUIStyle subtitle = new GUIStyle(title)
            {
                fontSize = Mathf.Clamp(Screen.height / 36, 17, 28),
                fontStyle = FontStyle.Normal,
                wordWrap = true,
                normal = { textColor = new Color(0.5f, 0.56f, 0.52f) }
            };
            GUIStyle button = new GUIStyle(GUI.skin.button)
            {
                alignment = TextAnchor.MiddleCenter,
                fontSize = Mathf.Clamp(Screen.height / 32, 18, 30),
                fontStyle = FontStyle.Bold
            };
            button.normal.textColor = new Color(0.76f, 0.74f, 0.66f);
            button.hover.textColor = new Color(0.95f, 0.8f, 0.45f);

            GUI.Label(new Rect(0f, Screen.height * 0.19f, Screen.width, 110f), "THE LAST PASSENGER", title);
            GUI.Label(new Rect(Screen.width * 0.2f, Screen.height * 0.38f, Screen.width * 0.6f, 80f),
                "A road should not remember what you put in the back.", subtitle);

            float buttonWidth = Mathf.Clamp(Screen.width * 0.22f, 220f, 380f);
            float buttonHeight = Mathf.Clamp(Screen.height * 0.075f, 48f, 72f);
            float buttonX = (Screen.width - buttonWidth) * 0.5f;
            if (GUI.Button(new Rect(buttonX, Screen.height * 0.57f, buttonWidth, buttonHeight), "PLAY", button))
            {
                PlayGame();
            }
            if (GUI.Button(new Rect(buttonX, Screen.height * 0.69f, buttonWidth, buttonHeight), "QUIT", button))
            {
                QuitGame();
            }

            GUI.Label(new Rect(0f, Screen.height * 0.84f, Screen.width, 34f),
                "Enter — play     Escape — quit", subtitle);
            GUI.color = previousColor;
        }
    }
}
