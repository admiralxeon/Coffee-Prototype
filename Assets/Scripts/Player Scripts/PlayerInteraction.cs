using UnityEngine;

public class PlayerInteraction : MonoBehaviour
{
    [Header("Interaction Settings")]
    [SerializeField] private float interactionRange = 2f;
    [SerializeField] private LayerMask interactableLayer = -1;
    [SerializeField] private KeyCode interactKey = KeyCode.E;
    
    private IInteractable currentInteractable;
    private Collider currentInteractableCollider;
    
    public System.Action<string> OnInteractionAvailable;
    public System.Action OnInteractionUnavailable;
    
    private void Update()
    {
        DetectInteractables();
        HandleInput();
    }
    
    private void DetectInteractables()
    {
        Collider[] nearbyColliders = Physics.OverlapSphere(
            transform.position, 
            interactionRange, 
            interactableLayer
        );
        
        IInteractable closestInteractable = null;
        float closestDistance = float.MaxValue;
        Collider closestCollider = null;
        
        foreach (Collider col in nearbyColliders)
        {
            IInteractable interactable = col.GetComponent<IInteractable>();
            if (interactable != null)
            {
                float distance = Vector3.Distance(transform.position, col.transform.position);
                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    closestInteractable = interactable;
                    closestCollider = col;
                }
            }
        }
        
        if (closestInteractable != currentInteractable)
        {
            if (currentInteractable != null)
            {
                OnInteractionUnavailable?.Invoke();
            }
            
            currentInteractable = closestInteractable;
            currentInteractableCollider = closestCollider;
            
            if (currentInteractable != null && currentInteractable.CanInteract())
            {
                OnInteractionAvailable?.Invoke(currentInteractable.GetInteractionText());
            }
        }
    }
    
    private void HandleInput()
    {
        if (Input.GetKeyDown(interactKey))
        {
            Debug.Log($"Interaction key ({interactKey}) pressed!");
            
            if (currentInteractable != null)
            {
                Debug.Log($"Current interactable: {currentInteractableCollider.name}");
                Debug.Log($"Can interact: {currentInteractable.CanInteract()}");
                Debug.Log($"Interaction text: {currentInteractable.GetInteractionText()}");
                
                if (currentInteractable.CanInteract())
                {
                    Debug.Log("Calling OnInteract()...");
                    currentInteractable.OnInteract();
                    Debug.Log("OnInteract() completed!");
                    
                    // Update UI after interaction
                    if (!currentInteractable.CanInteract())
                    {
                        OnInteractionUnavailable?.Invoke();
                    }
                    else
                    {
                        OnInteractionAvailable?.Invoke(currentInteractable.GetInteractionText());
                    }
                }
                else
                {
                    Debug.Log("Cannot interact with this object right now");
                }
            }
            else
            {
                Debug.Log("No interactable object detected");
            }
        }
    }
    
    // Debug method to show current state
    private void OnDrawGizmos()
    {
        // Show interaction range
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, interactionRange);
        
        // Show line to current interactable
        if (currentInteractableCollider != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawLine(transform.position, currentInteractableCollider.transform.position);
        }
    }
}