using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;
using System.Collections;

public class Customer : MonoBehaviour
{
    [Header("Customer Settings")]
    [SerializeField] private float patienceTime = 30f;
    [SerializeField] private int paymentAmount = 10;
    
    [Header("UI References")]
    [SerializeField] private GameObject speechBubble;
    [SerializeField] private Text orderText;
    
    [Header("Audio")]
    [SerializeField] private AudioClip orderSound;
    [SerializeField] private AudioClip thankYouSound;
    [SerializeField] private AudioClip impatientSound;
    
    private NavMeshAgent agent;
    private AudioSource audioSource;
    private CustomerState currentState;
    private float patienceTimer;
    private ServingCounter targetCounter;
    private CustomerSpawner spawner;
    private Transform targetPosition;
    
    // Events
    public System.Action<Customer> OnCustomerServed;
    public System.Action<Customer> OnCustomerLeft;
    
    // Customer states
    public enum CustomerState
    {
        MovingToCounter,
        WaitingForOrder,
        OrderReceived,
        Leaving
    }
    
    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        audioSource = GetComponent<AudioSource>();
        
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
        
        currentState = CustomerState.MovingToCounter;
        patienceTimer = patienceTime;
        
        // Hide speech bubble initially
        if (speechBubble != null)
        {
            speechBubble.SetActive(false);
        }
    }
    
    private void Update()
    {
        UpdateCustomerBehavior();
        UpdateSpeechBubbleRotation();
    }
    
    private void UpdateCustomerBehavior()
    {
        switch (currentState)
        {
            case CustomerState.MovingToCounter:
                HandleMovingToCounter();
                break;
                
            case CustomerState.WaitingForOrder:
                HandleWaitingForOrder();
                break;
                
            case CustomerState.OrderReceived:
                HandleOrderReceived();
                break;
                
            case CustomerState.Leaving:
                HandleLeaving();
                break;
        }
    }
    
    private void HandleMovingToCounter()
    {
        if (!agent.pathPending && agent.remainingDistance < 1.5f)
        {
            // Reached the counter
            StartWaitingForOrder();
        }
    }
    
    private void HandleWaitingForOrder()
    {
        // Decrease patience
        patienceTimer -= Time.deltaTime;
        
        // Update patience display
        UpdatePatienceDisplay();
        
        // Check if patience runs out
        if (patienceTimer <= 0f)
        {
            StartLeaving(false); // Leave without being served
        }
        
        // Play impatient sound occasionally
        if (patienceTimer < patienceTime * 0.3f && Random.Range(0f, 100f) < 1f)
        {
            PlaySound(impatientSound);
        }
    }
    
    private void HandleOrderReceived()
    {
        // Customer received their order, start leaving
        StartLeaving(true);
    }
    
    private void HandleLeaving()
    {
        if (!agent.pathPending && agent.remainingDistance < 1f)
        {
            // Reached exit point, destroy customer
            if (spawner != null)
            {
                spawner.OnCustomerDestroyed(this);
            }
            Destroy(gameObject);
        }
    }
    
    public void SetTarget(ServingCounter counter)
    {
        targetCounter = counter;
        if (counter != null && counter.GetServingPoint() != null)
        {
            agent.SetDestination(counter.GetServingPoint().position);
        }
    }
    
    public void SetSpawner(CustomerSpawner customerSpawner)
    {
        spawner = customerSpawner;
    }
    
    private void StartWaitingForOrder()
    {
        currentState = CustomerState.WaitingForOrder;
        agent.isStopped = true;
        
        // Show speech bubble with order
        ShowOrder();
        PlaySound(orderSound);
        
        Debug.Log("Customer is waiting for order");
    }
    
    public void ReceiveOrder()
    {
        if (currentState != CustomerState.WaitingForOrder) return;
        
        currentState = CustomerState.OrderReceived;
        
        // Hide order, show thank you
        ShowThankYou();
        PlaySound(thankYouSound);
        
        // Add money to player
        GameManager.Instance?.AddMoney(paymentAmount);
        
        Debug.Log($"Customer received order and paid ${paymentAmount}");
        
        // Invoke served event
        OnCustomerServed?.Invoke(this);
        
        // Start leaving after a short delay
        StartCoroutine(DelayedLeaving());
    }
    
    private IEnumerator DelayedLeaving()
    {
        yield return new WaitForSeconds(2f);
        StartLeaving(true);
    }
    
    private void StartLeaving(bool satisfied)
    {
        currentState = CustomerState.Leaving;
        agent.isStopped = false;
        
        // Hide speech bubble
        if (speechBubble != null)
        {
            speechBubble.SetActive(false);
        }
        
        // Move to a random exit point (or back to spawn)
        Vector3 exitPoint = FindExitPoint();
        agent.SetDestination(exitPoint);
        
        // Invoke left event
        OnCustomerLeft?.Invoke(this);
        
        if (!satisfied)
        {
            Debug.Log("Customer left unsatisfied (patience ran out)");
        }
    }
    
    private Vector3 FindExitPoint()
    {
        // Find a point far from the counter to leave towards
        Vector3 counterPos = targetCounter != null ? targetCounter.transform.position : transform.position;
        Vector3 direction = (transform.position - counterPos).normalized;
        Vector3 exitPoint = counterPos + direction * 20f;
        
        // Make sure the exit point is on the NavMesh
        NavMeshHit hit;
        if (NavMesh.SamplePosition(exitPoint, out hit, 10f, NavMesh.AllAreas))
        {
            return hit.position;
        }
        
        return transform.position + direction * 10f;
    }
    
    private void ShowOrder()
    {
        if (speechBubble != null && orderText != null)
        {
            speechBubble.SetActive(true);
            orderText.text = "I'd like coffee!";
            orderText.color = Color.white;
        }
    }
    
    private void ShowThankYou()
    {
        if (speechBubble != null && orderText != null)
        {
            orderText.text = "Thank you!";
            orderText.color = Color.green;
        }
    }
    
    private void UpdatePatienceDisplay()
    {
        if (orderText != null && currentState == CustomerState.WaitingForOrder)
        {
            float patiencePercent = patienceTimer / patienceTime;
            
            if (patiencePercent > 0.6f)
            {
                orderText.color = Color.white;
            }
            else if (patiencePercent > 0.3f)
            {
                orderText.color = Color.yellow;
            }
            else
            {
                orderText.color = Color.red;
                // Make text blink when very impatient
                orderText.text = Time.time % 1f < 0.5f ? "I'd like coffee!" : "Where's my coffee?!";
            }
        }
    }
    
    private void UpdateSpeechBubbleRotation()
    {
        // Make speech bubble always face the camera
        if (speechBubble != null && Camera.main != null)
        {
            speechBubble.transform.LookAt(Camera.main.transform);
            speechBubble.transform.Rotate(0, 180, 0); // Flip to face camera correctly
        }
    }
    
    private void PlaySound(AudioClip clip)
    {
        if (audioSource != null && clip != null)
        {
            audioSource.PlayOneShot(clip);
        }
    }
    
    // Public methods for external checking
    public bool IsWaitingForOrder()
    {
        return currentState == CustomerState.WaitingForOrder;
    }
    
    public CustomerState GetCurrentState()
    {
        return currentState;
    }
    
    public float GetPatiencePercent()
    {
        return patienceTimer / patienceTime;
    }
    
    // Visual debugging
    private void OnDrawGizmosSelected()
    {
        if (agent != null && agent.hasPath)
        {
            Gizmos.color = Color.blue;
            Vector3[] path = agent.path.corners;
            for (int i = 0; i < path.Length - 1; i++)
            {
                Gizmos.DrawLine(path[i], path[i + 1]);
            }
        }
        
        // Draw patience indicator
        if (currentState == CustomerState.WaitingForOrder)
        {
            Gizmos.color = patienceTimer > patienceTime * 0.5f ? Color.green : Color.red;
            Gizmos.DrawWireSphere(transform.position + Vector3.up * 2.5f, 0.5f);
        }
    }
    
    // ===== INITIALIZATION =====
    
    public void Initialize(CustomerSpawner customerSpawner, Transform counterPosition)
    {
        spawner = customerSpawner;
        targetPosition = counterPosition;
        Debug.Log($"Customer initialized with spawner and counter position");
    }
}