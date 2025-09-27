using UnityEngine;

public class BeanSource : MonoBehaviour, IInteractable
{
   [Header("Bean Source Settings")]
    [SerializeField] private float collectionCooldown = 0.5f;
    [SerializeField] private string sourceName = "Coffee Bean Bag";
    
    [Header("Effects")]
    [SerializeField] private AudioClip beanCollectSound;
    [SerializeField] private GameObject collectEffect;
    
    [Header("Debug")]
    [SerializeField] private bool enableDebugLogs = true;
    
    private float lastCollectionTime;
    private AudioSource audioSource;
    private PlayerInventory playerInventory;
    
    private void Start()
    {
        // Setup audio source
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.spatialBlend = 1f; // 3D sound
            audioSource.playOnAwake = false;
        }
        
        DebugLog($"Bean source '{sourceName}' initialized");
    }
    
    // ===== IINTERACTABLE IMPLEMENTATION =====
    
    public string GetInteractionText()
    {
        // Find player inventory if not cached
        if (playerInventory == null)
        {
            PlayerInventory[] inventories = FindObjectsOfType<PlayerInventory>();
            if (inventories.Length > 0)
            {
                playerInventory = inventories[0];
            }
        }
        
        if (playerInventory == null)
            return "Press E - Collect Bean";
        
        // Check cooldown
        if (Time.time - lastCollectionTime < collectionCooldown)
        {
            return "Collecting...";
        }
        
        // Check if inventory is full
        if (playerInventory.IsInventoryFull(ItemType.CoffeeBean))
        {
            int current = playerInventory.GetItemCount(ItemType.CoffeeBean);
            int max = playerInventory.GetCapacity(ItemType.CoffeeBean);
            return $"Bean Bag Full ({current}/{max})";
        }
        
        // Normal interaction
        int currentBeans = playerInventory.GetItemCount(ItemType.CoffeeBean);
        int maxBeans = playerInventory.GetCapacity(ItemType.CoffeeBean);
        return $"Press E - Collect Bean ({currentBeans}/{maxBeans})";
    }
    
    public bool CanInteract()
    {
        // Find player inventory
        if (playerInventory == null)
        {
            PlayerInventory[] inventories = FindObjectsOfType<PlayerInventory>();
            if (inventories.Length > 0)
            {
                playerInventory = inventories[0];
            }
        }
        
        if (playerInventory == null)
            return false;
        
        // Check cooldown
        if (Time.time - lastCollectionTime < collectionCooldown)
            return false;
        
        // Check if player can carry more beans
        return playerInventory.CanCarryItem(ItemType.CoffeeBean);
    }
    
    public void OnInteract()
    {
        if (!CanInteract())
            return;
        
        if (playerInventory == null)
            return;
        
        // Add bean to inventory
        if (playerInventory.TryAddItem(ItemType.CoffeeBean))
        {
            lastCollectionTime = Time.time;
            
            // Play effects
            PlayCollectionEffects();
            
            int currentBeans = playerInventory.GetItemCount(ItemType.CoffeeBean);
            DebugLog($"Bean collected! Player now has {currentBeans} beans.");
        }
        else
        {
            DebugLog("Failed to collect bean - inventory might be full");
        }
    }
    
    // ===== EFFECTS =====
    
    private void PlayCollectionEffects()
    {
        // Play sound
        if (audioSource != null && beanCollectSound != null)
        {
            audioSource.PlayOneShot(beanCollectSound);
        }
        
        // Spawn effect
        if (collectEffect != null)
        {
            GameObject effect = Instantiate(collectEffect, transform.position + Vector3.up, Quaternion.identity);
            Destroy(effect, 2f);
        }
    }
    
    private void DebugLog(string message)
    {
        if (enableDebugLogs)
        {
            Debug.Log($"[BeanSource] {message}");
        }
    }
    
    // ===== VISUAL FEEDBACK =====
    
    private void OnDrawGizmos()
    {
        // Draw interaction indicator
        Gizmos.color = CanInteract() ? Color.green : Color.yellow;
        Gizmos.DrawWireCube(transform.position + Vector3.up * 0.5f, Vector3.one * 0.3f);
        
        // Draw bean source label
        Gizmos.color = new Color(0.6f, 0.3f, 0.1f); // Brown color
        Gizmos.DrawWireSphere(transform.position + Vector3.up, 0.2f);
    }
}
