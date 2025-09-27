using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.SceneManagement;

public class MainMenuManager : MonoBehaviour
{
   [Header("Menu UI Elements")]
    [SerializeField] private GameObject mainMenuPanel;
    [SerializeField] private Button playButton;
    [SerializeField] private Button quitButton;
    
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
    
    [Header("Android Specific")]
    [SerializeField] private bool useAndroidBackButton = true;
    [SerializeField] private GameObject quitConfirmationPanel;
    [SerializeField] private Button confirmQuitButton;
    [SerializeField] private Button cancelQuitButton;
    
    private bool isQuitting = false;
    
    private void Start()
    {
        InitializeMenu();
        SetupButtonListeners();
        SetupAndroidControls();
    }
    
    private void Update()
    {
        HandleAndroidBackButton();
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
        
        if (quitConfirmationPanel != null)
        {
            quitConfirmationPanel.SetActive(false);
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
            AddTouchEffects(playButton);
        }
        
        if (quitButton != null)
        {
            quitButton.onClick.AddListener(OnQuitButtonClicked);
            AddTouchEffects(quitButton);
        }
        
        if (confirmQuitButton != null)
        {
            confirmQuitButton.onClick.AddListener(OnConfirmQuit);
            AddTouchEffects(confirmQuitButton);
        }
        
        if (cancelQuitButton != null)
        {
            cancelQuitButton.onClick.AddListener(OnCancelQuit);
            AddTouchEffects(cancelQuitButton);
        }
    }
    
    private void SetupAndroidControls()
    {
        Application.targetFrameRate = 60;
        Screen.sleepTimeout = SleepTimeout.NeverSleep;
    }
    
    private void HandleAndroidBackButton()
    {
        if (useAndroidBackButton && Input.GetKeyDown(KeyCode.Escape))
        {
            if (quitConfirmationPanel != null && quitConfirmationPanel.activeInHierarchy)
            {
                OnCancelQuit();
            }
            else
            {
                OnQuitButtonClicked();
            }
        }
    }
    
    #region Button Click Handlers
    
    public void OnPlayButtonClicked()
    {
        if (isQuitting) return;
        
        PlayClickSound();
        StartCoroutine(LoadGameScene());
    }
    
    public void OnQuitButtonClicked()
    {
        PlayClickSound();
        
        if (quitConfirmationPanel != null)
        {
            quitConfirmationPanel.SetActive(true);
            mainMenuPanel.SetActive(false);
        }
        else
        {
            StartCoroutine(QuitApplication());
        }
    }
    
    public void OnConfirmQuit()
    {
        PlayClickSound();
        StartCoroutine(QuitApplication());
        Invoke("ForceQuit", 2f);
    }
    
    private void ForceQuit()
    {
        if (isQuitting)
        {
            #if UNITY_EDITOR
                UnityEditor.EditorApplication.isPlaying = false;
            #else
                Application.Quit();
            #endif
        }
    }
    
    public void OnCancelQuit()
    {
        PlayClickSound();
        
        if (quitConfirmationPanel != null)
        {
            quitConfirmationPanel.SetActive(false);
        }
        
        if (mainMenuPanel != null)
        {
            mainMenuPanel.SetActive(true);
        }
    }
    
    #endregion
    
    #region Scene Management
    
    private IEnumerator LoadGameScene()
    {
        SetButtonsInteractable(false);
        yield return StartCoroutine(FadeOut());
        yield return new WaitForSeconds(sceneTransitionDelay);
        SceneManager.LoadScene(gameSceneName);
    }
    
    private IEnumerator QuitApplication()
    {
        isQuitting = true;
        
        SetButtonsInteractable(false);
        yield return StartCoroutine(FadeOut());
        yield return new WaitForSeconds(sceneTransitionDelay);
        
        #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
        #else
            Application.Quit();
        #endif
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
    
    #region Android Touch Effects
    
    private void AddTouchEffects(Button button)
    {
        if (button == null) return;
        
        AndroidButtonEffect effect = button.GetComponent<AndroidButtonEffect>();
        if (effect == null)
        {
            effect = button.gameObject.AddComponent<AndroidButtonEffect>();
        }
        
        effect.SetTouchFeedback(true);
    }
    
    private void SetButtonsInteractable(bool interactable)
    {
        if (playButton != null) playButton.interactable = interactable;
        if (quitButton != null) quitButton.interactable = interactable;
        if (confirmQuitButton != null) confirmQuitButton.interactable = interactable;
        if (cancelQuitButton != null) cancelQuitButton.interactable = interactable;
    }
    
    #endregion
    
    #region Public Methods
    
    public void LoadScene(string sceneName)
    {
        if (!isQuitting)
        {
            StartCoroutine(LoadSceneWithFade(sceneName));
        }
    }
    
    private IEnumerator LoadSceneWithFade(string sceneName)
    {
        SetButtonsInteractable(false);
        yield return StartCoroutine(FadeOut());
        yield return new WaitForSeconds(sceneTransitionDelay);
        SceneManager.LoadScene(sceneName);
    }
    
    public void RestartGame()
    {
        if (!isQuitting)
        {
            StartCoroutine(LoadSceneWithFade(gameSceneName));
        }
    }
    
    public void ReturnToMainMenu()
    {
        if (!isQuitting)
        {
            StartCoroutine(LoadSceneWithFade("MainMenu"));
        }
    }
    
    #endregion
    
    #region Android Lifecycle
    
    private void OnApplicationPause(bool pauseStatus)
    {
    }
    
    private void OnApplicationFocus(bool hasFocus)
    {
    }
    
    #endregion
}

// Android-optimized button effect component
public class AndroidButtonEffect : MonoBehaviour
{
    private Vector3 originalScale;
    private bool touchFeedbackEnabled = true;
    
    [Header("Mobile Touch Settings")]
    [SerializeField] private float pressScale = 0.95f;
    [SerializeField] private float animationSpeed = 8f;
    [SerializeField] private bool vibrateOnTouch = true;
    
    private Coroutine scaleCoroutine;
    
    private void Start()
    {
        originalScale = transform.localScale;
        
        Button button = GetComponent<Button>();
        if (button != null)
        {
            button.onClick.AddListener(OnButtonPressed);
        }
    }
    
    public void SetTouchFeedback(bool enabled)
    {
        touchFeedbackEnabled = enabled;
    }
    
    public void OnButtonPressed()
    {
        if (!touchFeedbackEnabled) return;
        
        if (vibrateOnTouch)
        {
            #if UNITY_ANDROID && !UNITY_EDITOR
                Handheld.Vibrate();
            #endif
        }
        if (scaleCoroutine != null)
        {
            StopCoroutine(scaleCoroutine);
        }
        scaleCoroutine = StartCoroutine(ButtonPressEffect());
    }
    
    private IEnumerator ButtonPressEffect()
    {
        Vector3 pressedScale = originalScale * pressScale;
        float elapsed = 0f;
        Vector3 startScale = transform.localScale;
        
        while (elapsed < 0.1f)
        {
            elapsed += Time.deltaTime;
            float progress = elapsed / 0.1f;
            transform.localScale = Vector3.Lerp(startScale, pressedScale, progress);
            yield return null;
        }
        
        elapsed = 0f;
        while (elapsed < 0.1f)
        {
            elapsed += Time.deltaTime;
            float progress = elapsed / 0.1f;
            transform.localScale = Vector3.Lerp(pressedScale, originalScale, progress);
            yield return null;
        }
        
        transform.localScale = originalScale;
    }
}
