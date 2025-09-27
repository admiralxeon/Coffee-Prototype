using UnityEngine;

public class BeanSource : MonoBehaviour, IInteractable
{
   [Header("Bean Source Settings")]
    [SerializeField] private float collectionCooldown = 0.5f;
    [SerializeField] private string sourceName = "Coffee Bean Bag";
    
    [Header("Effects")]
    [SerializeField] private AudioClip beanCollectSound;
    [SerializeField] private GameObject collectEffect;
    
    
    private float lastCollectionTime;
    private AudioSource audioSource;
    private PlayerInventory playerInventory;
    
    private void Start()
    {
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.spatialBlend = 1f;
            audioSource.playOnAwake = false;
        }
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
            return "Press E - Collect Bean";
        
        if (Time.time - lastCollectionTime < collectionCooldown)
        {
            return "Collecting...";
        }
        
        if (playerInventory.IsInventoryFull(ItemType.CoffeeBean))
        {
            int current = playerInventory.GetItemCount(ItemType.CoffeeBean);
            int max = playerInventory.GetCapacity(ItemType.CoffeeBean);
            return $"Bean Bag Full ({current}/{max})";
        }
        
        int currentBeans = playerInventory.GetItemCount(ItemType.CoffeeBean);
        int maxBeans = playerInventory.GetCapacity(ItemType.CoffeeBean);
        return $"Press E - Collect Bean ({currentBeans}/{maxBeans})";
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
        
        if (Time.time - lastCollectionTime < collectionCooldown)
            return false;
        
        return playerInventory.CanCarryItem(ItemType.CoffeeBean);
    }
    
    public void OnInteract()
    {
        if (!CanInteract())
            return;
        
        if (playerInventory == null)
            return;
        
        if (playerInventory.TryAddItem(ItemType.CoffeeBean))
        {
            lastCollectionTime = Time.time;
            PlayCollectionEffects();
        }
    }
    
    
    private void PlayCollectionEffects()
    {
        if (audioSource != null && beanCollectSound != null)
        {
            audioSource.PlayOneShot(beanCollectSound);
        }
        
        if (collectEffect != null)
        {
            GameObject effect = Instantiate(collectEffect, transform.position + Vector3.up, Quaternion.identity);
            Destroy(effect, 2f);
        }
    }
    
    
    
    private void OnDrawGizmos()
    {
        Gizmos.color = CanInteract() ? Color.green : Color.yellow;
        Gizmos.DrawWireCube(transform.position + Vector3.up * 0.5f, Vector3.one * 0.3f);
        
        Gizmos.color = new Color(0.6f, 0.3f, 0.1f);
        Gizmos.DrawWireSphere(transform.position + Vector3.up, 0.2f);
    }
}
