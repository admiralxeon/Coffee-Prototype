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
            if (currentInteractable != null && currentInteractable.CanInteract())
            {
                currentInteractable.OnInteract();
                
                if (!currentInteractable.CanInteract())
                {
                    OnInteractionUnavailable?.Invoke();
                }
                else
                {
                    OnInteractionAvailable?.Invoke(currentInteractable.GetInteractionText());
                }
            }
        }
    }
}
