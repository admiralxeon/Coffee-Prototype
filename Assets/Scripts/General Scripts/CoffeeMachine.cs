using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class CoffeeMachine : MonoBehaviour, IInteractable
{
    [Header("Processing Settings")]
    [SerializeField] private float processingTime = 3f;
    [SerializeField] private int beansRequired = 3;
    
    [Header("Coffee Cup")]
    [SerializeField] private GameObject coffeeCupPrefab;
    [SerializeField] private Transform cupSpawnPoint;
    
    [Header("Visual Feedback")]
    [SerializeField] private ProgressFeedbackUI progressFeedback;
    [SerializeField] private ParticleSystem steamEffect;
    [SerializeField] private Light machineLight;
    
    [Header("Colors")]
    [SerializeField] private Color idleColor = Color.gray;
    [SerializeField] private Color processingColor = Color.yellow;
    [SerializeField] private Color readyColor = Color.green;
    
    [Header("Audio")]
    [SerializeField] private AudioClip brewingSound;
    [SerializeField] private AudioClip completeSound;
    
    private bool isProcessing = false;
    private bool hasCoffeeReady = false;
    private GameObject currentCoffeeCup;
    private AudioSource audioSource;
    private Renderer machineRenderer;
    
    private void Start()
    {
        InitializeComponents();
        UpdateVisualState();
    }
    
    private void InitializeComponents()
    {
        // Audio setup
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
        audioSource.playOnAwake = false;
        audioSource.spatialBlend = 1f;
        
        // Renderer for color changes
        machineRenderer = GetComponentInChildren<Renderer>();
        
        // Progress feedback setup
        if (progressFeedback == null)
        {
            progressFeedback = FindObjectOfType<ProgressFeedbackUI>();
            if (progressFeedback == null)
            {
                Debug.LogWarning("ProgressFeedbackUI not found in scene!");
            }
        }
        
        Debug.Log($"Coffee Machine initialized - Processing time: {processingTime}s");
    }
    
    public string GetInteractionText()
    {
        if (hasCoffeeReady)
        {
            return "Collect Coffee";
        }
        else if (isProcessing)
        {
            return "Processing...";
        }
        else
        {
            var player = GetPlayerInventory();
            if (player != null && player.GetItemCount(ItemType.CoffeeBean) >= beansRequired)
            {
                return "Make Coffee";
            }
            else
            {
                return $"Need {beansRequired} beans";
            }
        }
    }
    
    public bool CanInteract()
    {
        if (hasCoffeeReady)
        {
            // Check if player can actually carry the coffee cup
            var player = GetPlayerInventory();
            return player != null && player.CanCarryItem(ItemType.CoffeeCup);
        }
        if (isProcessing) return false;
        
        var playerInventory = GetPlayerInventory();
        return playerInventory != null && playerInventory.GetItemCount(ItemType.CoffeeBean) >= beansRequired;
    }
    
    public void OnInteract()
    {
        if (hasCoffeeReady)
        {
            CollectCoffee();
        }
        else if (!isProcessing)
        {
            StartCoffeeProduction();
        }
    }
    
    private void StartCoffeeProduction()
    {
        var player = GetPlayerInventory();
        if (player == null || player.GetItemCount(ItemType.CoffeeBean) < beansRequired) 
            return;
        
        // Remove beans from player
        for (int i = 0; i < beansRequired; i++)
        {
            player.TryRemoveItem(ItemType.CoffeeBean);
        }
        
        isProcessing = true;
        UpdateVisualState();
        
        // Show progress bar
        if (progressFeedback != null)
        {
            progressFeedback.ShowProgress(transform, processingTime, "Brewing...");
        }
        
        // Start particle effects
        if (steamEffect != null)
        {
            steamEffect.Play();
        }
        
        // Play brewing sound (looped)
        if (audioSource != null && brewingSound != null)
        {
            audioSource.clip = brewingSound;
            audioSource.loop = true;
            audioSource.Play();
        }
        
        Debug.Log($"Coffee production started - {processingTime}s remaining");
        StartCoroutine(ProcessCoffee());
    }
    
    private IEnumerator ProcessCoffee()
    {
        float actualProcessingTime = processingTime;
        if (UpgradeSystem.Instance != null)
        {
            actualProcessingTime *= UpgradeSystem.Instance.GetMachineSpeedMultiplier();
        }
        yield return new WaitForSeconds(actualProcessingTime);
        
        // Stop processing effects
        if (audioSource != null)
        {
            audioSource.Stop();
            audioSource.loop = false;
        }
        
        if (steamEffect != null)
        {
            steamEffect.Stop();
        }
        
        // Play completion sound
        if (audioSource != null && completeSound != null)
        {
            audioSource.PlayOneShot(completeSound);
        }
        
        // Spawn coffee cup
        SpawnCoffeeCup();
        
        isProcessing = false;
        hasCoffeeReady = true;
        UpdateVisualState();
        
        Debug.Log("Coffee ready for collection!");
    }
    
    private void SpawnCoffeeCup()
    {
        if (coffeeCupPrefab == null || cupSpawnPoint == null) 
        {
            Debug.LogWarning("Coffee cup prefab or spawn point not set!");
            return;
        }
        
        currentCoffeeCup = Instantiate(coffeeCupPrefab, cupSpawnPoint.position, cupSpawnPoint.rotation);
        
        // Initialize the cup
        CoffeeCup cupScript = currentCoffeeCup.GetComponent<CoffeeCup>();
        if (cupScript != null)
        {
            cupScript.Initialize(this);
        }
        
        // Add a little pop-in animation
        StartCoroutine(AnimateCupSpawn(currentCoffeeCup));
    }
    
    private IEnumerator AnimateCupSpawn(GameObject cup)
    {
        if (cup == null) yield break;
        
        Vector3 originalScale = cup.transform.localScale;
        cup.transform.localScale = Vector3.zero;
        
        float duration = 0.3f;
        float elapsed = 0f;
        
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            // Elastic ease out
            float scale = Mathf.Sin(t * Mathf.PI * 0.5f);
            cup.transform.localScale = originalScale * scale;
            yield return null;
        }
        
        cup.transform.localScale = originalScale;
    }
    
    private void CollectCoffee()
    {
        var player = GetPlayerInventory();
        if (player == null) return;
        
        // Double-check that player can carry the item before attempting to add
        if (!player.CanCarryItem(ItemType.CoffeeCup))
        {
            Debug.LogWarning("Cannot collect coffee - inventory full!");
            return;
        }
        
        if (player.TryAddItem(ItemType.CoffeeCup))
        {
            // Remove the cup GameObject
            if (currentCoffeeCup != null)
            {
                Destroy(currentCoffeeCup);
                currentCoffeeCup = null;
            }
            
            hasCoffeeReady = false;
            UpdateVisualState();
            
            Debug.Log("Coffee collected!");
        }
        else
        {
            Debug.LogWarning("Failed to add coffee cup to inventory!");
        }
    }
    
    public void RemoveCupFromMachine(GameObject cup)
    {
        if (cup == currentCoffeeCup)
        {
            currentCoffeeCup = null;
            hasCoffeeReady = false;
            UpdateVisualState();
        }
    }
    
    private void UpdateVisualState()
    {
        // Update machine color
        Color targetColor = idleColor;
        
        if (hasCoffeeReady)
        {
            targetColor = readyColor;
        }
        else if (isProcessing)
        {
            targetColor = processingColor;
        }
        
        if (machineRenderer != null)
        {
            machineRenderer.material.color = targetColor;
        }
        
        // Update machine light
        if (machineLight != null)
        {
            machineLight.color = targetColor;
            machineLight.intensity = (hasCoffeeReady || isProcessing) ? 2f : 0.5f;
        }
    }
    
    private PlayerInventory GetPlayerInventory()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        return player?.GetComponent<PlayerInventory>();
    }
    
    // Public getters
    public bool IsProcessing() => isProcessing;
    public bool HasCoffeeReady() => hasCoffeeReady;
    public float GetProcessingTime() => processingTime;
    
    public  List<GameObject> GetCupsOnMachine()
    {
        var cups = new List<GameObject>();
        if (currentCoffeeCup != null)
        {
            cups.Add(currentCoffeeCup);
        }
        return cups;
    }
    
    private void OnDrawGizmosSelected()
    {
        // Visualize spawn point
        if (cupSpawnPoint != null)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(cupSpawnPoint.position, 0.1f);
            Gizmos.DrawLine(transform.position, cupSpawnPoint.position);
        }
    }
}