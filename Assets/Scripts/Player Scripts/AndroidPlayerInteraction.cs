using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class AndroidPlayerInteraction : MonoBehaviour
{
    [Header("Interaction Settings")]
    [SerializeField] private float interactionRange = 2f;
    [SerializeField] private LayerMask interactableLayer = -1;
    
    [Header("Touch Interaction")]
    [SerializeField] private bool useInteractionButton = true;
    [SerializeField] private bool useDoubleTap = false;
    [SerializeField] private float doubleTapTime = 0.3f;
    [SerializeField] private float tapRadius = 50f; // Screen pixels
    
    [Header("UI References")]
    [SerializeField] private Button interactionButton;
    [SerializeField] private GameObject interactionPrompt;
    [SerializeField] private TextMeshProUGUI interactionText;
    
    [Header("Visual Feedback")]
    [SerializeField] private GameObject interactionHighlight;
    [SerializeField] private float highlightPulseSpeed = 2f;
    
    private IInteractable currentInteractable;
    private Collider currentInteractableCollider;
    private Camera playerCamera;
    
    // Touch interaction variables
    private float lastTapTime = 0f;
    private Vector2 lastTapPosition;
    private bool isInteractionButtonVisible = false;
    
    // Events
    public System.Action<string> OnInteractionAvailable;
    public System.Action OnInteractionUnavailable;
    
    private void Start()
    {
        InitializeComponents();
        SetupUI();
    }
    
    private void InitializeComponents()
    {
        playerCamera = Camera.main;
        if (playerCamera == null)
        {
            playerCamera = FindObjectOfType<Camera>();
        }
        
        if (interactionButton != null)
        {
            interactionButton.onClick.AddListener(OnInteractionButtonPressed);
            interactionButton.gameObject.SetActive(false);
        }
        
        if (interactionPrompt != null)
        {
            interactionPrompt.SetActive(false);
        }
    }
    
    private void SetupUI()
    {
        if (interactionButton != null)
        {
            RectTransform buttonRect = interactionButton.GetComponent<RectTransform>();
            if (buttonRect.sizeDelta.x < 100f || buttonRect.sizeDelta.y < 100f)
            {
                buttonRect.sizeDelta = new Vector2(120f, 120f);
            }
        }
    }
    
    private void Update()
    {
        DetectInteractables();
        UpdateUI();
        
        if (interactionHighlight != null && interactionHighlight.activeInHierarchy)
        {
            AnimateHighlight();
        }
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
            if (interactable != null && interactable.CanInteract())
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
                HideInteractionUI();
            }
            
            currentInteractable = closestInteractable;
            currentInteractableCollider = closestCollider;
            
            if (currentInteractable != null)
            {
                string interactionText = currentInteractable.GetInteractionText();
                OnInteractionAvailable?.Invoke(interactionText);
                ShowInteractionUI(interactionText);
            }
        }
    }
    
    public void HandleTouchInteraction()
    {
        if (!useInteractionButton && Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);
            
            if (touch.phase == TouchPhase.Began)
            {
                if (useDoubleTap)
                {
                    HandleDoubleTapInteraction(touch);
                }
                else
                {
                    HandleDirectTapInteraction(touch);
                }
            }
        }
    }
    
    private void HandleDoubleTapInteraction(Touch touch)
    {
        float currentTime = Time.time;
        float timeSinceLastTap = currentTime - lastTapTime;
        float tapDistance = Vector2.Distance(touch.position, lastTapPosition);
        
        if (timeSinceLastTap <= doubleTapTime && tapDistance <= tapRadius)
        {
            // Double tap detected
            if (currentInteractable != null && currentInteractable.CanInteract())
            {
                PerformInteraction();
            }
            
            lastTapTime = 0f; // Reset to prevent triple tap
        }
        else
        {
            lastTapTime = currentTime;
            lastTapPosition = touch.position;
        }
    }
    
    private void HandleDirectTapInteraction(Touch touch)
    {
        Ray ray = playerCamera.ScreenPointToRay(touch.position);
        RaycastHit hit;
        
        if (Physics.Raycast(ray, out hit, interactionRange, interactableLayer))
        {
            IInteractable tappedInteractable = hit.collider.GetComponent<IInteractable>();
            if (tappedInteractable != null && tappedInteractable.CanInteract())
            {
                PerformInteraction(tappedInteractable);
            }
        }
    }
    
    private void OnInteractionButtonPressed()
    {
        if (currentInteractable != null && currentInteractable.CanInteract())
        {
            PerformInteraction();
            
            #if UNITY_ANDROID && !UNITY_EDITOR
                Handheld.Vibrate();
            #endif
        }
    }
    
    private void PerformInteraction(IInteractable interactable = null)
    {
        IInteractable targetInteractable = interactable ?? currentInteractable;
        
        if (targetInteractable != null)
        {
            targetInteractable.OnInteract();
            UpdateUI();
        }
    }
    
    #region UI Management
    
    private void ShowInteractionUI(string text)
    {
        if (useInteractionButton && interactionButton != null)
        {
            interactionButton.gameObject.SetActive(true);
            isInteractionButtonVisible = true;
            
          /*  TextMeshProUGUI buttonText = interactionButton.GetComponentInChildren<TextMeshProUGUI>();
            if (buttonText != null)
            {
                buttonText.text = text;
            }*/
        }
        
        if (interactionPrompt != null && interactionText != null)
        {
            interactionPrompt.SetActive(true);
            interactionText.text = text;
        }
        
        if (interactionHighlight != null && currentInteractableCollider != null)
        {
            interactionHighlight.SetActive(true);
            interactionHighlight.transform.position = currentInteractableCollider.transform.position;
        }
    }
    
    private void HideInteractionUI()
    {
        if (interactionButton != null)
        {
            interactionButton.gameObject.SetActive(false);
            isInteractionButtonVisible = false;
        }
        
        if (interactionPrompt != null)
        {
            interactionPrompt.SetActive(false);
        }
        
        if (interactionHighlight != null)
        {
            interactionHighlight.SetActive(false);
        }
    }
    
    private void UpdateUI()
    {
        if (currentInteractable != null)
        {
            string text = currentInteractable.GetInteractionText();
            bool canInteract = currentInteractable.CanInteract();
            
            if (interactionButton != null)
            {
                interactionButton.interactable = canInteract;
                
                /*TextMeshProUGUI buttonText = interactionButton.GetComponentInChildren<TextMeshProUGUI>();
                if (buttonText != null)
                {
                    buttonText.text = text;
                    buttonText.color = canInteract ? Color.white : Color.gray;
                }*/
            }
            
            if (interactionText != null)
            {
                interactionText.text = text;
                interactionText.color = canInteract ? Color.white : Color.gray;
            }
        }
    }
    
    private void AnimateHighlight()
    {
        if (interactionHighlight == null) return;
        
        float scale = 1f + Mathf.Sin(Time.time * highlightPulseSpeed) * 0.1f;
        interactionHighlight.transform.localScale = Vector3.one * scale;
        
        if (currentInteractableCollider != null)
        {
            Vector3 targetPos = currentInteractableCollider.bounds.center;
            targetPos.y += currentInteractableCollider.bounds.size.y * 0.6f;
            interactionHighlight.transform.position = targetPos;
        }
    }
    
    #endregion
    
    #region Public Methods
    
    public void SetInteractionEnabled(bool enabled)
    {
        this.enabled = enabled;
        
        if (!enabled)
        {
            HideInteractionUI();
            currentInteractable = null;
            currentInteractableCollider = null;
        }
    }
    
    public void SetInteractionMode(bool useButton)
    {
        useInteractionButton = useButton;
        useDoubleTap = !useButton;
        
        if (!useButton && interactionButton != null)
        {
            interactionButton.gameObject.SetActive(false);
        }
    }
    
    public bool HasInteractable()
    {
        return currentInteractable != null;
    }
    
    public string GetCurrentInteractionText()
    {
        return currentInteractable?.GetInteractionText() ?? "";
    }
    
    #endregion
    
    #region Debug
    
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, interactionRange);
        
        if (currentInteractableCollider != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawLine(transform.position, currentInteractableCollider.transform.position);
        }
    }
    
    #endregion
}