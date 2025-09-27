using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;

public class InGameMenuManager : MonoBehaviour
{
    [Header("UI Elements")]
    [SerializeField] private Button mainMenuButton;
    
    [Header("Scene Management")]
    [SerializeField] private string mainMenuSceneName = "Main";
    [SerializeField] private float sceneTransitionDelay = 0.5f;
    
    [Header("Audio")]
    [SerializeField] private AudioClip buttonClickSound;
    private AudioSource audioSource;
    
    [Header("Visual Effects")]
    [SerializeField] private CanvasGroup fadePanel;
    [SerializeField] private float fadeSpeed = 2f;
    
    private bool isTransitioning = false;
    
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
        
        if (fadePanel != null)
        {
            fadePanel.alpha = 0f;
            fadePanel.gameObject.SetActive(false);
        }
    }
    
    private void SetupButtonListeners()
    {
        if (mainMenuButton != null)
        {
            mainMenuButton.onClick.AddListener(OnMainMenuButtonClicked);
        }
    }
    
    public void OnMainMenuButtonClicked()
    {
        if (isTransitioning) return;
        
        PlayClickSound();
        StartCoroutine(LoadMainMenuScene());
    }
    
    private IEnumerator LoadMainMenuScene()
    {
        isTransitioning = true;
        
        if (mainMenuButton != null)
        {
            mainMenuButton.interactable = false;
        }
        
        yield return StartCoroutine(FadeOut());
        yield return new WaitForSeconds(sceneTransitionDelay);
        SceneManager.LoadScene(mainMenuSceneName);
    }
    
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
    
    private void PlayClickSound()
    {
        if (audioSource != null && buttonClickSound != null)
        {
            audioSource.PlayOneShot(buttonClickSound);
        }
    }
}
