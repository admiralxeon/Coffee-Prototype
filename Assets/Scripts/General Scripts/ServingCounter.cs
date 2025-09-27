using UnityEngine;

public class ServingCounter : MonoBehaviour, IInteractable
{
    [Header("Serving Configuration")]
    [SerializeField] private Transform servingPoint;
    [SerializeField] private float customerDetectionRadius = 5f;
    [SerializeField] private LayerMask customerLayer = 1 << 6; // Customer layer
    [SerializeField] private int moneyReward = 1; // How many money items to give
    
    [Header("Audio & Effects")]
    [SerializeField] private AudioClip servingSound;
    [SerializeField] private ParticleSystem servingEffect;
    
    private AudioSource audioSource;
    
    private void Start()
    {
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
        
        Debug.Log($"ServingCounter initialized - Detection radius: {customerDetectionRadius}");
    }
    
    public string GetInteractionText()
    {
        var player = FindObjectOfType<PlayerInventory>();
        if (player != null && player.GetItemCount(ItemType.CoffeeCup) > 0)
        {
            Customer waitingCustomer = FindWaitingCustomer();
            if (waitingCustomer != null)
            {
                return "Serve Coffee";
            }
            else
            {
                return "No customers waiting";
            }
        }
        return "";
    }
    
    public bool CanInteract()
    {
        var inventory = FindObjectOfType<PlayerInventory>();
        if (inventory == null || inventory.GetItemCount(ItemType.CoffeeCup) == 0)
        {
            return false;
        }
        
        // Can interact if there's a customer waiting
        Customer waitingCustomer = FindWaitingCustomer();
        return waitingCustomer != null;
    }
    
    public void OnInteract()
    {
        Debug.Log("ServingCounter OnInteract called!");
        
        var inventory = FindObjectOfType<PlayerInventory>();
        if (inventory == null)
        {
            Debug.LogError("No PlayerInventory found!");
            return;
        }
        
        if (inventory.GetItemCount(ItemType.CoffeeCup) == 0)
        {
            Debug.Log("Player has no coffee cups to serve!");
            return;
        }
        
        // Find customer waiting for order
        Customer waitingCustomer = FindWaitingCustomer();
        if (waitingCustomer != null)
        {
            Debug.Log($"Found waiting customer: {waitingCustomer.name}");
            
            // Remove coffee cup from player
            bool coffeeRemoved = inventory.TryRemoveItem(ItemType.CoffeeCup);
            if (coffeeRemoved)
            {
                Debug.Log("Coffee cup removed from inventory");
                
                // Give money to player for serving
                for (int i = 0; i < moneyReward; i++)
                {
                    inventory.TryAddItem(ItemType.Money);
                }
                Debug.Log($"Added {moneyReward} money to inventory");
                
                // Notify customer they received their order
                waitingCustomer.ReceiveOrder();
                Debug.Log("Customer notified - order received");
                
                // Play effects
                PlayServingEffects();
                
                Debug.Log("Coffee served successfully!");
            }
            else
            {
                Debug.LogError("Failed to remove coffee cup from inventory!");
            }
        }
        else
        {
            Debug.Log("No waiting customer found nearby!");
            
            // Debug: List all customers and their states
            Customer[] allCustomers = FindObjectsOfType<Customer>();
            Debug.Log($"Total customers in scene: {allCustomers.Length}");
            foreach (var customer in allCustomers)
            {
                float distance = Vector3.Distance(transform.position, customer.transform.position);
                Debug.Log($"Customer {customer.name}: Distance={distance:F1}, IsWaiting={customer.IsWaitingForOrder()}");
            }
        }
    }
    
    private Customer FindWaitingCustomer()
    {
        // Use OverlapSphere to find customers in range
        Collider[] nearbyColliders = Physics.OverlapSphere(transform.position, customerDetectionRadius, customerLayer);
        
        foreach (var collider in nearbyColliders)
        {
            var customer = collider.GetComponent<Customer>();
            if (customer != null && customer.IsWaitingForOrder())
            {
                Debug.Log($"Found waiting customer via OverlapSphere: {customer.name}");
                return customer;
            }
        }
        
        // Alternative method - check all customers in scene (fallback)
        Customer[] allCustomers = FindObjectsOfType<Customer>();
        foreach (var customer in allCustomers)
        {
            if (customer != null && customer.IsWaitingForOrder())
            {
                float distance = Vector3.Distance(transform.position, customer.transform.position);
                if (distance <= customerDetectionRadius)
                {
                    Debug.Log($"Found waiting customer via scene search: {customer.name} (distance: {distance:F1})");
                    return customer;
                }
            }
        }
        
        return null;
    }
    
    private void PlayServingEffects()
    {
        // Play serving sound
        if (audioSource != null && servingSound != null)
        {
            audioSource.PlayOneShot(servingSound);
        }
        
        // Play serving particle effect
        if (servingEffect != null)
        {
            servingEffect.Play();
        }
    }
    
    public Transform GetServingPoint()
    {
        return servingPoint;
    }
    
    // Visual debugging
    private void OnDrawGizmos()
    {
        // Draw customer detection radius
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, customerDetectionRadius);
        
        // Draw serving point
        if (servingPoint != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireCube(servingPoint.position, Vector3.one * 0.5f);
            Gizmos.DrawLine(transform.position, servingPoint.position);
        }
        
        // Draw lines to waiting customers
        Customer[] customers = FindObjectsOfType<Customer>();
        foreach (var customer in customers)
        {
            if (customer != null && customer.IsWaitingForOrder())
            {
                float distance = Vector3.Distance(transform.position, customer.transform.position);
                if (distance <= customerDetectionRadius)
                {
                    Gizmos.color = Color.blue;
                    Gizmos.DrawLine(transform.position, customer.transform.position);
                }
            }
        }
    }
    
    // Test method - right click component and select "Test Serve Coffee"
    [ContextMenu("Test Serve Coffee")]
    private void TestServeCoffee()
    {
        Debug.Log("=== MANUAL SERVE TEST ===");
        OnInteract();
    }
    
    // Debug method to check current state
    [ContextMenu("Debug Current State")]
    private void DebugCurrentState()
    {
        var inventory = FindObjectOfType<PlayerInventory>();
        Debug.Log($"=== SERVING COUNTER DEBUG ===");
        Debug.Log($"Player found: {inventory != null}");
        Debug.Log($"Coffee cups in inventory: {inventory?.GetItemCount(ItemType.CoffeeCup) ?? 0}");
        Debug.Log($"Can interact: {CanInteract()}");
        Debug.Log($"Interaction text: '{GetInteractionText()}'");
        
        Customer waitingCustomer = FindWaitingCustomer();
        Debug.Log($"Waiting customer found: {waitingCustomer != null}");
        if (waitingCustomer != null)
        {
            float distance = Vector3.Distance(transform.position, waitingCustomer.transform.position);
            Debug.Log($"Customer distance: {distance:F1}");
        }
    }
}