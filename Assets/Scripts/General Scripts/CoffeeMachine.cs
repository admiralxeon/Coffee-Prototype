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
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.spatialBlend = 1f;
            audioSource.playOnAwake = false;
        }
        
        if (cupSpawnPoint == null)
        {
            GameObject spawnPoint = new GameObject("CupSpawnPoint");
            spawnPoint.transform.SetParent(transform);
            spawnPoint.transform.localPosition = Vector3.up * 1.2f;
            cupSpawnPoint = spawnPoint.transform;
        }
        
        if (machineRenderer == null)
        {
            machineRenderer = GetComponent<Renderer>();
        }
        
    }
    
    private void Update()
    {
        UpdateProcessing();
    }
    
    
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
        
        SetMachineColor(processingColor);
        PlayProcessingEffects();
    }
    
    private void CompleteProcessing()
    {
        isProcessing = false;
        hasCoffeeReady = true;
        
        SpawnCoffeeCup();
        SetMachineColor(readyColor);
        PlayCompletionEffects();
    }
    
     private void SpawnCoffeeCup()
    {
        if (coffeeCupPrefab == null)
        {
            return;
        }
        
        if (cupsOnMachine.Count >= maxCupsOnMachine)
        {
            return;
        }
        
        Vector3 spawnPosition = cupSpawnPoint.position;
        spawnPosition += Vector3.up * 0.1f;
        
        if (cupsOnMachine.Count > 0)
        {
            float offset = cupsOnMachine.Count * 0.3f;
            spawnPosition += Vector3.right * offset;
        }
        
        GameObject newCup = Instantiate(coffeeCupPrefab, spawnPosition, cupSpawnPoint.rotation);
        
        CoffeeCup cupComponent = newCup.GetComponent<CoffeeCup>();
        if (cupComponent == null)
        {
            cupComponent = newCup.AddComponent<CoffeeCup>();
        }
        cupComponent.Initialize(this);
        
        cupsOnMachine.Add(newCup);
    }
    
    public string GetInteractionText()
    {
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
        
        int playerBeans = playerInventory.GetItemCount(ItemType.CoffeeBean);
        if (playerBeans < beansPerCoffee)
        {
            return $"Need {beansPerCoffee} beans (have {playerBeans})";
        }
        
        if (cupsOnMachine.Count >= maxCupsOnMachine)
        {
            return "Machine full - collect cups first";
        }
        
        return $"Press E - Make Coffee ({beansPerCoffee} bean)";
    }
    
    public bool CanInteract()
    {
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
        
        if (hasCoffeeReady)
            return true;
        
        if (isProcessing)
            return false;
        
        if (cupsOnMachine.Count >= maxCupsOnMachine)
            return false;
        return playerInventory.GetItemCount(ItemType.CoffeeBean) >= beansPerCoffee;
    }
    
    public void OnInteract()
    {
        if (!CanInteract())
            return;
        
        if (playerInventory == null)
            return;
        
        if (hasCoffeeReady)
        {
            CollectCoffee();
            return;
        }
        
        if (playerInventory.GetItemCount(ItemType.CoffeeBean) >= beansPerCoffee)
        {
            for (int i = 0; i < beansPerCoffee; i++)
            {
                playerInventory.TryRemoveItem(ItemType.CoffeeBean);
            }
            
            StartProcessing();
        }
    }
    
    private void CollectCoffee()
    {
        if (!hasCoffeeReady || cupsOnMachine.Count == 0)
            return;
        
        if (playerInventory.TryAddItem(ItemType.CoffeeCup))
        {
            GameObject cupToRemove = cupsOnMachine[cupsOnMachine.Count - 1];
            cupsOnMachine.RemoveAt(cupsOnMachine.Count - 1);
            Destroy(cupToRemove);
            
            hasCoffeeReady = cupsOnMachine.Count > 0;
            if (!hasCoffeeReady)
            {
                SetMachineColor(idleColor);
            }
        }
    }
    
    
    private void SetMachineColor(Color color)
    {
        if (machineRenderer != null)
        {
            machineRenderer.material.color = color;
        }
    }
    
    private void PlayProcessingEffects()
    {
        if (audioSource != null && processingSound != null)
        {
            audioSource.clip = processingSound;
            audioSource.loop = true;
            audioSource.Play();
        }
        
        if (steamEffect != null)
        {
            steamEffect.Play();
        }
        
        if (processingEffect != null)
        {
            GameObject effect = Instantiate(processingEffect, transform.position + Vector3.up, Quaternion.identity);
            Destroy(effect, processingTime);
        }
    }
    
    private void PlayCompletionEffects()
    {
        if (audioSource != null)
        {
            audioSource.Stop();
        }
        
        if (audioSource != null && completionSound != null)
        {
            audioSource.PlayOneShot(completionSound);
        }
        
        if (steamEffect != null)
        {
            steamEffect.Stop();
        }
    }
    
    
    public void RemoveCupFromMachine(GameObject cup)
    {
        if (cupsOnMachine.Contains(cup))
        {
            cupsOnMachine.Remove(cup);
            
            if (cupsOnMachine.Count == 0)
            {
                hasCoffeeReady = false;
                SetMachineColor(idleColor);
            }
        }
    }
    
    
    
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
    
    
    private void OnDrawGizmos()
    {
        Color gizmoColor = Color.white;
        if (isProcessing) gizmoColor = Color.red;
        else if (hasCoffeeReady) gizmoColor = Color.green;
        
        Gizmos.color = gizmoColor;
        Gizmos.DrawWireCube(transform.position + Vector3.up * 0.5f, Vector3.one * 0.4f);
        
        if (cupSpawnPoint != null)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(cupSpawnPoint.position, 0.1f);
        }
    }
}
