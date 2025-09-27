using UnityEngine;
using PinePie.SimpleJoystick; // Pinepie Joystick namespace

public class AndroidPlayerController : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float rotationSpeed = 10f;
    
    [Header("Pinepie Joystick Reference")]
    [SerializeField] private JoystickController joystick; // Reference to Pinepie Joystick
    [SerializeField] private string joystickName = "Movement"; // Name of the joystick if using multiple
    
    [Header("Movement Configuration")]
    [SerializeField] private bool useFixedUpdate = true;
    [SerializeField] private bool smoothRotation = true;
    [SerializeField] private float deadZone = 0.1f;
    
    [SerializeField] private bool showGizmos = true;
    
    // Components
    private Rigidbody rb;
    private Camera playerCamera;
    
    // Movement variables
    private Vector2 joystickInput;
    private Vector3 lastMoveDirection;
    
    private void Start()
    {
        InitializeComponents();
        SetupJoystick();
    }
    
    private void InitializeComponents()
    {
        rb = GetComponent<Rigidbody>();
        if (rb == null)
        {
            rb = gameObject.AddComponent<Rigidbody>();
        }
        
        rb.freezeRotation = true;
        rb.linearDamping = 5f;
        rb.mass = 1f;
        rb.angularDamping = 10f;
        
        playerCamera = Camera.main;
        if (playerCamera == null)
        {
            playerCamera = FindObjectOfType<Camera>();
        }
    }
    
    private void SetupJoystick()
    {
        if (joystick == null)
        {
            joystick = FindObjectOfType<JoystickController>();
        }
    }
    
    private void Update()
    {
        if (!useFixedUpdate)
        {
            HandleMovement();
        }
        
        ReadJoystickInput();
    }
    
    private void FixedUpdate()
    {
        if (useFixedUpdate)
        {
            HandleMovement();
        }
    }
    
    private void ReadJoystickInput()
    {
        if (joystick == null) return;
        
        joystickInput = joystick.InputDirection;
    }
    
    private void HandleMovement()
    {
        if (rb == null || joystick == null) return;
        
        if (joystickInput.magnitude > deadZone)
        {
            Vector3 moveDirection = new Vector3(joystickInput.x, 0, joystickInput.y);
            
            if (playerCamera != null)
            {
                moveDirection = ApplyCameraRelativeMovement(moveDirection);
            }
            
            lastMoveDirection = moveDirection;
            
            Vector3 targetVelocity = moveDirection * moveSpeed;
            targetVelocity.y = rb.linearVelocity.y;
            
            rb.linearVelocity = targetVelocity;
            
            if (smoothRotation && moveDirection.magnitude > deadZone)
            {
                HandleRotation(moveDirection);
            }
        }
        else
        {
            Vector3 velocity = new Vector3(0, rb.linearVelocity.y, 0);
            rb.linearVelocity = velocity;
        }
    }
    
    private void HandleRotation(Vector3 moveDirection)
    {
        if (moveDirection.magnitude < deadZone) return;
        
        Quaternion targetRotation = Quaternion.LookRotation(moveDirection);
        float rotationStep = rotationSpeed * (useFixedUpdate ? Time.fixedDeltaTime : Time.deltaTime);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationStep);
    }
    
    private Vector3 ApplyCameraRelativeMovement(Vector3 moveDirection)
    {
        if (playerCamera == null) return moveDirection;
        
        Vector3 cameraForward = playerCamera.transform.forward;
        Vector3 cameraRight = playerCamera.transform.right;
        
        cameraForward.y = 0;
        cameraRight.y = 0;
        
        cameraForward.Normalize();
        cameraRight.Normalize();
        
        return cameraForward * moveDirection.z + cameraRight * moveDirection.x;
    }
    
    #region Public Methods
    
    public void SetMovementEnabled(bool enabled)
    {
        this.enabled = enabled;
        
        if (!enabled)
        {
            if (rb != null)
            {
                rb.linearVelocity = new Vector3(0, rb.linearVelocity.y, 0);
            }
        }
    }
    
    public void SetMoveSpeed(float newSpeed)
    {
        moveSpeed = Mathf.Max(0, newSpeed);
    }
    
    public void SetRotationSpeed(float newSpeed)
    {
        rotationSpeed = Mathf.Max(0, newSpeed);
    }
    
    public bool IsMoving()
    {
        return rb != null && rb.linearVelocity.magnitude > 0.1f;
    }
    
    public Vector3 GetMovementDirection()
    {
        if (rb == null) return Vector3.zero;
        
        Vector3 velocity = rb.linearVelocity;
        velocity.y = 0;
        return velocity.normalized;
    }
    
    public Vector2 GetJoystickInput()
    {
        return joystickInput;
    }
    
    public float GetJoystickMagnitude()
    {
        return joystickInput.magnitude;
    }
    
    public bool IsJoystickActive()
    {
        return joystick != null && joystickInput.magnitude > deadZone;
    }
    
    #endregion
    
    #region Joystick Configuration
    
    public void SetJoystickReference(JoystickController newJoystick)
    {
        joystick = newJoystick;
    }
    
    public void SetDeadZone(float newDeadZone)
    {
        deadZone = Mathf.Clamp01(newDeadZone);
    }
    
    #endregion
    
    #region Debug and Testing
    
    [ContextMenu("Debug Joystick State")]
    private void DebugJoystickState()
    {
    }
    
    [ContextMenu("Test Movement")]
    private void TestMovement()
    {
        if (joystick == null)
        {
            return;
        }
        
        StartCoroutine(TestMovementCoroutine());
    }
    
    private System.Collections.IEnumerator TestMovementCoroutine()
    {
        Vector2 testInput = Vector2.up;
        float testDuration = 2f;
        float elapsed = 0f;
        
        while (elapsed < testDuration)
        {
            joystickInput = testInput;
            
            if (useFixedUpdate)
            {
                HandleMovement();
            }
            
            elapsed += Time.deltaTime;
            yield return null;
        }
        
        joystickInput = Vector2.zero;
    }
    
    #endregion
    
    #region Gizmos
    
    private void OnDrawGizmosSelected()
    {
        if (!showGizmos) return;
        
        if (Application.isPlaying && rb != null)
        {
            Vector3 velocity = rb.linearVelocity;
            velocity.y = 0;
            
            if (velocity.magnitude > 0.1f)
            {
                Gizmos.color = Color.green;
                Gizmos.DrawRay(transform.position, velocity.normalized * 2f);
                Gizmos.DrawWireSphere(transform.position + velocity.normalized * 2f, 0.2f);
            }
        }
        
        if (Application.isPlaying && joystickInput.magnitude > deadZone)
        {
            Vector3 inputDir = new Vector3(joystickInput.x, 0, joystickInput.y);
            
            if (playerCamera != null)
            {
                inputDir = ApplyCameraRelativeMovement(inputDir);
            }
            
            Gizmos.color = Color.blue;
            Vector3 startPos = transform.position + Vector3.up * 0.5f;
            Gizmos.DrawRay(startPos, inputDir * joystickInput.magnitude * 2f);
            Gizmos.DrawWireCube(startPos + inputDir * joystickInput.magnitude * 2f, Vector3.one * 0.1f);
        }
        
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, deadZone);
    }
    
    #endregion
}