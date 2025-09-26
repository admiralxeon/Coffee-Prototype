using UnityEngine;

public class TestInteractable : MonoBehaviour, IInteractable
{
    [Header("Test Settings")]
    [SerializeField] private string interactionText = "Press E to interact";
    [SerializeField] private int maxInteractions = 3;
    
    private int interactionCount = 0;
    
    public string GetInteractionText()
    {
        if (interactionCount >= maxInteractions)
        {
            return "Already used";
        }
        return $"{interactionText} ({interactionCount}/{maxInteractions})";
    }
    
    public bool CanInteract()
    {
        return interactionCount < maxInteractions;
    }
    
    public void OnInteract()
    {
        if (!CanInteract()) return;
        
        interactionCount++;
        StartCoroutine(InteractionFeedback());
    }
    
    private System.Collections.IEnumerator InteractionFeedback()
    {
        Vector3 originalScale = transform.localScale;
        
        float elapsed = 0f;
        while (elapsed < 0.1f)
        {
            elapsed += Time.deltaTime;
            float scale = Mathf.Lerp(1f, 1.2f, elapsed / 0.1f);
            transform.localScale = originalScale * scale;
            yield return null;
        }
        
        elapsed = 0f;
        while (elapsed < 0.1f)
        {
            elapsed += Time.deltaTime;
            float scale = Mathf.Lerp(1.2f, 1f, elapsed / 0.1f);
            transform.localScale = originalScale * scale;
            yield return null;
        }
        
        transform.localScale = originalScale;
    }
}
