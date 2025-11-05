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

        if (speechBubble != null)
        {
            speechBubble.SetActive(false);
        }
    }

    private void Start()
    {
        // Ensure destination is set once agent is on NavMesh
        StartCoroutine(WaitForNavMeshAndSetDestination());
    }

    private void Update()
    {
        UpdateCustomerBehavior();
        UpdateSpeechBubbleRotation();
    }

    private IEnumerator WaitForNavMeshAndSetDestination()
    {

        float timeout = 5f;
        float elapsed = 0f;

        while (agent != null && !agent.isOnNavMesh && elapsed < timeout)
        {
            elapsed += Time.deltaTime;
            yield return null;
        }

        if (agent != null && agent.isOnNavMesh)
        {
            // Once on NavMesh, try to set destination if we have a target
            TrySetDestinationToTarget();
        }
        else
        {
            Debug.LogError($"Customer {name} failed to get on NavMesh after {timeout} seconds!");
        }
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
        if (agent == null)
        {
            return;
        }

        // If agent is not moving and should be, try to set destination again
        if (agent.isOnNavMesh && !agent.pathPending && !agent.hasPath)
        {
            TrySetDestinationToTarget();
            return;
        }

        // Check if customer has reached the destination
        // Also check if customer is close enough to counter even if path is blocked
        if (agent.isOnNavMesh)
        {
            bool hasReachedDestination = !agent.pathPending && agent.hasPath && agent.remainingDistance < 1.5f;

            // Also check direct distance to counter if pathfinding is blocked
            bool isCloseToCounter = false;
            if (targetCounter != null && targetCounter.GetServingPoint() != null)
            {
                float distanceToCounter = Vector3.Distance(transform.position, targetCounter.GetServingPoint().position);
                isCloseToCounter = distanceToCounter < 2.5f; // Slightly larger than pathfinding check
            }
            else if (targetPosition != null)
            {
                float distanceToCounter = Vector3.Distance(transform.position, targetPosition.position);
                isCloseToCounter = distanceToCounter < 2.5f;
            }

            if (hasReachedDestination || isCloseToCounter)
            {
                StartWaitingForOrder();
            }
        }
    }

    private void HandleWaitingForOrder()
    {
        patienceTimer -= Time.deltaTime;
        UpdatePatienceDisplay();

        if (patienceTimer <= 0f)
        {
            StartLeaving(false);
        }
        if (patienceTimer < patienceTime * 0.3f && Random.Range(0f, 100f) < 1f)
        {
            PlaySound(impatientSound);
        }
    }

    private void HandleOrderReceived()
    {
        StartLeaving(true);
    }

    private void HandleLeaving()
    {
        if (agent != null && agent.isOnNavMesh && !agent.pathPending && agent.hasPath && agent.remainingDistance < 1f)
        {
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

        // Ensure agent is enabled and not stopped
        if (agent != null)
        {
            agent.isStopped = false;
        }

        TrySetDestinationToTarget();

        // Also try setting destination after a short delay in case NavMesh isn't ready
        StartCoroutine(DelayedSetDestination());
    }

    private IEnumerator DelayedSetDestination()
    {
        yield return new WaitForSeconds(0.1f);
        TrySetDestinationToTarget();
    }

    private void TrySetDestinationToTarget()
    {
        if (agent == null)
        {
            agent = GetComponent<NavMeshAgent>();
            if (agent == null)
            {
                Debug.LogWarning("Customer has no NavMeshAgent component!");
                return;
            }
        }

        // Wait for agent to be on NavMesh
        if (!agent.isOnNavMesh)
        {
            return; // Will be retried by WaitForNavMeshAndSetDestination coroutine
        }

        Vector3 destination = Vector3.zero;
        bool hasDestination = false;

        // Try to get destination from serving counter
        if (targetCounter != null && targetCounter.GetServingPoint() != null)
        {
            destination = targetCounter.GetServingPoint().position;
            hasDestination = true;
        }
        // Fallback to target position if counter doesn't have serving point
        else if (targetPosition != null)
        {
            destination = targetPosition.position;
            hasDestination = true;
        }

        if (hasDestination)
        {
            agent.isStopped = false;
            agent.SetDestination(destination);

            // Debug log to help diagnose issues
            if (!agent.hasPath)
            {
                Debug.LogWarning($"Customer {name} failed to set path to destination: {destination}. NavMesh available: {agent.isOnNavMesh}");
            }
        }
        else
        {
            Debug.LogWarning($"Customer {name} has no target destination! TargetCounter: {targetCounter != null}, TargetPosition: {targetPosition != null}");
        }
    }

    public void SetSpawner(CustomerSpawner customerSpawner)
    {
        spawner = customerSpawner;
    }

    private void StartWaitingForOrder()
    {
        // Only transition if we're not already waiting or received order
        if (currentState == CustomerState.WaitingForOrder || currentState == CustomerState.OrderReceived)
        {
            return;
        }

        currentState = CustomerState.WaitingForOrder;
        if (agent != null)
        {
            agent.isStopped = true;
        }
        ShowOrder();
        PlaySound(orderSound);
        Debug.Log($"Customer {name} is now waiting for order at position {transform.position}");
    }

    public void ReceiveOrder()
    {
        // If already waiting, proceed directly
        if (currentState == CustomerState.WaitingForOrder)
        {
            // Proceed to serve
        }
        // If moving to counter, check if close enough and transition
        else if (currentState == CustomerState.MovingToCounter)
        {
            float distanceToCounter = GetDistanceToCounter();

            // Use 4.0f to be more lenient - if ServingCounter detected them, they should be served
            if (distanceToCounter <= 4.0f)
            {
                Debug.Log($"Customer {name} is close enough to counter ({distanceToCounter:F2} units) but in {currentState} state. Transitioning to WaitingForOrder to receive order.");
                // Force transition to waiting state
                currentState = CustomerState.WaitingForOrder;
                if (agent != null)
                {
                    agent.isStopped = true;
                }
            }
            else
            {
                Debug.LogWarning($"Customer {name} cannot receive order - in {currentState} state and too far ({distanceToCounter:F2} units). Expected <= 4.0f. This shouldn't happen if ServingCounter detected them correctly.");
                return;
            }
        }
        // If already received order or leaving, don't serve again
        else if (currentState == CustomerState.OrderReceived || currentState == CustomerState.Leaving)
        {
            Debug.LogWarning($"Customer {name} cannot receive order - already in {currentState} state.");
            return;
        }
        // Any other state is invalid
        else
        {
            Debug.LogWarning($"Customer {name} cannot receive order - invalid state: {currentState}");
            return;
        }

        // If we reach here, the customer can receive the order
        currentState = CustomerState.OrderReceived;
        ShowThankYou();
        PlaySound(thankYouSound);

        // Check if GameManager exists and add money
        if (GameManager.Instance != null)
        {
            GameManager.Instance.AddMoney(paymentAmount);
            Debug.Log($"Customer {name} served! Added ${paymentAmount}. Total money: ${GameManager.Instance.GetMoney()}");
        }
        else
        {
            Debug.LogError("GameManager.Instance is null! Cannot add money. Make sure GameManager exists in the scene.");
        }

        OnCustomerServed?.Invoke(this);
        StartCoroutine(DelayedLeaving());
    }

    private float GetDistanceToCounter()
    {
        if (targetCounter != null && targetCounter.GetServingPoint() != null)
        {
            return Vector3.Distance(transform.position, targetCounter.GetServingPoint().position);
        }
        else if (targetPosition != null)
        {
            return Vector3.Distance(transform.position, targetPosition.position);
        }
        return float.MaxValue;
    }

    private IEnumerator DelayedLeaving()
    {
        yield return new WaitForSeconds(2f);
        StartLeaving(true);
    }

    private void StartLeaving(bool satisfied)
    {
        currentState = CustomerState.Leaving;

        if (agent != null)
        {
            agent.isStopped = false;

            if (agent.isOnNavMesh)
            {
                Vector3 exitPoint = FindExitPoint();
                agent.SetDestination(exitPoint);
            }
        }

        if (speechBubble != null)
        {
            speechBubble.SetActive(false);
        }

        OnCustomerLeft?.Invoke(this);
    }

    private Vector3 FindExitPoint()
    {
        Vector3 counterPos = targetCounter != null ? targetCounter.transform.position : transform.position;
        Vector3 direction = (transform.position - counterPos).normalized;
        Vector3 exitPoint = counterPos + direction * 20f;

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
                orderText.text = Time.time % 1f < 0.5f ? "I'd like coffee!" : "Where's my coffee?!";
            }
        }
    }

    private void UpdateSpeechBubbleRotation()
    {
        if (speechBubble != null && Camera.main != null)
        {
            speechBubble.transform.LookAt(Camera.main.transform);
            speechBubble.transform.Rotate(0, 180, 0);
        }
    }

    private void PlaySound(AudioClip clip)
    {
        if (audioSource != null && clip != null)
        {
            audioSource.PlayOneShot(clip);
        }
    }

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

        if (currentState == CustomerState.WaitingForOrder)
        {
            Gizmos.color = patienceTimer > patienceTime * 0.5f ? Color.green : Color.red;
            Gizmos.DrawWireSphere(transform.position + Vector3.up * 2.5f, 0.5f);
        }
    }

    public void Initialize(CustomerSpawner customerSpawner, Transform counterPosition)
    {
        spawner = customerSpawner;
        targetPosition = counterPosition;

        // Try to find the ServingCounter component if we have a counter position
        if (counterPosition != null)
        {
            ServingCounter counter = counterPosition.GetComponent<ServingCounter>();
            if (counter == null)
            {
                counter = counterPosition.GetComponentInParent<ServingCounter>();
            }
            if (counter == null)
            {
                counter = FindObjectOfType<ServingCounter>();
            }

            if (counter != null)
            {
                SetTarget(counter);
            }
        }
    }
}