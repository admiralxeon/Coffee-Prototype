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

        // Separate cups from other interactables to prioritize them
        System.Collections.Generic.List<(IInteractable interactable, Collider collider, float distance)> cups =
            new System.Collections.Generic.List<(IInteractable, Collider, float)>();
        System.Collections.Generic.List<(IInteractable interactable, Collider collider, float distance)> others =
            new System.Collections.Generic.List<(IInteractable, Collider, float)>();

        foreach (Collider col in nearbyColliders)
        {
            IInteractable interactable = col.GetComponent<IInteractable>();
            if (interactable != null && interactable.CanInteract())
            {
                // Skip machines that have coffee ready - prioritize the coffee cup instead
                CoffeeMachine machine = interactable as CoffeeMachine;
                if (machine != null && machine.HasCoffeeReady())
                {
                    continue; // Skip machines with coffee ready, interact with the cup instead
                }

                float distance = Vector3.Distance(transform.position, col.transform.position);

                // Check if this is a coffee cup
                CoffeeCup cup = interactable as CoffeeCup;
                if (cup != null)
                {
                    cups.Add((interactable, col, distance));
                }
                else
                {
                    others.Add((interactable, col, distance));
                }
            }
        }

        // Prioritize coffee cups over other interactables
        if (cups.Count > 0)
        {
            // Find closest cup
            foreach (var (interactable, collider, distance) in cups)
            {
                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    closestInteractable = interactable;
                    closestCollider = collider;
                }
            }
        }
        else
        {
            // No cups found in initial scan - try additional detection for cups on machines
            // Check nearby machines for cups that might be blocked by machine colliders
            CoffeeMachine[] nearbyMachines = FindObjectsOfType<CoffeeMachine>();
            foreach (CoffeeMachine machine in nearbyMachines)
            {
                if (machine.HasCoffeeReady())
                {
                    float machineDistance = Vector3.Distance(transform.position, machine.transform.position);
                    if (machineDistance <= interactionRange * 1.5f)
                    {
                        // Machine is nearby and has coffee - check its cups directly
                        var machineCups = machine.GetCupsOnMachine();
                        foreach (GameObject cupObj in machineCups)
                        {
                            if (cupObj != null)
                            {
                                CoffeeCup cup = cupObj.GetComponent<CoffeeCup>();
                                if (cup != null && cup.CanInteract())
                                {
                                    float cupDistance = Vector3.Distance(transform.position, cupObj.transform.position);
                                    if (cupDistance < closestDistance)
                                    {
                                        Collider cupCollider = cupObj.GetComponent<Collider>();
                                        if (cupCollider != null)
                                        {
                                            closestDistance = cupDistance;
                                            closestInteractable = cup;
                                            closestCollider = cupCollider;
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }

            // Also try casting upward from player position as backup
            if (closestInteractable == null)
            {
                Vector3 castStart = transform.position + Vector3.up * 0.5f; // Start slightly above player
                Collider[] upwardColliders = Physics.OverlapSphere(castStart, interactionRange * 1.2f, interactableLayer);

                foreach (Collider col in upwardColliders)
                {
                    CoffeeCup cup = col.GetComponent<CoffeeCup>();
                    if (cup != null && cup.CanInteract())
                    {
                        float distance = Vector3.Distance(transform.position, col.transform.position);
                        if (distance < closestDistance)
                        {
                            closestDistance = distance;
                            closestInteractable = cup;
                            closestCollider = col;
                        }
                    }
                }
            }

            // If still no cup found, use other interactables
            if (closestInteractable == null)
            {
                foreach (var (interactable, collider, distance) in others)
                {
                    if (distance < closestDistance)
                    {
                        closestDistance = distance;
                        closestInteractable = interactable;
                        closestCollider = collider;
                    }
                }
            }
        }

        // Always check if interactability state changed for the same object
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
        else if (currentInteractable != null)
        {
            // Same interactable, but check if interactability changed
            bool canInteractNow = currentInteractable.CanInteract();
            // This will be called every frame, but only update UI if state changed
            // We'll update the text to ensure it's current
            if (canInteractNow)
            {
                OnInteractionAvailable?.Invoke(currentInteractable.GetInteractionText());
            }
            else
            {
                OnInteractionUnavailable?.Invoke();
            }
        }
        else if (currentInteractable == null && closestInteractable == null)
        {
            // Make sure UI is cleared when no interactable
            OnInteractionUnavailable?.Invoke();
        }
    }

    private void HandleInput()
    {
        if (Input.GetKeyDown(interactKey))
        {
            if (currentInteractable != null)
            {
                if (currentInteractable.CanInteract())
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

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, interactionRange);

        if (currentInteractableCollider != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawLine(transform.position, currentInteractableCollider.transform.position);
        }
    }
}