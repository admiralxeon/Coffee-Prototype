# Integration Guide for New Gameplay Systems

## Overview
This guide explains how to integrate the new gameplay enhancement systems into your coffee shop game.

## New Systems Added

### 1. **UpgradeSystem.cs**
A system for purchasing upgrades that improve gameplay.

**Setup:**
- Add `UpgradeSystem` component to a GameObject in your scene
- The system initializes with default upgrades automatically
- Connect to UI to display available upgrades

**Integration Points:**
- `CoffeeMachine.cs`: Check `UpgradeSystem.Instance.GetMachineSpeedMultiplier()` when brewing
- `PlayerInventory.cs`: Check `UpgradeSystem.Instance.GetInventoryCapacityBonus()` for capacity
- `PlayerController.cs`: Apply `UpgradeSystem.Instance.GetMovementSpeedMultiplier()` to movement speed

**Example Usage:**
```csharp
// In CoffeeMachine.cs ProcessCoffee()
float processingTime = this.processingTime;
if (UpgradeSystem.Instance != null)
{
    processingTime *= UpgradeSystem.Instance.GetMachineSpeedMultiplier();
}
yield return new WaitForSeconds(processingTime);
```

### 2. **AchievementSystem.cs**
Tracks player achievements and unlocks rewards.

**Setup:**
- Add `AchievementSystem` component to a GameObject in your scene
- Achievements are initialized automatically
- Connect to UI to show achievement progress

**Integration Points:**
- `GameManager.cs`: Call `AchievementSystem.Instance.IncrementAchievement("serve_10", 1)` when customer served
- `Customer.cs`: Track when customers are served/lost
- `PlayerInventory.cs`: Track bean collection

**Example Usage:**
```csharp
// In GameManager.cs AddMoney()
if (AchievementSystem.Instance != null)
{
    AchievementSystem.Instance.IncrementAchievement("serve_10", 1);
    AchievementSystem.Instance.SetAchievementValue("money_maker", money);
}
```

### 3. **GameStatistics.cs**
Tracks detailed gameplay statistics.

**Setup:**
- Add `GameStatistics` component to a GameObject in your scene
- Automatically tracks play time and combos

**Integration Points:**
- `Customer.cs`: Call `GameStatistics.Instance.RecordCustomerServed(payment, serveTime)` when served
- `Customer.cs`: Call `GameStatistics.Instance.RecordCustomerLost()` when customer leaves angry

**Example Usage:**
```csharp
// In Customer.cs ReceiveOrder()
float serveTime = Time.time - orderStartTime;
if (GameStatistics.Instance != null)
{
    GameStatistics.Instance.RecordCustomerServed(paymentAmount, serveTime);
}
```

### 4. **CoffeeOrder.cs & CustomerOrder.cs**
Defines different coffee types and customer types.

**Integration Points:**
- `Customer.cs`: Add `CustomerOrderData` field to track what customer wants
- `CoffeeMachine.cs`: Support different coffee types with different brewing times

**Example Usage:**
```csharp
// In Customer.cs
private CustomerOrderData orderData;

void Start()
{
    orderData = CustomerOrderData.GenerateRandomOrder();
    // Use orderData.coffeeOrder.beansRequired for payment
    // Use orderData.GetPatienceTime(basePatience) for patience
}
```

### 5. **EnhancedUIManager.cs**
Provides enhanced UI feedback with animations.

**Setup:**
- Add `EnhancedUIManager` component to your UI Canvas
- Assign UI references in inspector
- Create a money popup prefab (TextMeshProUGUI with CanvasGroup)

**Integration Points:**
- `GameManager.cs`: Call `EnhancedUIManager.ShowMoneyGain(amount, position)` when money is earned
- Connect inventory display to PlayerInventory events

**Example Usage:**
```csharp
// In GameManager.cs AddMoney()
if (EnhancedUIManager.Instance != null)
{
    EnhancedUIManager.Instance.ShowMoneyGain(amount, Vector3.zero);
}
```

## Step-by-Step Integration

### Step 1: Add Core Systems
1. Create empty GameObjects in your scene:
   - "UpgradeSystem"
   - "AchievementSystem"
   - "GameStatistics"
   - "EnhancedUIManager"

