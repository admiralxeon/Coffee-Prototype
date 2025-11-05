using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.SceneManagement;

public class MainMenuManager : MonoBehaviour
{
    [Header("Menu UI Elements")]
    [SerializeField] private GameObject mainMenuPanel;
    [SerializeField] private Button playButton;

    [Header("Scene Management")]
    [SerializeField] private string gameSceneName = "GameScene";
    [SerializeField] private float sceneTransitionDelay = 0.5f;

    [Header("Audio")]
    [SerializeField] private AudioClip buttonClickSound;
    [SerializeField] private AudioClip buttonHoverSound;
    private AudioSource audioSource;

    [Header("Visual Effects")]
    [SerializeField] private CanvasGroup fadePanel;
    [SerializeField] private float fadeSpeed = 2f;

    private void Start()
    {
        InitializeMenu();
        SetupButtonListeners();
    }

    private void InitializeMenu()
    {
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }

        if (mainMenuPanel != null)
        {
            mainMenuPanel.SetActive(true);
        }

        if (fadePanel != null)
        {
            fadePanel.alpha = 0f;
            fadePanel.gameObject.SetActive(false);
        }
    }

    private void SetupButtonListeners()
    {
        if (playButton != null)
        {
            playButton.onClick.AddListener(OnPlayButtonClicked);
        }
    }

    #region Button Click Handlers

    public void OnPlayButtonClicked()
    {
        Debug.Log("Play button clicked!");
        PlayClickSound();
        StartCoroutine(LoadGameScene());
    }

    #endregion

    #region Scene Management

    private IEnumerator LoadGameScene()
    {
        SetButtonsInteractable(false);

        // Validate scene exists before attempting to load
        if (!DoesSceneExist(gameSceneName))
        {
            Debug.LogError($"Scene '{gameSceneName}' not found in build settings! Please add the scene to File > Build Settings > Scenes In Build.\n{GetAvailableScenesList()}");
            SetButtonsInteractable(true); // Re-enable buttons so user can try again
            yield break;
        }

        Debug.Log($"Loading scene: {gameSceneName}");
        yield return StartCoroutine(FadeOut());
        yield return new WaitForSeconds(sceneTransitionDelay);

        try
        {
            SceneManager.LoadScene(gameSceneName);
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Failed to load scene '{gameSceneName}': {e.Message}");
            SetButtonsInteractable(true);
        }
    }

    private bool DoesSceneExist(string sceneName)
    {
        if (string.IsNullOrEmpty(sceneName))
        {
            return false;
        }

        // Check if scene exists in build settings
        for (int i = 0; i < SceneManager.sceneCountInBuildSettings; i++)
        {
            string scenePath = SceneUtility.GetScenePathByBuildIndex(i);
            string sceneNameFromPath = System.IO.Path.GetFileNameWithoutExtension(scenePath);

            if (sceneNameFromPath == sceneName)
            {
                return true;
            }
        }

        return false;
    }

    private string GetAvailableScenesList()
    {
        System.Text.StringBuilder sceneList = new System.Text.StringBuilder();
        sceneList.Append("Available scenes in build settings:\n");

        for (int i = 0; i < SceneManager.sceneCountInBuildSettings; i++)
        {
            string scenePath = SceneUtility.GetScenePathByBuildIndex(i);
            string sceneNameFromPath = System.IO.Path.GetFileNameWithoutExtension(scenePath);
            sceneList.Append($"  - {sceneNameFromPath}\n");
        }

        return sceneList.ToString();
    }

    #endregion

    #region Visual Effects

    private IEnumerator FadeOut()
    {
        if (fadePanel == null) yield break;

        fadePanel.gameObject.SetActive(true);

        float startAlpha = fadePanel.alpha;
        float elapsed = 0f;
        float duration = 1f / fadeSpeed;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float progress = elapsed / duration;
            fadePanel.alpha = Mathf.Lerp(startAlpha, 1f, progress);
            yield return null;
        }

        fadePanel.alpha = 1f;
    }

    private IEnumerator FadeIn()
    {
        if (fadePanel == null) yield break;

        float startAlpha = fadePanel.alpha;
        float elapsed = 0f;
        float duration = 1f / fadeSpeed;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float progress = elapsed / duration;
            fadePanel.alpha = Mathf.Lerp(startAlpha, 0f, progress);
            yield return null;
        }

        fadePanel.alpha = 0f;
        fadePanel.gameObject.SetActive(false);
    }

    #endregion

    #region Audio

    private void PlayClickSound()
    {
        if (audioSource != null && buttonClickSound != null)
        {
            audioSource.PlayOneShot(buttonClickSound);
        }
    }

    private void PlayHoverSound()
    {
        if (audioSource != null && buttonHoverSound != null)
        {
            audioSource.PlayOneShot(buttonHoverSound);
        }
    }

    #endregion

    #region Button Management

    private void SetButtonsInteractable(bool interactable)
    {
        if (playButton != null) playButton.interactable = interactable;
    }

    #endregion

    #region Public Methods

    public void LoadScene(string sceneName)
    {
        StartCoroutine(LoadSceneWithFade(sceneName));
    }

    private IEnumerator LoadSceneWithFade(string sceneName)
    {
        SetButtonsInteractable(false);

        // Validate scene exists before attempting to load
        if (!DoesSceneExist(sceneName))
        {
            Debug.LogError($"Scene '{sceneName}' not found in build settings! Please add the scene to File > Build Settings > Scenes In Build.\n{GetAvailableScenesList()}");
            SetButtonsInteractable(true);
            yield break;
        }

        Debug.Log($"Loading scene: {sceneName}");
        yield return StartCoroutine(FadeOut());
        yield return new WaitForSeconds(sceneTransitionDelay);

        try
        {
            SceneManager.LoadScene(sceneName);
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Failed to load scene '{sceneName}': {e.Message}");
            SetButtonsInteractable(true);
        }
    }

    public void RestartGame()
    {
        StartCoroutine(LoadSceneWithFade(gameSceneName));
    }

    public void ReturnToMainMenu()
    {
        StartCoroutine(LoadSceneWithFade("MainMenu"));
    }

    #endregion
}
