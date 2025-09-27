using UnityEngine;

public class CoffeeMachine : MonoBehaviour, IInteractable
{
   [Header("Coffee Machine Settings")]
    [SerializeField] private float processingTime = 3f;
    [SerializeField] private int beansPerCoffee = 1;
    [SerializeField] private string machineName = "Coffee Machine";
    
    [Header("Coffee Cup Settings")]
    [SerializeField] private GameObject coffeeCupPrefab;
    [SerializeField] private Transform cupSpawnPoint;
    [SerializeField] private int maxCupsOnMachine = 3;
    
    [Header("Processing Effects")]
    [SerializeField] private GameObject processingEffect;
    [SerializeField] private AudioClip processingSound;
    [SerializeField] private AudioClip completionSound;
    [SerializeField] private ParticleSystem steamEffect;
    
    [Header("Visual Feedback")]
    [SerializeField] private Renderer machineRenderer;
    [SerializeField] private Color idleColor = Color.white;
    [SerializeField] private Color processingColor = Color.red;
    [SerializeField] private Color readyColor = Color.green;
    
    [Header("Debug")]
    [SerializeField] private bool enableDebugLogs = true;
    
    // Machine state
    private bool isProcessing = false;
    private bool hasCoffeeReady = false;
    private float processingStartTime;
    private PlayerInventory playerInventory;
    private AudioSource audioSource;
    
    // Coffee cups on machine
    private System.Collections.Generic.List<GameObject> cupsOnMachine = new System.Collections.Generic.List<GameObject>();
    
    private void Start()
    {
        SetupMachine();
        SetMachineColor(idleColor);
    }
    
    private void SetupMachine()
    {
        // Setup audio source
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.spatialBlend = 1f; // 3D sound
            audioSource.playOnAwake = false;
        }
        
        // Setup cup spawn point if not assigned
        if (cupSpawnPoint == null)
        {
            GameObject spawnPoint = new GameObject("CupSpawnPoint");
            spawnPoint.transform.SetParent(transform);
            spawnPoint.transform.localPosition = Vector3.up * 1.2f; // On top of machine
            cupSpawnPoint = spawnPoint.transform;
        }
        
        // Setup machine renderer if not assigned
        if (machineRenderer == null)
        {
            machineRenderer = GetComponent<Renderer>();
        }
        
