using UnityEngine;
using System.Collections.Generic;

public class CustomerSpawner : MonoBehaviour
{
     [Header("Spawning Settings")]
    [SerializeField] private GameObject customerPrefab;
    [SerializeField] private Transform spawnPoint;
    [SerializeField] private Transform counterPosition;
    [SerializeField] private float spawnInterval = 10f;
    [SerializeField] private int maxCustomers = 3;
    
    [Header("Customer Names")]
    [SerializeField] private string[] customerNames = {"Alex", "Sam", "Jordan", "Casey", "Taylor"};
    
    [Header("Debug")]
    [SerializeField] private bool enableDebugLogs = true;
    [SerializeField] private bool autoSpawn = true;
    
    // Active customers
    private List<Customer> activeCustomers = new List<Customer>();
    private float lastSpawnTime;
    private int customerCounter = 0;
    
    // Events
    public System.Action<Customer> OnCustomerSpawned;
    public System.Action<Customer> OnCustomerServed;
    public System.Action<int> OnCustomerCountChanged;
    
    private void Start()
    {
        SetupSpawner();
        
        if (autoSpawn)
        {
            SpawnCustomer(); // Spawn first customer immediately
        }
    }
    
    private void Update()
    {
        if (autoSpawn)
        {
            UpdateSpawning();
        }
    }
    
    private void SetupSpawner()
    {
        // Setup spawn point if not assigned
        if (spawnPoint == null)
        {
            GameObject spawn = new GameObject("CustomerSpawnPoint");
            spawn.transform.SetParent(transform);
            spawn.transform.localPosition = Vector3.zero;
            spawnPoint = spawn.transform;
        }
        
        // Setup counter position if not assigned
        if (counterPosition == null)
        {
            GameObject counter = GameObject.Find("ServingCounter");
            if (counter != null)
            {
                counterPosition = counter.transform;
            }
        }
        
        DebugLog("Customer spawner initialized");
    }
    
    // ===== SPAWNING LOGIC =====
    
    private void UpdateSpawning()
    {
        if (Time.time - lastSpawnTime >= spawnInterval)
        {
            if (activeCustomers.Count < maxCustomers)
            {
                SpawnCustomer();
            }
        }
    }
    
    public void SpawnCustomer()
    {
        if (customerPrefab == null)
        {
            Debug.LogError("CustomerSpawner: Customer prefab not assigned!");
            return;
        }
        
        if (activeCustomers.Count >= maxCustomers)
        {
            DebugLog("Cannot spawn customer - at max capacity");
            return;
        }
        
        // Spawn customer
        GameObject customerObj = Instantiate(customerPrefab, spawnPoint.position, spawnPoint.rotation);
        Customer customer = customerObj.GetComponent<Customer>();
        
        if (customer == null)
        {
            customer = customerObj.AddComponent<Customer>();
        }
        
        // Configure customer
        string customerName = GetRandomCustomerName();
        customer.name = $"Customer_{customerCounter}_{customerName}";
        
        // Initialize customer
        customer.Initialize(this, counterPosition);
        
        // Set target counter for movement
        ServingCounter servingCounter = FindObjectOfType<ServingCounter>();
        if (servingCounter != null)
        {
            customer.SetTarget(servingCounter);
            DebugLog($"Set target counter for customer: {servingCounter.name}");
        }
        else
        {
            Debug.LogError("No ServingCounter found in scene! Customer will not move.");
        }
        
        // Subscribe to events
        customer.OnCustomerServed += HandleCustomerServed;
        customer.OnCustomerLeft += HandleCustomerLeft;
        
        // Add to active list
        activeCustomers.Add(customer);
        customerCounter++;
        lastSpawnTime = Time.time;
        
        DebugLog($"Spawned customer: {customerName}. Active customers: {activeCustomers.Count}");
        
        OnCustomerSpawned?.Invoke(customer);
        OnCustomerCountChanged?.Invoke(activeCustomers.Count);
    }
    
    // ===== CUSTOMER MANAGEMENT =====
    
    public void OnCustomerDestroyed(Customer customer)
    {
        if (activeCustomers.Contains(customer))
        {
            activeCustomers.Remove(customer);
            OnCustomerCountChanged?.Invoke(activeCustomers.Count);
            DebugLog($"Customer removed. Active customers: {activeCustomers.Count}");
        }
    }
    
    private void HandleCustomerServed(Customer customer)
    {
        DebugLog($"Customer {customer.name} was served");
        OnCustomerServed?.Invoke(customer);
    }
    
    private void HandleCustomerLeft(Customer customer)
    {
        DebugLog($"Customer {customer.name} left");
        OnCustomerDestroyed(customer);
    }
    
    // ===== UTILITY METHODS =====
    
    private string GetRandomCustomerName()
    {
        if (customerNames.Length == 0)
            return "Customer";
        
        int randomIndex = Random.Range(0, customerNames.Length);
        return customerNames[randomIndex];
    }
    
    public Vector3 GetExitPoint()
    {
        // Return spawn point as exit point (customers leave where they came from)
        return spawnPoint.position;
    }
    
    public int GetActiveCustomerCount()
    {
        return activeCustomers.Count;
    }
    
    public List<Customer> GetActiveCustomers()
    {
        return new List<Customer>(activeCustomers);
    }
    
    // ===== DEBUG METHODS =====
    
    [ContextMenu("Spawn Test Customer")]
    public void SpawnTestCustomer()
    {
        SpawnCustomer();
    }
    
    [ContextMenu("Clear All Customers")]
    public void ClearAllCustomers()
    {
        foreach (Customer customer in activeCustomers.ToArray())
        {
            if (customer != null)
            {
                Destroy(customer.gameObject);
            }
        }
        activeCustomers.Clear();
        OnCustomerCountChanged?.Invoke(0);
    }
    
    private void DebugLog(string message)
    {
        if (enableDebugLogs)
        {
            Debug.Log($"[CustomerSpawner] {message}");
        }
    }
    
    // ===== GIZMOS =====
    
    private void OnDrawGizmos()
    {
        // Draw spawn point
        if (spawnPoint != null)
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawWireSphere(spawnPoint.position, 0.5f);
            Gizmos.DrawWireCube(spawnPoint.position + Vector3.up, Vector3.one * 0.3f);
        }
        
        // Draw counter position
        if (counterPosition != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireCube(counterPosition.position + Vector3.up, Vector3.one * 0.4f);
        }
        
        // Draw connection line
        if (spawnPoint != null && counterPosition != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawLine(spawnPoint.position, counterPosition.position);
        }
    }
}
