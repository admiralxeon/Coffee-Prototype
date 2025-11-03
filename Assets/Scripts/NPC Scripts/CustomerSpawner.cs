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
    [SerializeField] private string[] customerNames = { "Alex", "Sam", "Jordan", "Casey", "Taylor" };

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
            SpawnCustomer();
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
        if (spawnPoint == null)
        {
            GameObject spawn = new GameObject("CustomerSpawnPoint");
            spawn.transform.SetParent(transform);
            spawn.transform.localPosition = Vector3.zero;
            spawnPoint = spawn.transform;
        }

        if (counterPosition == null)
        {
            GameObject counter = GameObject.Find("ServingCounter");
            if (counter != null)
            {
                counterPosition = counter.transform;
            }
        }
    }


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
            return;
        }

        if (activeCustomers.Count >= maxCustomers)
        {
            return;
        }

        GameObject customerObj = Instantiate(customerPrefab, spawnPoint.position, spawnPoint.rotation);
        Customer customer = customerObj.GetComponent<Customer>();

        if (customer == null)
        {
            customer = customerObj.AddComponent<Customer>();
        }

        string customerName = GetRandomCustomerName();
        customer.name = $"Customer_{customerCounter}_{customerName}";

        // Initialize with spawner and counter position
        customer.Initialize(this, counterPosition);

        // Also explicitly set the serving counter as target
        ServingCounter servingCounter = FindObjectOfType<ServingCounter>();
        if (servingCounter != null)
        {
            customer.SetTarget(servingCounter);
        }
        else
        {
            Debug.LogWarning($"CustomerSpawner: No ServingCounter found in scene! Customer {customer.name} may not move correctly.");
        }

        // Ensure the customer starts in MovingToCounter state
        // This should already be set in Awake, but double-check

        customer.OnCustomerServed += HandleCustomerServed;
        customer.OnCustomerLeft += HandleCustomerLeft;

        activeCustomers.Add(customer);
        customerCounter++;
        lastSpawnTime = Time.time;

        OnCustomerSpawned?.Invoke(customer);
        OnCustomerCountChanged?.Invoke(activeCustomers.Count);
    }


    public void OnCustomerDestroyed(Customer customer)
    {
        if (activeCustomers.Contains(customer))
        {
            activeCustomers.Remove(customer);
            OnCustomerCountChanged?.Invoke(activeCustomers.Count);
        }
    }

    private void HandleCustomerServed(Customer customer)
    {
        OnCustomerServed?.Invoke(customer);
    }

    private void HandleCustomerLeft(Customer customer)
    {
        OnCustomerDestroyed(customer);
    }


    private string GetRandomCustomerName()
    {
        if (customerNames.Length == 0)
            return "Customer";

        int randomIndex = Random.Range(0, customerNames.Length);
        return customerNames[randomIndex];
    }

    public Vector3 GetExitPoint()
    {
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



    private void OnDrawGizmos()
    {
        if (spawnPoint != null)
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawWireSphere(spawnPoint.position, 0.5f);
            Gizmos.DrawWireCube(spawnPoint.position + Vector3.up, Vector3.one * 0.3f);
        }

        if (counterPosition != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireCube(counterPosition.position + Vector3.up, Vector3.one * 0.4f);
        }

        if (spawnPoint != null && counterPosition != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawLine(spawnPoint.position, counterPosition.position);
        }
    }
}
