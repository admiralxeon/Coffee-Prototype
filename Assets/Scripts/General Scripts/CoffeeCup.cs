using UnityEngine;

public class CoffeeCup : MonoBehaviour, IInteractable
{
    [Header("Coffee Cup Settings")]
    [SerializeField] private bool canBePickedUp = true;
    
    [Header("Effects")]
    [SerializeField] private AudioClip pickupSound;
    
    private CoffeeMachine parentMachine;
    private AudioSource audioSource;
    
    public void Initialize(CoffeeMachine machine)
    {
        parentMachine = machine;
        
        // Setup audio source
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.spatialBlend = 1f;
            audioSource.playOnAwake = false;
        }
    }
    
    // ===== IINTERACTABLE IMPLEMENTATION =====
    
    public string GetInteractionText()
    {
        if (!canBePickedUp)
            return "Coffee Cup";
        
        return "Press E - Take Coffee";
    }
    
    public bool CanInteract()
    {
        if (!canBePickedUp)
            return false;
        
        // Check if player can carry more coffee cups
        PlayerInventory playerInventory = FindObjectOfType<PlayerInventory>();
        if (playerInventory == null)
            return false;
        
        return playerInventory.CanCarryItem(ItemType.CoffeeCup);
    }
    
    public void OnInteract()
    {
        if (!CanInteract())
            return;
        
        PlayerInventory playerInventory = FindObjectOfType<PlayerInventory>();
        if (playerInventory == null)
            return;
        
        // Add to player inventory
        if (playerInventory.TryAddItem(ItemType.CoffeeCup))
        {
            // Play pickup sound
            if (audioSource != null && pickupSound != null)
            {
                audioSource.PlayOneShot(pickupSound);
            }
            
            // Remove from parent machine
            if (parentMachine != null)
            {
                parentMachine.RemoveCupFromMachine(gameObject);
            }
            
            // Destroy this cup
            Destroy(gameObject, 0.1f); // Small delay for sound
            
            Debug.Log("Coffee cup collected by player");
        }
    }
    
    // ===== VISUAL FEEDBACK =====
    
    private void OnDrawGizmos()
    {
        Gizmos.color = canBePickedUp ? Color.yellow : Color.gray;
        Gizmos.DrawWireCube(transform.position + Vector3.up * 0.1f, Vector3.one * 0.15f);
    }
}
