using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 5f;
    public float rotationSpeed = 10f;
    public float boundarySize = 8f;
    
    [Header("Animation")]
    public Animator animator;
    
    [Header("Bean Stacking")]
    public Transform backStackPoint; 
    public GameObject beanPrefab; 
    
    private Rigidbody rb;
    private Vector3 movement;
    private bool isMoving;
    
    private readonly string SPEED_PARAM = "Speed";
    private readonly string IS_MOVING_PARAM = "IsMoving";
    
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
    }
    
    void Update()
    {
        HandleInput();
        UpdateAnimation();
    }
    
    void FixedUpdate()
    {
        MovePlayer();
        RotatePlayer();
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
        if (animator != null)
        {
            float speedValue = isMoving ? 1f : 0f;
            animator.SetBool(IS_MOVING_PARAM, isMoving);
        }
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