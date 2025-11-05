using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 5f;
    public float rotationSpeed = 10f;
    public float boundarySize = 8f;

    [Header("Animation")]
    public Animator animator;
    [SerializeField] private bool debugAnimations = false;

    [Header("Bean Stacking")]
    public Transform backStackPoint;
    public GameObject beanPrefab;

    private Rigidbody rb;
    private Vector3 movement;
    private bool isMoving;
    private bool wasMoving; // Track previous movement state for animation transitions

    private readonly string SPEED_PARAM = "Speed";
    private readonly string IS_MOVING_PARAM = "IsMoving";
    private readonly string WALK_TRIGGER = "walk";
    private readonly string IDLE_TRIGGER = "idle";

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true;

        if (animator == null)
            animator = GetComponentInChildren<Animator>();

        if (backStackPoint == null)
        {
            GameObject stackPoint = new GameObject("BackStackPoint");
            stackPoint.transform.SetParent(transform);
            stackPoint.transform.localPosition = new Vector3(0, 1.5f, -0.3f);
            backStackPoint = stackPoint.transform;
        }

        // Initialize animation state
        wasMoving = false;

        // Set initial idle state
        if (animator != null && animator.enabled)
        {
            animator.SetTrigger(IDLE_TRIGGER);
        }
    }

    void Update()
    {
        HandleInput();
    }

    void FixedUpdate()
    {
        MovePlayer();
        RotatePlayer();
    }

    void LateUpdate()
    {
        UpdateAnimation();
    }

    private void HandleInput()
    {
        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");

        movement = new Vector3(horizontal, 0f, vertical).normalized;
        isMoving = movement.magnitude > 0.1f;
    }

    private void MovePlayer()
    {
        if (isMoving)
        {
            Vector3 targetPosition = rb.position + movement * moveSpeed * Time.fixedDeltaTime;

            targetPosition.x = Mathf.Clamp(targetPosition.x, -boundarySize, boundarySize);
            targetPosition.z = Mathf.Clamp(targetPosition.z, -boundarySize, boundarySize);

            rb.MovePosition(targetPosition);
        }
    }

    private void RotatePlayer()
    {
        if (isMoving)
        {
            Quaternion targetRotation = Quaternion.LookRotation(movement);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.fixedDeltaTime);
        }
    }

    private void UpdateAnimation()
    {
        if (animator == null || !animator.enabled)
            return;

        // Check for state transitions and trigger animations accordingly
        if (isMoving && !wasMoving)
        {
            // Started moving - trigger walk animation
            animator.ResetTrigger(IDLE_TRIGGER);
            animator.SetTrigger(WALK_TRIGGER);

            if (debugAnimations)
                Debug.Log($"Setting walk trigger. Animator: {(animator != null ? "found" : "null")}, Enabled: {animator.enabled}");
        }
        else if (!isMoving && wasMoving)
        {
            // Stopped moving - trigger idle animation
            animator.ResetTrigger(WALK_TRIGGER);
            animator.SetTrigger(IDLE_TRIGGER);

            if (debugAnimations)
                Debug.Log($"Setting idle trigger. Animator: {(animator != null ? "found" : "null")}, Enabled: {animator.enabled}");
        }

        // Update previous state
        wasMoving = isMoving;
    }

    private bool HasParameter(string paramName)
    {
        if (animator == null || animator.parameters == null)
            return false;

        foreach (AnimatorControllerParameter param in animator.parameters)
        {
            if (param.name == paramName)
                return true;
        }
        return false;
    }

    private bool HasParameter(string paramName, AnimatorControllerParameterType paramType)
    {
        if (animator == null || animator.parameters == null)
            return false;

        foreach (AnimatorControllerParameter param in animator.parameters)
        {
            if (param.name == paramName && param.type == paramType)
                return true;
        }
        return false;
    }

    public void AddBeanToStack(int stackCount)
    {
        if (backStackPoint != null && beanPrefab != null)
        {
            Vector3 stackPosition = backStackPoint.position + Vector3.up * (stackCount * 0.3f);
            GameObject bean = Instantiate(beanPrefab, stackPosition, Quaternion.identity, backStackPoint);
            bean.name = $"Bean_Stack_{stackCount}";
        }
    }

    public void ClearBeanStack()
    {
        if (backStackPoint != null)
        {
            for (int i = backStackPoint.childCount - 1; i >= 0; i--)
            {
                DestroyImmediate(backStackPoint.GetChild(i).gameObject);
            }
        }
    }
}