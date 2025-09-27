using UnityEngine;
using System.Collections.Generic;
using System.Collections;

public class PlayerInventory : MonoBehaviour, IItemCarrier
{
   [Header("Capacity Settings")]
    [SerializeField] private int maxBeans = 3;
    [SerializeField] private int maxCoffees = 5;
    
    [Header("Bean Stacking Visuals")]
    [SerializeField] private Transform stackingParent;
    [SerializeField] private GameObject beanStackPrefab;
    [SerializeField] private Vector3 stackOffset = new Vector3(0, 1.5f, -0.3f);
    [SerializeField] private float stackSpacing = 0.2f;
    
    [Header("Animation")]
    [SerializeField] private float stackAnimationDuration = 0.3f;
    [SerializeField] private AnimationCurve stackCurve = new AnimationCurve(
        new Keyframe(0f, 0f, 0f, 0f),
        new Keyframe(0.7f, 1.1f, 0f, 0f),
        new Keyframe(1f, 1f, 0f, 0f)
    );
    
    [Header("Debug")]
    [SerializeField] private bool enableDebugLogs = true;
    
    // Inventory data
    private Dictionary<ItemType, int> inventory = new Dictionary<ItemType, int>();
    private List<GameObject> visualBeanStack = new List<GameObject>();
    
    // Events
    public System.Action<ItemType, int> OnInventoryChanged;
    public System.Action<int, int> OnBeanCountChanged; // current, max
    
    private void Awake()
    {
        InitializeInventory();
        SetupStackingParent();
    }
    
    private void InitializeInventory()
    {
        inventory[ItemType.CoffeeBean] = 0;
        inventory[ItemType.CoffeeCup] = 0;
        inventory[ItemType.Money] = 0;
        
        DebugLog("Inventory initialized");
    }
    
    private void SetupStackingParent()
    {
        if (stackingParent == null)
        {
            GameObject stackParent = new GameObject("BeanStack");
            stackParent.transform.SetParent(transform);
            stackParent.transform.localPosition = stackOffset;
            stackingParent = stackParent.transform;
            DebugLog("Created bean stacking parent");
        }
    }
    
    // ===== IITEMCARRIER IMPLEMENTATION =====
    
    public bool CanCarryItem(ItemType itemType)
    {
        switch (itemType)
        {
            case ItemType.CoffeeBean:
                return GetItemCount(ItemType.CoffeeBean) < maxBeans;
            case ItemType.CoffeeCup:
                return GetItemCount(ItemType.CoffeeCup) < maxCoffees;
            case ItemType.Money:
                return true; // Money has no limit
            default:
                return false;
        }
    }
    
    public bool TryAddItem(ItemType itemType)
    {
        if (!CanCarryItem(itemType))
        {
            DebugLog($"Cannot carry more {itemType}. Current: {GetItemCount(itemType)}, Max: {GetCapacity(itemType)}");
            return false;
        }
        
        // Ensure the key exists before incrementing
        if (!inventory.ContainsKey(itemType))
        {
            inventory[itemType] = 0;
        }
        
        inventory[itemType]++;
        
        // Handle visual stacking for beans
        if (itemType == ItemType.CoffeeBean)
        {
            AddBeanToVisualStack();
            OnBeanCountChanged?.Invoke(inventory[itemType], maxBeans);
        }
        
        OnInventoryChanged?.Invoke(itemType, inventory[itemType]);
        DebugLog($"Added {itemType}. New count: {inventory[itemType]}");
        
        return true;
    }
    
    public bool TryRemoveItem(ItemType itemType)
    {
        if (GetItemCount(itemType) <= 0)
        {
            DebugLog($"Cannot remove {itemType} - inventory is empty");
            return false;
        }
        
        inventory[itemType]--;
        
        // Handle visual stacking for beans
        if (itemType == ItemType.CoffeeBean)
        {
            RemoveBeanFromVisualStack();
            OnBeanCountChanged?.Invoke(inventory[itemType], maxBeans);
        }
        
        OnInventoryChanged?.Invoke(itemType, inventory[itemType]);
        DebugLog($"Removed {itemType}. New count: {inventory[itemType]}");
        
        return true;
    }
    
    public int GetItemCount(ItemType itemType)
    {
        return inventory.ContainsKey(itemType) ? inventory[itemType] : 0;
    }
    
    public int GetCapacity(ItemType itemType)
    {
        switch (itemType)
        {
            case ItemType.CoffeeBean:
                return maxBeans;
            case ItemType.CoffeeCup:
                return maxCoffees;
            default:
                return int.MaxValue;
        }
    }
    
    // ===== VISUAL STACKING SYSTEM =====
    
