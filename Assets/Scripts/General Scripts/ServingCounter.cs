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
        
        Customer waitingCustomer = FindWaitingCustomer();
        return waitingCustomer != null;
    }
    
    public void OnInteract()
    {
        var inventory = FindObjectOfType<PlayerInventory>();
        if (inventory == null)
        {
            return;
        }
        
        if (inventory.GetItemCount(ItemType.CoffeeCup) == 0)
        {
            return;
        }
        
        Customer waitingCustomer = FindWaitingCustomer();
        if (waitingCustomer != null)
        {
            bool coffeeRemoved = inventory.TryRemoveItem(ItemType.CoffeeCup);
            if (coffeeRemoved)
            {
                for (int i = 0; i < moneyReward; i++)
                {
                    inventory.TryAddItem(ItemType.Money);
                }
                
                waitingCustomer.ReceiveOrder();
                PlayServingEffects();
            }
        }
    }
    
    private Customer FindWaitingCustomer()
    {
        Collider[] nearbyColliders = Physics.OverlapSphere(transform.position, customerDetectionRadius, customerLayer);
        
        foreach (var collider in nearbyColliders)
        {
            var customer = collider.GetComponent<Customer>();
            if (customer != null && customer.IsWaitingForOrder())
            {
                return customer;
            }
        }
        
        Customer[] allCustomers = FindObjectsOfType<Customer>();
        foreach (var customer in allCustomers)
        {
            if (customer != null && customer.IsWaitingForOrder())
            {
                float distance = Vector3.Distance(transform.position, customer.transform.position);
                if (distance <= customerDetectionRadius)
                {
                    return customer;
                }
            }
        }
        
        return null;
    }
    
    private void PlayServingEffects()
    {
        if (audioSource != null && servingSound != null)
        {
            audioSource.PlayOneShot(servingSound);
        }
        
        if (servingEffect != null)
        {
            servingEffect.Play();
        }
    }
    
    public Transform GetServingPoint()
    {
        return servingPoint;
    }
    
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, customerDetectionRadius);
        
        if (servingPoint != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireCube(servingPoint.position, Vector3.one * 0.5f);
            Gizmos.DrawLine(transform.position, servingPoint.position);
        }
        
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
    
    [ContextMenu("Test Serve Coffee")]
    private void TestServeCoffee()
    {
        OnInteract();
    }
    
    [ContextMenu("Debug Current State")]
    private void DebugCurrentState()
    {
        var inventory = FindObjectOfType<PlayerInventory>();
        Customer waitingCustomer = FindWaitingCustomer();
    }
}