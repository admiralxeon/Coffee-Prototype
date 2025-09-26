using UnityEngine;
using TMPro;

public class InteractionPromptUI : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private GameObject promptPanel;
    [SerializeField] private TextMeshProUGUI promptText;
    
    [Header("Settings")]
    [SerializeField] private float fadeSpeed = 5f;
    
    private CanvasGroup canvasGroup;
    private bool isVisible = false;
    
    private void Awake()
    {
        ValidateReferences();
        SetupCanvasGroup();
        ConnectToPlayerInteraction();
        SetInitialState();
    }
    
    private void ValidateReferences()
    {
        if (promptPanel == null)
        {
            Debug.LogError("PromptPanel reference is NULL! Please assign it in the inspector.");
            return;
        }
        
        if (promptText == null)
        {
            Debug.LogError("PromptText reference is NULL! Please assign it in the inspector.");
            return;
        }
    }
    
    private void SetupCanvasGroup()
    {
        canvasGroup = promptPanel?.GetComponent<CanvasGroup>();
        
        if (canvasGroup == null)
        {
            canvasGroup = GetComponent<CanvasGroup>();
        }
        
        if (canvasGroup == null && promptPanel != null)
        {
            canvasGroup = promptPanel.AddComponent<CanvasGroup>();
        }
        
        if (canvasGroup == null)
        {
            canvasGroup = gameObject.AddComponent<CanvasGroup>();
        }
    }
    
    private void ConnectToPlayerInteraction()
    {
        PlayerInteraction playerInteraction = FindObjectOfType<PlayerInteraction>();
        if (playerInteraction != null)
        {
            playerInteraction.OnInteractionAvailable += ShowPrompt;
            playerInteraction.OnInteractionUnavailable += HidePrompt;
        }
        else
        {
            Debug.LogError("No PlayerInteraction found in scene!");
        }
    }
    
    private void SetInitialState()
    {
        if (canvasGroup != null)
        {
            canvasGroup.alpha = 0f;
        }
        
        if (promptPanel != null)
        {
            promptPanel.SetActive(true);
        }
    }
    
    public void ShowPrompt(string text)
    {
        if (promptPanel == null)
        {
            Debug.LogError("Cannot show prompt - promptPanel is null!");
            return;
        }
        
        if (promptText == null)
        {
            Debug.LogError("Cannot show prompt - promptText is null!");
            return;
        }
        
        promptPanel.SetActive(true);
        promptText.text = text;
        isVisible = true;
        StopAllCoroutines();
        StartCoroutine(FadeToAlpha(1f));
    }
    
    public void HidePrompt()
    {
        isVisible = false;
        StopAllCoroutines();
        StartCoroutine(FadeToAlpha(0f));
    }
    
    private System.Collections.IEnumerator FadeToAlpha(float targetAlpha)
    {
        if (canvasGroup == null)
        {
            Debug.LogError("Cannot fade - canvasGroup is null!");
            yield break;
        }
        
        float startAlpha = canvasGroup.alpha;
        float elapsed = 0f;
        float duration = 1f / fadeSpeed;
        
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float progress = elapsed / duration;
            float currentAlpha = Mathf.Lerp(startAlpha, targetAlpha, progress);
            canvasGroup.alpha = currentAlpha;
            yield return null;
        }
        
        canvasGroup.alpha = targetAlpha;
    }
    
}
