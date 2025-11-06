using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class ProgressFeedbackUI : MonoBehaviour
{
    [Header("Progress Bar References")]
    [SerializeField] private GameObject progressBarCanvas;
    [SerializeField] private Image progressCircleFill;
    [SerializeField] private Image progressCircleBackground;
    [SerializeField] private TextMeshProUGUI progressText;
    
    [Header("Progress Settings")]
    [SerializeField] private Color processingColor = new Color(1f, 0.65f, 0f); // Orange
    [SerializeField] private Color completeColor = new Color(0.3f, 0.8f, 0.3f); // Green
    [SerializeField] private float rotationSpeed = 100f;
    
    [Header("Animation Settings")]
    [SerializeField] private float scaleAnimationSpeed = 2f;
    [SerializeField] private float pulseAmount = 0.1f;
    
    private float currentProgress = 0f;
    private bool isAnimating = false;
    private Vector3 originalScale;
    private Transform targetTransform;
    
    private void Awake()
    {
        if (progressBarCanvas != null)
        {
            originalScale = progressBarCanvas.transform.localScale;
            progressBarCanvas.SetActive(false);
        }
        
        // Configure circular fill image for radial fill
        if (progressCircleFill != null)
        {
            progressCircleFill.type = Image.Type.Filled;
            progressCircleFill.fillMethod = Image.FillMethod.Radial360;
            progressCircleFill.fillOrigin = (int)Image.Origin360.Top;
            progressCircleFill.fillClockwise = true;
            progressCircleFill.fillAmount = 0f;
        }
    }
    
    private void Update()
    {
        if (!isAnimating) return;
        
        // Keep progress bar above target
        if (targetTransform != null && progressBarCanvas != null)
        {
            Vector3 targetPosition = targetTransform.position + Vector3.up * 2f;
            progressBarCanvas.transform.position = targetPosition;
            
            // Make it face camera
            if (Camera.main != null)
            {
                progressBarCanvas.transform.LookAt(Camera.main.transform);
                progressBarCanvas.transform.Rotate(0, 180, 0);
            }
        }
        
        // Animate progress bar (pulse effect)
        AnimatePulse();
    }
    
    /// <summary>
    /// Show progress bar above a specific transform
    /// </summary>
    public void ShowProgress(Transform target, float duration, string statusText = "Processing...")
    {
        targetTransform = target;
        currentProgress = 0f;
        isAnimating = true;
        
        if (progressBarCanvas != null)
        {
            progressBarCanvas.SetActive(true);
        }
        
        if (progressText != null)
        {
            progressText.text = statusText;
        }
        
        if (progressCircleFill != null)
        {
            progressCircleFill.color = processingColor;
            progressCircleFill.fillAmount = 0f;
        }
        
        StartCoroutine(AnimateProgress(duration));
    }
    
    /// <summary>
    /// Animate the progress from 0 to 1 over duration
    /// </summary>
    private IEnumerator AnimateProgress(float duration)
    {
        float elapsed = 0f;
        
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            currentProgress = Mathf.Clamp01(elapsed / duration);
            
            if (progressCircleFill != null)
            {
                progressCircleFill.fillAmount = currentProgress;
            }
            
            // Update percentage text
            if (progressText != null)
            {
                int percentage = Mathf.RoundToInt(currentProgress * 100f);
                progressText.text = $"{percentage}%";
            }
            
            yield return null;
        }
        
        // Complete
        currentProgress = 1f;
        if (progressCircleFill != null)
        {
            progressCircleFill.fillAmount = 1f;
            progressCircleFill.color = completeColor;
        }
        
        if (progressText != null)
        {
            progressText.text = "Done!";
        }
        
        // Wait a moment before hiding
        yield return new WaitForSeconds(0.3f);
        
        HideProgress();
    }
    
    /// <summary>
    /// Hide the progress bar
    /// </summary>
    public void HideProgress()
    {
        isAnimating = false;
        
        if (progressBarCanvas != null)
        {
            progressBarCanvas.SetActive(false);
        }
        
        currentProgress = 0f;
    }
    
    /// <summary>
    /// Pulse animation for visual feedback
    /// </summary>
    private void AnimatePulse()
    {
        if (progressBarCanvas == null) return;
        
        float pulse = 1f + Mathf.Sin(Time.time * scaleAnimationSpeed) * pulseAmount;
        progressBarCanvas.transform.localScale = originalScale * pulse;
    }
    
    /// <summary>
    /// Manual progress update (if you need more control)
    /// </summary>
    public void SetProgress(float progress)
    {
        currentProgress = Mathf.Clamp01(progress);
        
        if (progressCircleFill != null)
        {
            progressCircleFill.fillAmount = currentProgress;
        }
        
        if (progressText != null)
        {
            int percentage = Mathf.RoundToInt(currentProgress * 100f);
            progressText.text = $"{percentage}%";
        }
    }
    
    /// <summary>
    /// Check if progress is currently being shown
    /// </summary>
    public bool IsShowing()
    {
        return isAnimating;
    }
}