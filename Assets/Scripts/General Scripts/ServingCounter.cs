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
    private PlayerInventory cachedPlayerInventory;

    private PlayerInventory GetPlayerInventory()
    {
        if (cachedPlayerInventory == null)
        {
            cachedPlayerInventory = FindObjectOfType<PlayerInventory>();
        }
        return cachedPlayerInventory;
    }

    private void Start()
    {
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }

        // Ensure serving point is set (create one if missing)
        if (servingPoint == null)
        {
            GameObject servingPointObj = new GameObject("ServingPoint");
            servingPointObj.transform.SetParent(transform);
            servingPointObj.transform.localPosition = Vector3.up * 0.5f; // Slightly above the counter
            servingPoint = servingPointObj.transform;
        }
    }

    public string GetInteractionText()
    {
        var player = GetPlayerInventory();
        if (player != null && player.GetItemCount(ItemType.CoffeeCup) > 0)
        {
            Customer waitingCustomer = FindWaitingCustomer();
            if (waitingCustomer != null)
            {
                return "Press E - Serve Coffee";
            }
            else
            {
                return "Wait for customer to order";
            }
        }
        return "Serving Counter";
    }

    public bool CanInteract()
    {
        var inventory = GetPlayerInventory();
        if (inventory == null)
        {
            return false;
        }

        int coffeeCount = inventory.GetItemCount(ItemType.CoffeeCup);
        if (coffeeCount == 0)
        {
            return false;
        }

        // Check if there's a waiting customer - only allow serving when customer is ready
        Customer waitingCustomer = FindWaitingCustomer();
        bool canInteract = waitingCustomer != null;

        // Debug log for troubleshooting
        if (!canInteract && coffeeCount > 0)
        {
            // Only log occasionally to avoid spam
            if (Random.Range(0f, 100f) < 1f) // 1% chance per frame
            {
                Debug.Log($"ServingCounter: Player has {coffeeCount} coffee(s) but no waiting customer found.");
            }
        }

        return canInteract;
    }

    public void OnInteract()
    {
        var inventory = GetPlayerInventory();
        if (inventory == null)
        {
            Debug.LogWarning("ServingCounter: PlayerInventory not found!");
            return;
        }

        int coffeeCount = inventory.GetItemCount(ItemType.CoffeeCup);
        if (coffeeCount == 0)
        {
            Debug.LogWarning("ServingCounter: Player has no coffee cups!");
            return;
        }

        Customer waitingCustomer = FindWaitingCustomer();
        if (waitingCustomer != null)
        {
            Debug.Log($"ServingCounter: Found waiting customer {waitingCustomer.name}. Serving coffee...");
            // Try to serve coffee to waiting customer - only remove coffee if customer accepts it
            bool orderReceived = waitingCustomer.ReceiveOrder();
            if (orderReceived)
            {
                // Customer successfully received order, now remove coffee from inventory
                bool coffeeRemoved = inventory.TryRemoveItem(ItemType.CoffeeCup);
                if (coffeeRemoved)
                {
                    Debug.Log($"ServingCounter: Coffee removed from inventory. Remaining cups: {inventory.GetItemCount(ItemType.CoffeeCup)}");
                    // Don't add money here - the customer's ReceiveOrder() method will handle it via GameManager
                    // This prevents double money rewards
                    PlayServingEffects();
                }
                else
                {
                    Debug.LogError("ServingCounter: Customer received order but failed to remove coffee from inventory!");
                }
            }
            else
            {
                Debug.LogWarning("ServingCounter: Customer could not receive order (wrong state or too far). Coffee not removed.");
            }
        }
        // This should never happen if CanInteract() is working correctly
        // But adding as safety check
        else
        {
            Debug.LogWarning("Tried to serve coffee but no customer found. Wait for a customer to arrive.");
        }
    }

    private Customer FindWaitingCustomer()
    {
        // First try direct overlap sphere detection
        Collider[] nearbyColliders = Physics.OverlapSphere(transform.position, customerDetectionRadius, customerLayer);

        Customer closestCustomer = null;
        float closestDistance = float.MaxValue;

        foreach (var collider in nearbyColliders)
        {
            var customer = collider.GetComponent<Customer>();
            if (customer != null)
            {
                bool isWaiting = customer.IsWaitingForOrder();

                Vector3 referencePoint = servingPoint != null ? servingPoint.position : transform.position;
                float distance = Vector3.Distance(referencePoint, customer.transform.position);

                bool isCloseEnough = false;
                if (!isWaiting)
                {
                    Customer.CustomerState state = customer.GetCurrentState();
                    // If customer is moving to counter and very close, treat them as waiting
                    if (state == Customer.CustomerState.MovingToCounter && distance < 4.0f)
                    {
                        isCloseEnough = true;
                        Debug.Log($"ServingCounter: Customer {customer.name} is close enough ({distance:F2}m from serving point) despite being in {state} state. Treating as waiting.");
                    }
                }

                if (isWaiting || isCloseEnough)
                {
                    if (distance < closestDistance)
                    {
                        closestDistance = distance;
                        closestCustomer = customer;
                    }
                }
            }
        }

        // If no customers found via collider detection (might be blocked by counter's collider),
        // fall back to direct object query - this bypasses collider blocking
        if (closestCustomer == null)
        {
            Customer[] allCustomers = FindObjectsOfType<Customer>();
            // Use serving point position to match Customer.ReceiveOrder() calculation
            Vector3 referencePoint = servingPoint != null ? servingPoint.position : transform.position;

            foreach (var customer in allCustomers)
            {
                if (customer != null)
                {
                    bool isWaiting = customer.IsWaitingForOrder();
                    float distance = Vector3.Distance(referencePoint, customer.transform.position);

                    // Also accept customers who are moving to counter but are very close (within 4 units)
                    // Use 4.0f to match ReceiveOrder's acceptance distance
                    bool isCloseEnough = false;
                    if (!isWaiting && distance <= customerDetectionRadius)
                    {
                        Customer.CustomerState state = customer.GetCurrentState();
                        if (state == Customer.CustomerState.MovingToCounter && distance < 4.0f)
                        {
                            isCloseEnough = true;
                        }
                    }

                    if (isWaiting || isCloseEnough)
                    {
                        if (distance <= customerDetectionRadius)
                        {
                            // Also check if there's a clear line of sight (optional, can help with detection)
                            // But for now, just use distance check to ensure we detect customers even if blocked
                            if (distance < closestDistance)
                            {
                                closestDistance = distance;
                                closestCustomer = customer;
                            }
                        }
                    }
                }
            }
        }

        // Additional detection: Try casting from above the counter to find customers
        // This helps if the counter's collider is blocking detection at ground level
        if (closestCustomer == null)
        {
            Vector3 castPosition = (servingPoint != null ? servingPoint.position : transform.position) + Vector3.up * 0.5f;
            Collider[] upwardColliders = Physics.OverlapSphere(castPosition, customerDetectionRadius, customerLayer);

            // Use serving point position to match Customer.ReceiveOrder() calculation
            Vector3 referencePoint = servingPoint != null ? servingPoint.position : transform.position;

            foreach (var collider in upwardColliders)
            {
                var customer = collider.GetComponent<Customer>();
                if (customer != null)
                {
                    bool isWaiting = customer.IsWaitingForOrder();
                    float distance = Vector3.Distance(referencePoint, customer.transform.position);

                    // Also accept customers who are moving to counter but are very close (within 4 units)
                    // Use 4.0f to match ReceiveOrder's acceptance distance
                    bool isCloseEnough = false;
                    if (!isWaiting)
                    {
                        Customer.CustomerState state = customer.GetCurrentState();
                        if (state == Customer.CustomerState.MovingToCounter && distance < 4.0f)
                        {
                            isCloseEnough = true;
                        }
                    }

                    if (isWaiting || isCloseEnough)
                    {
                        if (distance < closestDistance && distance <= customerDetectionRadius)
                        {
                            closestDistance = distance;
                            closestCustomer = customer;
                        }
                    }
                }
            }
        }

        // Debug log if no customer found but there are customers nearby (throttled to avoid spam)
        if (closestCustomer == null)
        {
            // Only log occasionally to avoid console spam
            if (Random.Range(0f, 100f) < 0.5f) // 0.5% chance per call
            {
                Customer[] allCustomers = FindObjectsOfType<Customer>();
                if (allCustomers.Length > 0)
                {
                    System.Text.StringBuilder debugInfo = new System.Text.StringBuilder();
                    debugInfo.AppendLine($"ServingCounter: No waiting customer found. Total customers: {allCustomers.Length}");
                    foreach (var customer in allCustomers)
                    {
                        if (customer != null)
                        {
                            float distance = Vector3.Distance(transform.position, customer.transform.position);
                            debugInfo.AppendLine($"  - Customer {customer.name}: Distance={distance:F2}, State={customer.GetCurrentState()}, IsWaiting={customer.IsWaitingForOrder()}");
                        }
                    }
                    Debug.LogWarning(debugInfo.ToString());
                }
            }
        }

        return closestCustomer;
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
        var inventory = GetPlayerInventory();
        Customer waitingCustomer = FindWaitingCustomer();
    }
}