    private void AddBeanToVisualStack()
    {
        if (beanStackPrefab == null)
        {
            DebugLog("Warning: Bean stack prefab not assigned!");
            return;
        }
        
        Vector3 stackPosition = GetStackPosition(visualBeanStack.Count);
        GameObject newBean = Instantiate(beanStackPrefab, stackingParent);
        newBean.transform.localPosition = stackPosition;
        newBean.transform.localRotation = GetRandomRotation();
        
        visualBeanStack.Add(newBean);
        
        // Animate the bean appearing
        StartCoroutine(AnimateBeanAppear(newBean));
        DebugLog($"Added visual bean to stack. Total: {visualBeanStack.Count}");
    }
    
    private void RemoveBeanFromVisualStack()
    {
        if (visualBeanStack.Count == 0)
            return;
        
        GameObject beanToRemove = visualBeanStack[visualBeanStack.Count - 1];
        visualBeanStack.RemoveAt(visualBeanStack.Count - 1);
        
        // Animate the bean disappearing
        StartCoroutine(AnimateBeanDisappear(beanToRemove));
        DebugLog($"Removed visual bean from stack. Remaining: {visualBeanStack.Count}");
    }
    
    private Vector3 GetStackPosition(int stackIndex)
    {
        // Stack beans vertically with slight random offset for natural look
        float randomX = Random.Range(-0.05f, 0.05f);
        float randomZ = Random.Range(-0.05f, 0.05f);
        return new Vector3(randomX, stackIndex * stackSpacing, randomZ);
    }
    
    private Quaternion GetRandomRotation()
    {
        // Small random rotation for natural look
        float randomY = Random.Range(-15f, 15f);
        return Quaternion.Euler(0, randomY, 0);
    }
    
    private System.Collections.IEnumerator AnimateBeanAppear(GameObject bean)
    {
        Vector3 targetScale = Vector3.one * 0.3f; // Small bean size
        Vector3 targetPosition = bean.transform.localPosition;
        
        // Start from above and scale from zero
        bean.transform.localScale = Vector3.zero;
        bean.transform.localPosition = targetPosition + Vector3.up * 0.5f;
        
        float elapsed = 0f;
        while (elapsed < stackAnimationDuration)
        {
            elapsed += Time.deltaTime;
            float progress = elapsed / stackAnimationDuration;
            float curveValue = stackCurve.Evaluate(progress);
            
            bean.transform.localScale = Vector3.Lerp(Vector3.zero, targetScale, curveValue);
            bean.transform.localPosition = Vector3.Lerp(
                targetPosition + Vector3.up * 0.5f, 
                targetPosition, 
                curveValue
            );
            
            yield return null;
        }
        
        bean.transform.localScale = targetScale;
        bean.transform.localPosition = targetPosition;
    }
    
    private System.Collections.IEnumerator AnimateBeanDisappear(GameObject bean)
    {
        Vector3 initialScale = bean.transform.localScale;
        Vector3 initialPosition = bean.transform.localPosition;
        
        float elapsed = 0f;
        while (elapsed < stackAnimationDuration)
        {
            elapsed += Time.deltaTime;
            float progress = elapsed / stackAnimationDuration;
            float reverseProgress = 1f - progress;
            
            bean.transform.localScale = Vector3.Lerp(Vector3.zero, initialScale, reverseProgress);
            bean.transform.localPosition = Vector3.Lerp(
                initialPosition + Vector3.up * 0.3f, 
                initialPosition, 
                reverseProgress
            );
            
            yield return null;
        }
        
        Destroy(bean);
    }
    
    // ===== UTILITY METHODS =====
    
    public bool HasItems(ItemType itemType, int count = 1)
    {
        return GetItemCount(itemType) >= count;
    }
    
    public bool IsInventoryFull(ItemType itemType)
    {
        return !CanCarryItem(itemType);
    }
    
    private void DebugLog(string message)
    {
        if (enableDebugLogs)
        {
            Debug.Log($"[PlayerInventory] {message}");
        }
    }
    
    // ===== DEBUG METHODS =====
    
    [ContextMenu("Add Test Bean")]
    private void AddTestBean()
    {
        TryAddItem(ItemType.CoffeeBean);
    }
    
    [ContextMenu("Remove Test Bean")]
    private void RemoveTestBean()
    {
        TryRemoveItem(ItemType.CoffeeBean);
    }
    
    [ContextMenu("Clear All Beans")]
    private void ClearAllBeans()
    {
        while (GetItemCount(ItemType.CoffeeBean) > 0)
        {
            TryRemoveItem(ItemType.CoffeeBean);
        }
    }
}