2. Add the respective components to each GameObject

### Step 2: Update Existing Scripts

#### Update GameManager.cs:
```csharp
public System.Action<int> OnMoneyChanged; // Add this event

public void AddMoney(int amount)
{
    money += amount;
    customersServed++;
    
    OnMoneyChanged?.Invoke(amount); // Add this
    
    // Track statistics
    if (GameStatistics.Instance != null)
    {
        GameStatistics.Instance.RecordCustomerServed(amount, 0f);
    }
    
    // Track achievements
    if (AchievementSystem.Instance != null)
    {
        AchievementSystem.Instance.IncrementAchievement("serve_10", 1);
        AchievementSystem.Instance.SetAchievementValue("money_maker", money);
    }
    
    // Show UI feedback
    if (EnhancedUIManager.Instance != null)
    {
        EnhancedUIManager.Instance.ShowMoneyGain(amount, Vector3.zero);
    }
    
    UpdateUI();
}
```

#### Update Customer.cs:
Add these fields:
```csharp
private CustomerOrderData orderData;
private float orderStartTime;
```

In `Start()`:
```csharp
orderData = CustomerOrderData.GenerateRandomOrder();
orderStartTime = Time.time;
patienceTime = orderData.GetPatienceTime(patienceTime);
paymentAmount = orderData.GetFinalPayment();
```

In `ReceiveOrder()`:
```csharp
float serveTime = Time.time - orderStartTime;
if (GameStatistics.Instance != null)
{
    GameStatistics.Instance.RecordCustomerServed(paymentAmount, serveTime);
}
```

#### Update CoffeeMachine.cs:
In `ProcessCoffee()`:
```csharp
float actualProcessingTime = processingTime;
if (UpgradeSystem.Instance != null)
{
    actualProcessingTime *= UpgradeSystem.Instance.GetMachineSpeedMultiplier();
}
yield return new WaitForSeconds(actualProcessingTime);
```

### Step 3: Create UI

1. **Upgrade Shop UI:**
   - Create a panel with upgrade buttons
   - Display upgrade name, description, cost
   - Call `UpgradeSystem.Instance.PurchaseUpgrade(upgrade)` on click

2. **Achievement UI:**
   - Create a panel showing all achievements
   - Display progress bars for each achievement
   - Show unlocked achievements with special styling

3. **Statistics Panel:**
   - Use `EnhancedUIManager` stats panel
   - Or create custom panel using `GameStatistics.Instance` getters

4. **Money Popup Prefab:**
   - Create a TextMeshProUGUI GameObject
   - Add CanvasGroup component
   - Set as prefab and assign to EnhancedUIManager

### Step 4: Testing

1. Test upgrade purchases
2. Test achievement unlocking
3. Test statistics tracking
4. Test UI animations
5. Test combo system

## Optional Enhancements

### Add Upgrade Shop UI:
Create a simple shop UI that shows available upgrades:
```csharp
public class UpgradeShopUI : MonoBehaviour
{
    [SerializeField] private Transform upgradeContainer;
    [SerializeField] private GameObject upgradeButtonPrefab;
    
    void Start()
    {
        RefreshUpgrades();
    }
    
    void RefreshUpgrades()
    {
        foreach (var upgrade in UpgradeSystem.Instance.GetAvailableUpgrades())
        {
            // Create button for each upgrade
            // Display upgrade info
            // Add purchase button
        }
    }
}
```

### Add Achievement Notification:
Create a popup that shows when achievements unlock:
```csharp
public class AchievementNotification : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI titleText;
    [SerializeField] private TextMeshProUGUI descriptionText;
    
    public void Show(Achievement achievement)
    {
        titleText.text = achievement.title;
        descriptionText.text = achievement.description;
        // Animate in
    }
}
```

## Notes

- All systems use Singleton pattern for easy access
- Statistics and achievements are automatically saved/loaded
- Systems are designed to work independently - you can use some without others
- Make sure to initialize systems before other scripts try to use them (use Script Execution Order if needed)

## Next Steps

1. Implement customer order system (different coffee types)
2. Add visual variety to customers
3. Create upgrade shop UI
4. Add achievement gallery UI
5. Polish visual effects and animations