        DebugLog($"Coffee machine '{machineName}' initialized");
    }
    
    private void Update()
    {
        UpdateProcessing();
    }
    
    // ===== PROCESSING LOGIC =====
    
    private void UpdateProcessing()
    {
        if (isProcessing)
        {
            float elapsed = Time.time - processingStartTime;
            if (elapsed >= processingTime)
            {
                CompleteProcessing();
            }
        }
    }
    
    private void StartProcessing()
    {
        if (isProcessing || hasCoffeeReady)
            return;
        
        isProcessing = true;
        processingStartTime = Time.time;
        
        // Visual/Audio feedback
        SetMachineColor(processingColor);
        PlayProcessingEffects();
        
        DebugLog("Started coffee processing");
    }
    
    private void CompleteProcessing()
    {
        isProcessing = false;
        hasCoffeeReady = true;
        
        // Spawn coffee cup
        SpawnCoffeeCup();
        
        // Visual/Audio feedback
        SetMachineColor(readyColor);
        PlayCompletionEffects();
        
        DebugLog("Coffee processing completed");
    }
    
     private void SpawnCoffeeCup()
    {
        if (coffeeCupPrefab == null)
        {
            DebugLog("Warning: Coffee cup prefab not assigned!");
            return;
        }
        
        if (cupsOnMachine.Count >= maxCupsOnMachine)
        {
            DebugLog("Machine is full of cups!");
            return;
        }
        
        // Calculate spawn position with slight offset for multiple cups
        Vector3 spawnPosition = cupSpawnPoint.position;
        
        // Add extra height to ensure cup sits properly on machine
        spawnPosition += Vector3.up * 0.1f;
        
        if (cupsOnMachine.Count > 0)
        {
            float offset = cupsOnMachine.Count * 0.3f;
            spawnPosition += Vector3.right * offset;
        }
        
        GameObject newCup = Instantiate(coffeeCupPrefab, spawnPosition, cupSpawnPoint.rotation);
        
        // Add CoffeeCup component if it doesn't exist
        CoffeeCup cupComponent = newCup.GetComponent<CoffeeCup>();
        if (cupComponent == null)
        {
            cupComponent = newCup.AddComponent<CoffeeCup>();
        }
        cupComponent.Initialize(this);
        
        cupsOnMachine.Add(newCup);
        DebugLog($"Spawned coffee cup. Total on machine: {cupsOnMachine.Count}");
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
            return "Press E to use machine";
        
        // Check machine state
        if (isProcessing)
        {
            float elapsed = Time.time - processingStartTime;
            float remaining = processingTime - elapsed;
            return $"Processing... ({remaining:F1}s)";
        }
        
        if (hasCoffeeReady)
        {
            return "Coffee Ready! Press E to collect";
        }
        
        // Check if player has enough beans
        int playerBeans = playerInventory.GetItemCount(ItemType.CoffeeBean);
        if (playerBeans < beansPerCoffee)
        {
            return $"Need {beansPerCoffee} beans (have {playerBeans})";
        }
        
        // Check if machine can hold more cups
        if (cupsOnMachine.Count >= maxCupsOnMachine)
        {
            return "Machine full - collect cups first";
        }
        
        return $"Press E - Make Coffee ({beansPerCoffee} bean)";
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
        
        // Can collect ready coffee
        if (hasCoffeeReady)
            return true;
        
        // Can't interact while processing
        if (isProcessing)
            return false;
        
        // Can't interact if machine is full
        if (cupsOnMachine.Count >= maxCupsOnMachine)
            return false;
        
        // Check if player has enough beans
        return playerInventory.GetItemCount(ItemType.CoffeeBean) >= beansPerCoffee;
    }
    
    public void OnInteract()
    {
        if (!CanInteract())
            return;
        
        if (playerInventory == null)
            return;
        
        // Collect ready coffee
        if (hasCoffeeReady)
        {
            CollectCoffee();
            return;
        }
        
        // Start making coffee
        if (playerInventory.GetItemCount(ItemType.CoffeeBean) >= beansPerCoffee)
        {
            // Remove beans from inventory
            for (int i = 0; i < beansPerCoffee; i++)
            {
                playerInventory.TryRemoveItem(ItemType.CoffeeBean);
            }
            
            StartProcessing();
            DebugLog($"Player used {beansPerCoffee} beans to start coffee making");
        }
    }
    
    private void CollectCoffee()
    {
        if (!hasCoffeeReady || cupsOnMachine.Count == 0)
            return;
        
        // Add coffee to player inventory
        if (playerInventory.TryAddItem(ItemType.CoffeeCup))
        {
            // Remove cup from machine
            GameObject cupToRemove = cupsOnMachine[cupsOnMachine.Count - 1];
            cupsOnMachine.RemoveAt(cupsOnMachine.Count - 1);
            Destroy(cupToRemove);
            
            // Reset machine state
            hasCoffeeReady = cupsOnMachine.Count > 0; // Still ready if more cups available
            if (!hasCoffeeReady)
            {
                SetMachineColor(idleColor);
            }
            
            DebugLog("Player collected coffee cup");
        }
        else
        {
            DebugLog("Player inventory full - cannot collect coffee");
        }
    }
    
    // ===== EFFECTS AND FEEDBACK =====
    
    private void SetMachineColor(Color color)
    {
        if (machineRenderer != null)
        {
            machineRenderer.material.color = color;
        }
    }
    
    private void PlayProcessingEffects()
    {
        // Play sound
        if (audioSource != null && processingSound != null)
        {
            audioSource.clip = processingSound;
            audioSource.loop = true;
            audioSource.Play();
        }
        
        // Start steam effect
        if (steamEffect != null)
        {
            steamEffect.Play();
        }
        
        // Spawn processing effect
        if (processingEffect != null)
        {
            GameObject effect = Instantiate(processingEffect, transform.position + Vector3.up, Quaternion.identity);
            Destroy(effect, processingTime);
        }
    }
    
    private void PlayCompletionEffects()
    {
        // Stop processing sound
        if (audioSource != null)
        {
            audioSource.Stop();
        }
        
        // Play completion sound
        if (audioSource != null && completionSound != null)
        {
            audioSource.PlayOneShot(completionSound);
        }
        
        // Stop steam effect
        if (steamEffect != null)
        {
            steamEffect.Stop();
        }
    }
    
    // ===== UTILITY METHODS =====
    
    public void RemoveCupFromMachine(GameObject cup)
    {
        if (cupsOnMachine.Contains(cup))
        {
            cupsOnMachine.Remove(cup);
            DebugLog($"Cup removed from machine. Remaining: {cupsOnMachine.Count}");
            
            // Update machine state
            if (cupsOnMachine.Count == 0)
            {
                hasCoffeeReady = false;
                SetMachineColor(idleColor);
            }
        }
    }
    
    private void DebugLog(string message)
    {
        if (enableDebugLogs)
        {
            Debug.Log($"[CoffeeMachine] {message}");
        }
    }
    
    // ===== DEBUG METHODS =====
    
    [ContextMenu("Start Test Processing")]
    private void StartTestProcessing()
    {
        if (!isProcessing && !hasCoffeeReady)
        {
            StartProcessing();
        }
    }
    
    [ContextMenu("Complete Processing")]
    private void ForceCompleteProcessing()
    {
        if (isProcessing)
        {
            CompleteProcessing();
        }
    }
    
    // ===== VISUAL GIZMOS =====
    
    private void OnDrawGizmos()
    {
        // Draw machine state
        Color gizmoColor = Color.white;
        if (isProcessing) gizmoColor = Color.red;
        else if (hasCoffeeReady) gizmoColor = Color.green;
        
        Gizmos.color = gizmoColor;
        Gizmos.DrawWireCube(transform.position + Vector3.up * 0.5f, Vector3.one * 0.4f);
        
        // Draw cup spawn point
        if (cupSpawnPoint != null)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(cupSpawnPoint.position, 0.1f);
        }
    }
}
