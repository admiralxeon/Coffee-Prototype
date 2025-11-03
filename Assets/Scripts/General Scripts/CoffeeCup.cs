using UnityEngine;

public class CoffeeCup : MonoBehaviour, IInteractable
{
    [Header("Coffee Cup Settings")]
    [SerializeField] private bool canBePickedUp = true;

    [Header("Effects")]
    [SerializeField] private AudioClip pickupSound;

    private CoffeeMachine parentMachine;
    private AudioSource audioSource;

    public void Initialize(CoffeeMachine machine)
    {
        parentMachine = machine;

        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.spatialBlend = 1f;
            audioSource.playOnAwake = false;
        }

        // Ensure the cup has a collider for interaction detection
        Collider existingCollider = GetComponent<Collider>();
        if (existingCollider == null)
        {
            BoxCollider collider = gameObject.AddComponent<BoxCollider>();
            collider.isTrigger = false;
            collider.size = new Vector3(0.3f, 0.4f, 0.3f); // Larger size for better detection
            collider.center = new Vector3(0, 0.2f, 0); // Center it on the cup
        }
        else
        {
            // Ensure existing collider is not a trigger and properly sized
            existingCollider.isTrigger = false;

            // If it's a BoxCollider, ensure it's a reasonable size
            if (existingCollider is BoxCollider boxCollider)
            {
                if (boxCollider.size.magnitude < 0.1f)
                {
                    boxCollider.size = new Vector3(0.3f, 0.4f, 0.3f);
                    boxCollider.center = new Vector3(0, 0.2f, 0);
                }
            }
        }

        // Add a kinematic rigidbody to ensure proper collision detection
        // This helps with Physics.OverlapSphere detection
        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb == null)
        {
            rb = gameObject.AddComponent<Rigidbody>();
            rb.isKinematic = true; // Don't need physics, just detection
            rb.useGravity = false;
            rb.constraints = RigidbodyConstraints.FreezeAll; // Prevent any movement
        }
        else
        {
            // Ensure existing rigidbody doesn't interfere
            rb.isKinematic = true;
            rb.useGravity = false;
            rb.constraints = RigidbodyConstraints.FreezeAll;
        }
    }


    public string GetInteractionText()
    {
        if (!canBePickedUp)
            return "Coffee Cup";

        return "Press E - Take Coffee";
    }

    public bool CanInteract()
    {
        if (!canBePickedUp)
            return false;

        PlayerInventory playerInventory = FindObjectOfType<PlayerInventory>();
        if (playerInventory == null)
            return false;

        return playerInventory.CanCarryItem(ItemType.CoffeeCup);
    }

    public void OnInteract()
    {
        if (!CanInteract())
            return;

        PlayerInventory playerInventory = FindObjectOfType<PlayerInventory>();
        if (playerInventory == null)
            return;

        if (playerInventory.TryAddItem(ItemType.CoffeeCup))
        {
            if (audioSource != null && pickupSound != null)
            {
                audioSource.PlayOneShot(pickupSound);
            }

            if (parentMachine != null)
            {
                parentMachine.RemoveCupFromMachine(gameObject);
            }

            Destroy(gameObject, 0.1f);
        }
    }


    private void OnDrawGizmos()
    {
        Gizmos.color = canBePickedUp ? Color.yellow : Color.gray;
        Gizmos.DrawWireCube(transform.position + Vector3.up * 0.1f, Vector3.one * 0.15f);
    }
}
